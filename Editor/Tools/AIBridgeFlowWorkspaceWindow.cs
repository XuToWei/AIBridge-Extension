using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AIBridge.Editor
{
    /// <summary>
    /// Workspace for authoring and launching reusable and temporary .flow.txt scripts.
    /// </summary>
    public class AIBridgeFlowWorkspaceWindow : EditorWindow
    {
        private const double StatusRefreshIntervalSeconds = 1.0d;
        private const float ScriptListWidth = 280f;

        private readonly List<FlowWorkspaceScriptInfo> _scripts = new List<FlowWorkspaceScriptInfo>();

        private Vector2 _scriptListScrollPosition;
        private Vector2 _editorScrollPosition;
        private FlowWorkspaceSourceFilter _sourceFilter = FlowWorkspaceSourceFilter.All;
        private FlowWorkspaceScriptInfo _selectedScript;
        private FlowWorkspaceStatusSnapshot _statusSnapshot;
        private string _savedContent = string.Empty;
        private string _editorContent = string.Empty;
        private string _feedbackMessage;
        private MessageType _feedbackType = MessageType.None;
        private double _lastStatusRefreshTime;

        [MenuItem("AIBridge/Flow Workspace")]
        private static void OpenWindow()
        {
            var window = GetWindow<AIBridgeFlowWorkspaceWindow>();
            window.titleContent = new GUIContent("Flow Workspace");
            window.minSize = new Vector2(900f, 600f);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshWorkspace(true);
        }

        private void OnFocus()
        {
            RefreshStatus();
        }

        private void OnInspectorUpdate()
        {
            var now = EditorApplication.timeSinceStartup;
            if (now - _lastStatusRefreshTime >= StatusRefreshIntervalSeconds)
            {
                RefreshStatus();
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(8);

            DrawToolbar();
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            DrawScriptListPanel();
            EditorGUILayout.Space(8);
            DrawEditorPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("AI Bridge Flow Workspace", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Manage reusable flows in Flows/ and temporary flows in AIBridgeCache/flow-temp/. Execute starts the cached CLI immediately and the latest run/job status below updates from cache files.",
                MessageType.Info);

            if (!string.IsNullOrWhiteSpace(_feedbackMessage))
            {
                EditorGUILayout.HelpBox(_feedbackMessage, _feedbackType);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _sourceFilter = (FlowWorkspaceSourceFilter)EditorGUILayout.EnumPopup("Source", _sourceFilter, GUILayout.Width(220f));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshScriptList(true);
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(90f)))
            {
                RefreshWorkspace(true);
            }

            if (GUILayout.Button("New Reusable", GUILayout.Width(120f)))
            {
                CreateScript(FlowWorkspaceScriptSource.Reusable);
            }

            if (GUILayout.Button("New Temporary", GUILayout.Width(120f)))
            {
                CreateScript(FlowWorkspaceScriptSource.Temporary);
            }

            if (GUILayout.Button("Open Flows", GUILayout.Width(100f)))
            {
                FlowWorkspaceUtility.RevealReusableFlowsDirectory();
            }

            if (GUILayout.Button("Open Temp", GUILayout.Width(100f)))
            {
                FlowWorkspaceUtility.RevealTemporaryFlowsDirectory();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawScriptListPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ScriptListWidth));
            EditorGUILayout.LabelField("Scripts", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_scripts.Count + " item(s)", EditorStyles.miniLabel);
            EditorGUILayout.Space(4);

            _scriptListScrollPosition = EditorGUILayout.BeginScrollView(_scriptListScrollPosition, GUI.skin.box, GUILayout.ExpandHeight(true));

            if (_scripts.Count == 0)
            {
                EditorGUILayout.HelpBox("No .flow.txt scripts found for the current source filter.", MessageType.None);
            }

            for (var i = 0; i < _scripts.Count; i++)
            {
                DrawScriptListEntry(_scripts[i]);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawScriptListEntry(FlowWorkspaceScriptInfo script)
        {
            var isSelected = _selectedScript != null && string.Equals(_selectedScript.AbsolutePath, script.AbsolutePath, StringComparison.OrdinalIgnoreCase);
            var buttonLabel = (isSelected ? "▶ " : string.Empty) + script.RootRelativePath;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(buttonLabel, EditorStyles.miniButtonLeft))
            {
                SelectScript(script);
            }

            if (GUILayout.Button("Execute", EditorStyles.miniButtonRight, GUILayout.Width(64f)))
            {
                ExecuteScriptFromList(script);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(script.Source == FlowWorkspaceScriptSource.Reusable ? "Reusable" : "Temporary", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawEditorPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (_selectedScript == null)
            {
                EditorGUILayout.HelpBox("Select or create a .flow.txt script to begin editing.", MessageType.Info);
                DrawStatusSection();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField(GetSelectedTitle(), EditorStyles.boldLabel);
            EditorGUILayout.TextField("Path", _selectedScript.ProjectRelativePath);
            EditorGUILayout.TextField("Source", _selectedScript.Source == FlowWorkspaceScriptSource.Reusable ? "Reusable" : "Temporary");
            EditorGUILayout.Space(4);

            DrawActionBar();
            EditorGUILayout.Space(6);

            _editorScrollPosition = EditorGUILayout.BeginScrollView(_editorScrollPosition, GUI.skin.box, GUILayout.ExpandHeight(true));
            var updatedContent = EditorGUILayout.TextArea(_editorContent, GUILayout.ExpandHeight(true));
            if (!string.Equals(updatedContent, _editorContent, StringComparison.Ordinal))
            {
                _editorContent = updatedContent;
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            DrawStatusSection();
            EditorGUILayout.EndVertical();
        }

        private void DrawActionBar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!IsDirty);
            if (GUILayout.Button("Save", GUILayout.Height(26f)))
            {
                SaveCurrentScript();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Execute", GUILayout.Height(26f)))
            {
                ExecuteSelectedScript();
            }

            if (GUILayout.Button("Reveal", GUILayout.Height(26f)))
            {
                FlowWorkspaceUtility.Reveal(_selectedScript);
            }

            if (GUILayout.Button("Duplicate", GUILayout.Height(26f)))
            {
                DuplicateSelectedScript();
            }

            EditorGUI.BeginDisabledGroup(_selectedScript.Source != FlowWorkspaceScriptSource.Temporary);
            if (GUILayout.Button("Promote", GUILayout.Height(26f)))
            {
                PromoteSelectedScript();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Delete", GUILayout.Height(26f)))
            {
                DeleteSelectedScript();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusSection()
        {
            EditorGUILayout.LabelField("Latest Status", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(FormatFlowRunStatus(), GetFlowRunMessageType());
            EditorGUILayout.HelpBox(FormatWorkflowJobStatus(), GetWorkflowJobMessageType());
        }

        private void RefreshWorkspace(bool preserveSelection)
        {
            FlowWorkspaceUtility.EnsureWorkspaceDirectories();
            RefreshScriptList(preserveSelection);
            RefreshStatus();
        }

        private void RefreshScriptList(bool preserveSelection)
        {
            var selectedPath = preserveSelection && _selectedScript != null ? _selectedScript.AbsolutePath : null;
            _scripts.Clear();
            _scripts.AddRange(FlowWorkspaceUtility.DiscoverScripts(_sourceFilter));

            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                for (var i = 0; i < _scripts.Count; i++)
                {
                    if (string.Equals(_scripts[i].AbsolutePath, selectedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _selectedScript = _scripts[i];
                        return;
                    }
                }
            }

            if (_scripts.Count > 0)
            {
                if (_selectedScript == null || !preserveSelection || !string.IsNullOrWhiteSpace(selectedPath))
                {
                    LoadSelectedScript(_scripts[0]);
                }
            }
            else
            {
                _selectedScript = null;
                _savedContent = string.Empty;
                _editorContent = string.Empty;
            }
        }

        private void RefreshStatus()
        {
            _statusSnapshot = FlowWorkspaceUtility.LoadStatusSnapshot();
            _lastStatusRefreshTime = EditorApplication.timeSinceStartup;
        }

        private void CreateScript(FlowWorkspaceScriptSource source)
        {
            if (!TryResolveDirtyChanges("creating a new script"))
            {
                return;
            }

            var script = FlowWorkspaceUtility.CreateScript(source);
            if (script == null)
            {
                return;
            }

            SetFeedback("Created " + script.ProjectRelativePath, MessageType.Info);
            RefreshScriptList(false);
            SelectScriptByPath(script.AbsolutePath);
        }

        private void SelectScript(FlowWorkspaceScriptInfo script)
        {
            if (script == null)
            {
                return;
            }

            if (_selectedScript != null && string.Equals(_selectedScript.AbsolutePath, script.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!TryResolveDirtyChanges("switching scripts"))
            {
                return;
            }

            LoadSelectedScript(script);
        }

        private void LoadSelectedScript(FlowWorkspaceScriptInfo script)
        {
            _selectedScript = script;
            _savedContent = script != null ? FlowWorkspaceUtility.ReadScript(script.AbsolutePath) : string.Empty;
            _editorContent = _savedContent;
            _editorScrollPosition = Vector2.zero;
        }

        private void SelectScriptByPath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return;
            }

            for (var i = 0; i < _scripts.Count; i++)
            {
                if (string.Equals(_scripts[i].AbsolutePath, absolutePath, StringComparison.OrdinalIgnoreCase))
                {
                    LoadSelectedScript(_scripts[i]);
                    return;
                }
            }
        }

        private bool SaveCurrentScript()
        {
            if (_selectedScript == null)
            {
                return false;
            }

            try
            {
                FlowWorkspaceUtility.WriteScript(_selectedScript.AbsolutePath, _editorContent);
                _savedContent = _editorContent;
                SetFeedback("Saved " + _selectedScript.ProjectRelativePath, MessageType.Info);
                return true;
            }
            catch (Exception ex)
            {
                SetFeedback("Failed to save flow script: " + ex.Message, MessageType.Error);
                return false;
            }
        }

        private void ExecuteSelectedScript()
        {
            if (_selectedScript == null)
            {
                return;
            }

            if (IsDirty && !SaveCurrentScript())
            {
                return;
            }

            var result = FlowWorkspaceUtility.ExecuteScriptAsync(_selectedScript);
            SetFeedback(result.Message, result.Success ? MessageType.Info : MessageType.Error);
            RefreshStatus();
        }

        private void ExecuteScriptFromList(FlowWorkspaceScriptInfo script)
        {
            if (script == null)
            {
                return;
            }

            if (_selectedScript == null || !string.Equals(_selectedScript.AbsolutePath, script.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveDirtyChanges("executing another script"))
                {
                    return;
                }

                LoadSelectedScript(script);
            }

            ExecuteSelectedScript();
        }

        private void DuplicateSelectedScript()
        {
            if (_selectedScript == null)
            {
                return;
            }

            if (IsDirty && !SaveCurrentScript())
            {
                return;
            }

            try
            {
                var duplicate = FlowWorkspaceUtility.DuplicateScript(_selectedScript);
                if (duplicate == null)
                {
                    SetFeedback("Failed to duplicate the selected flow script.", MessageType.Error);
                    return;
                }

                RefreshScriptList(false);
                SelectScriptByPath(duplicate.AbsolutePath);
                SetFeedback("Duplicated to " + duplicate.ProjectRelativePath, MessageType.Info);
            }
            catch (Exception ex)
            {
                SetFeedback("Failed to duplicate flow script: " + ex.Message, MessageType.Error);
            }
        }

        private void PromoteSelectedScript()
        {
            if (_selectedScript == null || _selectedScript.Source != FlowWorkspaceScriptSource.Temporary)
            {
                return;
            }

            if (IsDirty && !SaveCurrentScript())
            {
                return;
            }

            try
            {
                var promoted = FlowWorkspaceUtility.PromoteScript(_selectedScript);
                if (promoted == null)
                {
                    SetFeedback("Failed to promote the selected temporary flow.", MessageType.Error);
                    return;
                }

                RefreshScriptList(false);
                SelectScriptByPath(promoted.AbsolutePath);
                SetFeedback("Promoted to " + promoted.ProjectRelativePath, MessageType.Info);
            }
            catch (Exception ex)
            {
                SetFeedback("Failed to promote flow script: " + ex.Message, MessageType.Error);
            }
        }

        private void DeleteSelectedScript()
        {
            if (_selectedScript == null)
            {
                return;
            }

            var confirmed = EditorUtility.DisplayDialog(
                "Delete Flow Script",
                "Delete " + _selectedScript.ProjectRelativePath + "?",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            try
            {
                var deletedPath = _selectedScript.ProjectRelativePath;
                FlowWorkspaceUtility.DeleteScript(_selectedScript);
                RefreshScriptList(false);
                SetFeedback("Deleted " + deletedPath, MessageType.Info);
            }
            catch (Exception ex)
            {
                SetFeedback("Failed to delete flow script: " + ex.Message, MessageType.Error);
            }
        }

        private bool TryResolveDirtyChanges(string action)
        {
            if (!IsDirty || _selectedScript == null)
            {
                return true;
            }

            var decision = EditorUtility.DisplayDialogComplex(
                "Unsaved Flow Changes",
                "Save changes to " + _selectedScript.ProjectRelativePath + " before " + action + "?",
                "Save",
                "Cancel",
                "Don't Save");

            if (decision == 0)
            {
                return SaveCurrentScript();
            }

            return decision == 2;
        }

        private string GetSelectedTitle()
        {
            if (_selectedScript == null)
            {
                return "Flow Editor";
            }

            return IsDirty ? _selectedScript.Name + " *" : _selectedScript.Name;
        }

        private void SetFeedback(string message, MessageType type)
        {
            _feedbackMessage = message;
            _feedbackType = type;
        }

        private MessageType GetFlowRunMessageType()
        {
            var status = _statusSnapshot != null && _statusSnapshot.LatestFlowRun != null
                ? _statusSnapshot.LatestFlowRun.Status
                : null;

            if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                return MessageType.Error;
            }

            if (string.Equals(status, "running", StringComparison.OrdinalIgnoreCase))
            {
                return MessageType.Warning;
            }

            if (string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase) || string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return MessageType.Info;
            }

            return MessageType.None;
        }

        private MessageType GetWorkflowJobMessageType()
        {
            var job = _statusSnapshot != null ? _statusSnapshot.LatestWorkflowJob : null;
            if (job == null)
            {
                return MessageType.None;
            }

            if (string.Equals(job.status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                return MessageType.Error;
            }

            if (string.Equals(job.status, "running", StringComparison.OrdinalIgnoreCase))
            {
                return MessageType.Warning;
            }

            if (string.Equals(job.status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return MessageType.Info;
            }

            return MessageType.None;
        }

        private string FormatFlowRunStatus()
        {
            var run = _statusSnapshot != null ? _statusSnapshot.LatestFlowRun : null;
            if (run == null)
            {
                return "Latest Flow Run\nNo cached flow run found yet.";
            }

            var sourcePath = string.IsNullOrWhiteSpace(run.SourceFilePath)
                ? "Unknown"
                : FlowWorkspaceUtility.ToProjectRelativePath(run.SourceFilePath);

            return "Latest Flow Run\n"
                   + "Run: " + ValueOrFallback(run.RunId) + "\n"
                   + "Flow: " + ValueOrFallback(run.FlowName) + "\n"
                   + "Status: " + ValueOrFallback(run.Status) + "\n"
                   + "Current Step: " + ValueOrFallback(run.CurrentStepId) + "\n"
                   + "Source: " + sourcePath + "\n"
                   + "Started: " + ValueOrFallback(run.StartedAtUtc) + "\n"
                   + "Completed: " + ValueOrFallback(run.CompletedAtUtc) + "\n"
                   + "Error: " + ValueOrFallback(run.Error);
        }

        private string FormatWorkflowJobStatus()
        {
            var job = _statusSnapshot != null ? _statusSnapshot.LatestWorkflowJob : null;
            if (job == null)
            {
                return "Latest Workflow Job\nNo cached workflow job found yet.";
            }

            return "Latest Workflow Job\n"
                   + "Job: " + ValueOrFallback(job.jobId) + "\n"
                   + "Type: " + ValueOrFallback(job.jobType) + "\n"
                   + "Status: " + ValueOrFallback(job.status) + "\n"
                   + "Phase: " + ValueOrFallback(job.phase) + "\n"
                   + "Started: " + ValueOrFallback(job.startedAtUtc) + "\n"
                   + "Updated: " + ValueOrFallback(job.updatedAtUtc) + "\n"
                   + "Completed: " + ValueOrFallback(job.completedAtUtc) + "\n"
                   + "Error: " + ValueOrFallback(job.error);
        }

        private string ValueOrFallback(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private bool IsDirty => !string.Equals(_editorContent, _savedContent, StringComparison.Ordinal);
    }
}
