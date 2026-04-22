using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AIBridge.Editor
{
    /// <summary>
    /// Main settings window for AI Bridge.
    /// </summary>
    public class AIBridgeSettingsWindow : EditorWindow
    {
        // 页签枚举
        private enum TabType
        {
            BasicSettings,    // 基础设置
            GifSettings,      // GIF 设置
            DirectoryInfo,    // 目录信息
            SkillInstall,     // Skills 安装
            Actions           // 操作
        }

        private sealed class AssistantIntegrationSelectionState
        {
            public AssistantIntegrationTarget Target { get; set; }
            public bool IsDetected { get; set; }
            public string Detail { get; set; }
            public bool IsSelected { get; set; }
        }

        private Vector2 _scrollPosition;
        private bool _bridgeEnabled;
        private bool _debugLogging;
        private List<AssistantIntegrationSelectionState> _assistantIntegrationSelections;
        private TabType _currentTab = TabType.BasicSettings; // 当前选中的页签

        // GIF Settings
        private int _gifFrameCount;
        private int _gifFps;
        private float _gifScale;
        private int _gifColorCount;
        private float _gifStartDelay;

        [MenuItem("AIBridge/Settings")]
        private static void OpenWindow()
        {
            var window = GetWindow<AIBridgeSettingsWindow>();
            window.titleContent = new GUIContent("AI Bridge Settings");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
            LoadAssistantIntegrationSelections();
        }

        private void LoadSettings()
        {
            _bridgeEnabled = AIBridge.Enabled;
            _debugLogging = AIBridgeLogger.DebugEnabled;

            _gifFrameCount = GifRecorderSettings.DefaultFrameCount;
            _gifFps = GifRecorderSettings.DefaultFps;
            _gifScale = GifRecorderSettings.DefaultScale;
            _gifColorCount = GifRecorderSettings.DefaultColorCount;
            _gifStartDelay = GifRecorderSettings.DefaultStartDelay;
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(5);

            // 绘制页签工具栏
            DrawTabToolbar();
            EditorGUILayout.Space(5);

            // 绘制当前页签内容
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_currentTab)
            {
                case TabType.BasicSettings:
                    DrawBridgeSettings();
                    break;
                case TabType.GifSettings:
                    DrawGifSettings();
                    break;
                case TabType.DirectoryInfo:
                    DrawDirectoryInfo();
                    break;
                case TabType.SkillInstall:
                    DrawAssistantIntegrationSettings();
                    break;
                case TabType.Actions:
                    DrawActions();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTabToolbar()
        {
            var tabNames = new[] { "基础设置", "GIF 设置", "目录信息", "Skills 安装", "操作" };
            _currentTab = (TabType)GUILayout.Toolbar((int)_currentTab, tabNames);
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("AI Bridge Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "AI Bridge enables communication between AI assistants and Unity Editor.\n" +
                "Use F12 to capture screenshots and F11 to record GIFs in Play mode.",
                MessageType.Info);
        }

        private void DrawBridgeSettings()
        {
            EditorGUILayout.LabelField("Bridge Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _bridgeEnabled = EditorGUILayout.Toggle("Bridge Enabled", _bridgeEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                AIBridge.Enabled = _bridgeEnabled;
            }

            EditorGUI.BeginChangeCheck();
            _debugLogging = EditorGUILayout.Toggle("Debug Logging", _debugLogging);
            if (EditorGUI.EndChangeCheck())
            {
                AIBridgeLogger.DebugEnabled = _debugLogging;
            }
        }

        private void DrawGifSettings()
        {
            EditorGUILayout.LabelField("GIF Recording Settings (F11)", EditorStyles.boldLabel);

            _gifFrameCount = EditorGUILayout.IntSlider("Frame Count", _gifFrameCount, 10, GifRecorder.MaxFrameCount);
            EditorGUILayout.LabelField($"  Duration: {(float)_gifFrameCount / _gifFps:F1}s", EditorStyles.miniLabel);

            _gifFps = EditorGUILayout.IntSlider("FPS", _gifFps, 10, 30);

            _gifScale = EditorGUILayout.Slider("Scale", _gifScale, 0.25f, 1f);
            EditorGUILayout.LabelField($"  Output: {(int)(1920 * _gifScale)}x{(int)(1080 * _gifScale)} (at 1080p)", EditorStyles.miniLabel);

            _gifColorCount = EditorGUILayout.IntSlider("Color Count", _gifColorCount, 64, 256);

            _gifStartDelay = EditorGUILayout.Slider("Start Delay (s)", _gifStartDelay, 0f, 5f);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save GIF Settings"))
            {
                GifRecorderSettings.DefaultFrameCount = _gifFrameCount;
                GifRecorderSettings.DefaultFps = _gifFps;
                GifRecorderSettings.DefaultScale = _gifScale;
                GifRecorderSettings.DefaultColorCount = _gifColorCount;
                GifRecorderSettings.DefaultStartDelay = _gifStartDelay;
                Debug.Log("[AIBridge] GIF settings saved.");
            }

            if (GUILayout.Button("Reset to Defaults"))
            {
                GifRecorderSettings.ResetToDefaults();
                LoadSettings();
                Debug.Log("[AIBridge] GIF settings reset to defaults.");
            }
            EditorGUILayout.EndHorizontal();

            if (GifRecorder.IsRecording)
            {
                EditorGUILayout.HelpBox("GIF Recording in progress...", MessageType.Warning);
            }
        }

        private void DrawDirectoryInfo()
        {
            EditorGUILayout.LabelField("Directory Information", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Bridge Directory", AIBridge.BridgeDirectory);
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                if (!Directory.Exists(AIBridge.BridgeDirectory))
                {
                    Directory.CreateDirectory(AIBridge.BridgeDirectory);
                }
                EditorUtility.RevealInFinder(AIBridge.BridgeDirectory);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Screenshots Directory", ScreenshotHelper.ScreenshotsDir);
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                ScreenshotHelper.EnsureScreenshotsDirectory();
                EditorUtility.RevealInFinder(ScreenshotHelper.ScreenshotsDir);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Process Commands Now", GUILayout.Height(30)))
            {
                AIBridge.ProcessCommandsNow();
                Debug.Log("[AIBridge] Commands processed.");
            }

            if (GUILayout.Button("Clear Screenshot Cache", GUILayout.Height(30)))
            {
                ClearScreenshotCache();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Hotkeys", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "F12 - Capture Screenshot (Play mode only)\n" +
                "F11 - Start/Stop GIF Recording (Play mode only)",
                MessageType.None);
        }

        private void DrawAssistantIntegrationSettings()
        {
            EditorGUILayout.LabelField("Skill Installation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select which supported AI tools should receive AIBridge skill installation. Detected tools are selected by default on first use.",
                MessageType.Info);

            if (_assistantIntegrationSelections == null)
            {
                LoadAssistantIntegrationSelections();
            }

            foreach (var selection in _assistantIntegrationSelections)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.BeginChangeCheck();
                var selected = EditorGUILayout.ToggleLeft(selection.Target.DisplayName, selection.IsSelected);
                if (EditorGUI.EndChangeCheck())
                {
                    selection.IsSelected = selected;
                    AssistantIntegrationSelectionSettings.SetSelected(selection.Target.Id, selected);
                }

                var status = selection.IsDetected ? "Detected" : "Not detected";
                EditorGUILayout.LabelField(status + ": " + selection.Detail, EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select Detected"))
            {
                SelectDetectedTools();
            }

            if (GUILayout.Button("Select All"))
            {
                SelectAllTools();
            }

            if (GUILayout.Button("Clear"))
            {
                ClearToolSelection();
            }

            EditorGUILayout.EndHorizontal();

            var selectedCount = _assistantIntegrationSelections.Count(selection => selection.IsSelected);
            EditorGUILayout.LabelField($"{selectedCount} tool(s) selected", EditorStyles.miniLabel);

            EditorGUI.BeginDisabledGroup(selectedCount == 0);
            if (GUILayout.Button("Install Selected Integrations", GUILayout.Height(30)))
            {
                InstallSelectedTools();
            }
            EditorGUI.EndDisabledGroup();

            if (selectedCount == 0)
            {
                EditorGUILayout.HelpBox("Select at least one tool to install AIBridge integrations.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // 安装 AGENTS.md 按钮
            EditorGUILayout.LabelField("AGENTS 工作流规范", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "安装 AGENTS.md 工作流规范文档到项目根目录，方便初次使用者更好地使用 AIBridge。\n安装后会自动执行一次 Skills 安装。",
                MessageType.Info);

            if (GUILayout.Button("安装 AGENTS.md 到项目根目录", GUILayout.Height(30)))
            {
                InstallAgentsFile();
            }
        }

        private void LoadAssistantIntegrationSelections()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var targets = AssistantIntegrationRegistry.GetTargets();
            var selections = AssistantIntegrationSelectionSettings.LoadSelections(projectRoot, targets);

            _assistantIntegrationSelections = new List<AssistantIntegrationSelectionState>(targets.Count);
            foreach (var target in targets)
            {
                var detection = AssistantIntegrationDetector.Detect(projectRoot, target);
                _assistantIntegrationSelections.Add(new AssistantIntegrationSelectionState
                {
                    Target = target,
                    IsDetected = detection.IsDetected,
                    Detail = detection.Detail,
                    IsSelected = selections.TryGetValue(target.Id, out var isSelected) && isSelected
                });
            }
        }

        private void SelectDetectedTools()
        {
            foreach (var selection in _assistantIntegrationSelections)
            {
                selection.IsSelected = selection.IsDetected;
                AssistantIntegrationSelectionSettings.SetSelected(selection.Target.Id, selection.IsSelected);
            }
        }

        private void SelectAllTools()
        {
            foreach (var selection in _assistantIntegrationSelections)
            {
                selection.IsSelected = true;
                AssistantIntegrationSelectionSettings.SetSelected(selection.Target.Id, true);
            }
        }

        private void ClearToolSelection()
        {
            foreach (var selection in _assistantIntegrationSelections)
            {
                selection.IsSelected = false;
                AssistantIntegrationSelectionSettings.SetSelected(selection.Target.Id, false);
            }
        }

        private void InstallSelectedTools()
        {
            var selectedTargetIds = _assistantIntegrationSelections
                .Where(selection => selection.IsSelected)
                .Select(selection => selection.Target.Id)
                .ToArray();

            SkillInstaller.ManualInstallSelected(selectedTargetIds);
            LoadAssistantIntegrationSelections();
        }

        private void ClearScreenshotCache()
        {
            var screenshotsDir = ScreenshotHelper.ScreenshotsDir;
            if (Directory.Exists(screenshotsDir))
            {
                var files = Directory.GetFiles(screenshotsDir);
                int count = 0;
                foreach (var file in files)
                {
                    if (Path.GetFileName(file) != ".gitignore")
                    {
                        try
                        {
                            File.Delete(file);
                            count++;
                        }
                        catch
                        {
                            // Ignore deletion errors
                        }
                    }
                }
                Debug.Log($"[AIBridge] Cleared {count} files from screenshot cache.");
            }
        }

        /// <summary>
        /// 获取 AGENTS.md 源文件路径（兼容 Packages 和 PackageCache）
        /// </summary>
        private static string GetSourceAgentsPath()
        {
            const string PACKAGE_NAME = "cn.lys.aibridge";
            const string AGENTS_FILE_NAME = "AGENTS.md";
            var projectRoot = Path.GetDirectoryName(Application.dataPath);

            // 方法 1: 直接从 Packages 目录查找（本地/嵌入式包）
            var directPath = Path.Combine(projectRoot, "Packages", PACKAGE_NAME, AGENTS_FILE_NAME);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            // 方法 2: 使用 PackageInfo 解析路径（git/registry 包）
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{PACKAGE_NAME}");
            if (packageInfo != null)
            {
                var packagePath = Path.Combine(packageInfo.resolvedPath, AGENTS_FILE_NAME);
                if (File.Exists(packagePath))
                {
                    return packagePath;
                }
            }

            return null;
        }

        /// <summary>
        /// 安装 AGENTS.md 到项目根目录
        /// </summary>
        private void InstallAgentsFile()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var targetPath = Path.Combine(projectRoot, "AGENTS.md");

            // 检查目标文件是否已存在
            if (File.Exists(targetPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "确认覆盖",
                    "项目根目录已存在 AGENTS.md 文件，是否覆盖？",
                    "覆盖",
                    "取消"))
                {
                    return;
                }
            }

            // 获取源文件路径
            var sourcePath = GetSourceAgentsPath();
            if (string.IsNullOrEmpty(sourcePath))
            {
                EditorUtility.DisplayDialog(
                    "安装失败",
                    "未找到 AGENTS.md 源文件。\n预期位置：Packages/cn.lys.aibridge/AGENTS.md",
                    "确定");
                return;
            }

            try
            {
                // 拷贝文件
                File.Copy(sourcePath, targetPath, true);
                Debug.Log($"[AIBridge] AGENTS.md 已安装到: {targetPath}");

                // 自动执行 Skills 安装
                InstallSelectedTools();

                EditorUtility.DisplayDialog(
                    "安装成功",
                    $"AGENTS.md 已成功安装到项目根目录。\n\n已自动执行 Skills 安装。",
                    "确定");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "安装失败",
                    $"拷贝 AGENTS.md 时发生错误：\n{ex.Message}",
                    "确定");
            }
        }
    }
}
