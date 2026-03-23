using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AIBridge.Internal.Json;
using UnityEditor;
using UnityEngine;

namespace AIBridge.Editor
{
    internal enum FlowWorkspaceSourceFilter
    {
        Reusable,
        Temporary,
        All
    }

    internal enum FlowWorkspaceScriptSource
    {
        Reusable,
        Temporary
    }

    internal sealed class FlowWorkspaceScriptInfo
    {
        public string Name;
        public string AbsolutePath;
        public string ProjectRelativePath;
        public string RootRelativePath;
        public FlowWorkspaceScriptSource Source;
    }

    internal sealed class FlowWorkspaceRunStatus
    {
        public string RunId;
        public string FlowName;
        public string SourceFilePath;
        public string Status;
        public string CurrentStepId;
        public string StartedAtUtc;
        public string CompletedAtUtc;
        public string Error;
    }

    internal sealed class FlowWorkspaceStatusSnapshot
    {
        public FlowWorkspaceRunStatus LatestFlowRun;
        public WorkflowJobState LatestWorkflowJob;
    }

    internal sealed class FlowWorkspaceExecutionResult
    {
        public bool Success;
        public string Message;
    }

    internal static class FlowWorkspaceUtility
    {
        private const string FlowExtension = ".flow.txt";
        private const string TempGitIgnoreContent = "*\n!.gitignore\n";

        public static string ProjectRoot
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AIBridge.BridgeDirectory))
                {
                    var parent = Directory.GetParent(AIBridge.BridgeDirectory);
                    if (parent != null)
                    {
                        return parent.FullName;
                    }
                }

                return Path.GetDirectoryName(Application.dataPath);
            }
        }

        public static string ReusableFlowsDirectory => Path.Combine(ProjectRoot, "Flows");

        public static string TemporaryFlowsDirectory => Path.Combine(AIBridge.BridgeDirectory, "flow-temp");

        public static string CliDirectory => Path.Combine(AIBridge.BridgeDirectory, "CLI");

        public static string FlowRunsDirectory => Path.Combine(AIBridge.BridgeDirectory, "flow-runs");

        public static void EnsureWorkspaceDirectories()
        {
            Directory.CreateDirectory(ReusableFlowsDirectory);
            Directory.CreateDirectory(TemporaryFlowsDirectory);

            var gitIgnorePath = Path.Combine(TemporaryFlowsDirectory, ".gitignore");
            if (!File.Exists(gitIgnorePath))
            {
                File.WriteAllText(gitIgnorePath, TempGitIgnoreContent);
            }
        }

        public static List<FlowWorkspaceScriptInfo> DiscoverScripts(FlowWorkspaceSourceFilter filter)
        {
            EnsureWorkspaceDirectories();

            var scripts = new List<FlowWorkspaceScriptInfo>();

            if (filter == FlowWorkspaceSourceFilter.Reusable || filter == FlowWorkspaceSourceFilter.All)
            {
                AddScriptsFromRoot(ReusableFlowsDirectory, FlowWorkspaceScriptSource.Reusable, scripts);
            }

            if (filter == FlowWorkspaceSourceFilter.Temporary || filter == FlowWorkspaceSourceFilter.All)
            {
                AddScriptsFromRoot(TemporaryFlowsDirectory, FlowWorkspaceScriptSource.Temporary, scripts);
            }

            scripts.Sort(CompareScripts);
            return scripts;
        }

        public static string ReadScript(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public static void WriteScript(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content ?? string.Empty);
        }

        public static FlowWorkspaceScriptInfo CreateScript(FlowWorkspaceScriptSource source)
        {
            EnsureWorkspaceDirectories();

            var targetRoot = source == FlowWorkspaceScriptSource.Reusable ? ReusableFlowsDirectory : TemporaryFlowsDirectory;
            var defaultName = source == FlowWorkspaceScriptSource.Reusable ? "new_reusable.flow.txt" : "new_temporary.flow.txt";
            var selectedPath = EditorUtility.SaveFilePanel("Create Flow Script", targetRoot, defaultName, "txt");
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return null;
            }

            selectedPath = EnsureFlowScriptPath(selectedPath);
            if (!IsPathUnderRoot(selectedPath, targetRoot))
            {
                EditorUtility.DisplayDialog("Invalid Flow Script Location", "Flow scripts must be created inside the selected root directory.", "OK");
                return null;
            }

            if (File.Exists(selectedPath))
            {
                var overwrite = EditorUtility.DisplayDialog(
                    "Overwrite Flow Script",
                    "A flow script with that name already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel");

                if (!overwrite)
                {
                    return null;
                }
            }

            WriteScript(selectedPath, BuildDefaultFlowContent(selectedPath));
            return BuildScriptInfo(selectedPath, source, targetRoot);
        }

        public static FlowWorkspaceScriptInfo DuplicateScript(FlowWorkspaceScriptInfo script)
        {
            if (script == null || !File.Exists(script.AbsolutePath))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(script.AbsolutePath);
            var duplicatePath = GetUniqueFlowPath(directory, GetFlowBaseName(script.Name) + "_copy");
            File.Copy(script.AbsolutePath, duplicatePath, false);

            var root = script.Source == FlowWorkspaceScriptSource.Reusable ? ReusableFlowsDirectory : TemporaryFlowsDirectory;
            return BuildScriptInfo(duplicatePath, script.Source, root);
        }

        public static FlowWorkspaceScriptInfo PromoteScript(FlowWorkspaceScriptInfo script)
        {
            if (script == null || script.Source != FlowWorkspaceScriptSource.Temporary || !File.Exists(script.AbsolutePath))
            {
                return null;
            }

            EnsureWorkspaceDirectories();

            var targetPath = Path.Combine(ReusableFlowsDirectory, script.RootRelativePath.Replace('/', Path.DirectorySeparatorChar));
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            if (File.Exists(targetPath))
            {
                targetPath = GetUniqueFlowPath(targetDirectory, GetFlowBaseName(Path.GetFileName(targetPath)));
            }

            File.Copy(script.AbsolutePath, targetPath, false);
            return BuildScriptInfo(targetPath, FlowWorkspaceScriptSource.Reusable, ReusableFlowsDirectory);
        }

        public static void DeleteScript(FlowWorkspaceScriptInfo script)
        {
            if (script == null || !File.Exists(script.AbsolutePath))
            {
                return;
            }

            File.Delete(script.AbsolutePath);
        }

        public static FlowWorkspaceExecutionResult ExecuteScriptAsync(FlowWorkspaceScriptInfo script)
        {
            var result = new FlowWorkspaceExecutionResult();

            if (script == null || !File.Exists(script.AbsolutePath))
            {
                result.Success = false;
                result.Message = "The selected flow script no longer exists.";
                return result;
            }

            EnsureWorkspaceDirectories();

            var cliPath = GetCliExecutablePath();
            if (!File.Exists(cliPath))
            {
                result.Success = false;
                result.Message = "Cached AIBridge CLI was not found under AIBridgeCache/CLI. Run the installer first.";
                return result;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = cliPath,
                Arguments = "flow run --file " + QuoteArgument(script.ProjectRelativePath) + " --raw",
                WorkingDirectory = ProjectRoot,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            startInfo.EnvironmentVariables["UNITY_PROJECT_ROOT"] = ProjectRoot;

            try
            {
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.Success = false;
                    result.Message = "Failed to start the cached CLI process.";
                    return result;
                }

                process.Dispose();
                result.Success = true;
                result.Message = "Flow execution started. Progress and results will appear asynchronously in the cache-backed status area.";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Failed to start flow execution: " + ex.Message;
                return result;
            }
        }

        public static void Reveal(FlowWorkspaceScriptInfo script)
        {
            if (script == null)
            {
                return;
            }

            var revealPath = File.Exists(script.AbsolutePath)
                ? script.AbsolutePath
                : (script.Source == FlowWorkspaceScriptSource.Reusable ? ReusableFlowsDirectory : TemporaryFlowsDirectory);

            EditorUtility.RevealInFinder(revealPath);
        }

        public static void RevealReusableFlowsDirectory()
        {
            EnsureWorkspaceDirectories();
            EditorUtility.RevealInFinder(ReusableFlowsDirectory);
        }

        public static void RevealTemporaryFlowsDirectory()
        {
            EnsureWorkspaceDirectories();
            EditorUtility.RevealInFinder(TemporaryFlowsDirectory);
        }

        public static FlowWorkspaceStatusSnapshot LoadStatusSnapshot()
        {
            EnsureWorkspaceDirectories();
            WorkflowJobCacheManager.EnsureDirectoryExists();

            return new FlowWorkspaceStatusSnapshot
            {
                LatestFlowRun = LoadLatestFlowRun(),
                LatestWorkflowJob = WorkflowJobCacheManager.LoadLast()
            };
        }

        public static string ToProjectRelativePath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return string.Empty;
            }

            var fullProjectRoot = Path.GetFullPath(ProjectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fullPath = Path.GetFullPath(absolutePath);
            if (string.Equals(fullPath, fullProjectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var prefix = fullProjectRoot + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return NormalizePath(fullPath.Substring(prefix.Length));
            }

            return NormalizePath(fullPath);
        }

        private static void AddScriptsFromRoot(string root, FlowWorkspaceScriptSource source, List<FlowWorkspaceScriptInfo> scripts)
        {
            if (!Directory.Exists(root))
            {
                return;
            }

            var files = Directory.GetFiles(root, "*" + FlowExtension, SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                scripts.Add(BuildScriptInfo(files[i], source, root));
            }
        }

        private static int CompareScripts(FlowWorkspaceScriptInfo left, FlowWorkspaceScriptInfo right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            if (left.Source != right.Source)
            {
                return left.Source.CompareTo(right.Source);
            }

            return string.Compare(left.RootRelativePath, right.RootRelativePath, StringComparison.OrdinalIgnoreCase);
        }

        private static FlowWorkspaceScriptInfo BuildScriptInfo(string absolutePath, FlowWorkspaceScriptSource source, string root)
        {
            var fullPath = Path.GetFullPath(absolutePath);
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var relativePath = fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)
                ? fullPath.Substring(fullRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                : Path.GetFileName(fullPath);

            return new FlowWorkspaceScriptInfo
            {
                Name = Path.GetFileName(fullPath),
                AbsolutePath = fullPath,
                ProjectRelativePath = ToProjectRelativePath(fullPath),
                RootRelativePath = NormalizePath(relativePath),
                Source = source
            };
        }

        private static string BuildDefaultFlowContent(string path)
        {
            var fileName = Path.GetFileName(path);
            var flowName = SanitizeFlowName(GetFlowBaseName(fileName));
            return "FLOW " + flowName + "\n\nEND\n";
        }

        private static string SanitizeFlowName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "new_flow";
            }

            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                {
                    chars[i] = '_';
                }
            }

            var sanitized = new string(chars).Trim('_');
            return string.IsNullOrWhiteSpace(sanitized) ? "new_flow" : sanitized;
        }

        private static string EnsureFlowScriptPath(string path)
        {
            if (path.EndsWith(FlowExtension, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(0, path.Length - ".txt".Length) + FlowExtension;
            }

            return path + FlowExtension;
        }

        private static bool IsPathUnderRoot(string path, string root)
        {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fullPath = Path.GetFullPath(path);

            if (string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetUniqueFlowPath(string directory, string baseName)
        {
            var candidate = Path.Combine(directory, baseName + FlowExtension);
            if (!File.Exists(candidate))
            {
                return candidate;
            }

            var index = 2;
            while (true)
            {
                candidate = Path.Combine(directory, baseName + "_" + index + FlowExtension);
                if (!File.Exists(candidate))
                {
                    return candidate;
                }

                index++;
            }
        }

        private static string GetFlowBaseName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "flow";
            }

            return fileName.EndsWith(FlowExtension, StringComparison.OrdinalIgnoreCase)
                ? fileName.Substring(0, fileName.Length - FlowExtension.Length)
                : Path.GetFileNameWithoutExtension(fileName);
        }

        private static string GetCliExecutablePath()
        {
            return Path.Combine(CliDirectory, GetCliExecutableName());
        }

        private static string GetCliExecutableName()
        {
#if UNITY_EDITOR_WIN
            return "AIBridgeCLI.exe";
#else
            return "AIBridgeCLI";
#endif
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static FlowWorkspaceRunStatus LoadLatestFlowRun()
        {
            var lastPath = Path.Combine(FlowRunsDirectory, "last.json");
            if (!File.Exists(lastPath))
            {
                return null;
            }

            var lastPointer = TryReadJsonObject(lastPath);
            if (lastPointer == null)
            {
                return null;
            }

            var runId = GetString(lastPointer, "runId", "RunId");
            if (string.IsNullOrWhiteSpace(runId))
            {
                return null;
            }

            var runStatePath = Path.Combine(FlowRunsDirectory, runId + ".json");
            if (!File.Exists(runStatePath))
            {
                return new FlowWorkspaceRunStatus
                {
                    RunId = runId,
                    FlowName = GetString(lastPointer, "flowName", "FlowName"),
                    SourceFilePath = GetString(lastPointer, "sourceFilePath", "SourceFilePath"),
                    Status = "pending"
                };
            }

            var runState = TryReadJsonObject(runStatePath);
            if (runState == null)
            {
                return null;
            }

            return new FlowWorkspaceRunStatus
            {
                RunId = GetString(runState, "RunId", "runId"),
                FlowName = GetString(runState, "FlowName", "flowName"),
                SourceFilePath = GetString(runState, "SourceFilePath", "sourceFilePath"),
                Status = GetString(runState, "Status", "status"),
                CurrentStepId = GetString(runState, "CurrentStepId", "currentStepId"),
                StartedAtUtc = GetString(runState, "StartedAtUtc", "startedAtUtc"),
                CompletedAtUtc = GetString(runState, "CompletedAtUtc", "completedAtUtc"),
                Error = GetString(runState, "Error", "error")
            };
        }

        private static Dictionary<string, object> TryReadJsonObject(string path)
        {
            try
            {
                return AIBridgeJson.DeserializeObject(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        private static string GetString(Dictionary<string, object> data, params string[] keys)
        {
            if (data == null || keys == null)
            {
                return null;
            }

            for (var i = 0; i < keys.Length; i++)
            {
                object value;
                if (data.TryGetValue(keys[i], out value) && value != null)
                {
                    return value.ToString();
                }
            }

            return null;
        }

        private static string NormalizePath(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }
    }
}
