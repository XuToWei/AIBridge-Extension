using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using AIBridgeCLI.Core;
using Newtonsoft.Json;

namespace AIBridgeCLI.Commands
{
    internal static class UnityEditorInstanceResolver
    {
        private const string MetadataFileName = "editor-instance.json";
        private static readonly TimeSpan MaxMetadataAge = TimeSpan.FromMinutes(10);

        public static bool TryResolve(out Process process, out string error)
        {
            process = null;
            error = null;

            var exchangeDirectory = PathHelper.GetExchangeDirectory();
            var metadataPath = Path.Combine(exchangeDirectory, MetadataFileName);

            if (!File.Exists(metadataPath))
            {
                error = "Unity Editor metadata for the current project was not found. Make sure this project's Unity Editor is open and AIBridge is active.";
                return false;
            }

            EditorInstanceMetadata metadata;
            try
            {
                var json = File.ReadAllText(metadataPath);
                metadata = JsonConvert.DeserializeObject<EditorInstanceMetadata>(json);
            }
            catch (Exception ex)
            {
                error = $"Failed to read Unity Editor metadata: {ex.Message}";
                return false;
            }

            if (metadata == null)
            {
                error = "Unity Editor metadata is empty or invalid.";
                return false;
            }

            var expectedProjectRoot = Path.GetDirectoryName(exchangeDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!PathsEqual(metadata.projectRoot, expectedProjectRoot))
            {
                error = "Unity Editor metadata does not match the current project root.";
                return false;
            }

            if (metadata.processId <= 0)
            {
                error = "Unity Editor metadata does not contain a valid process ID.";
                return false;
            }

            if (!TryParseUtcTimestamp(metadata.lastUpdatedUtc, out var lastUpdatedUtc))
            {
                error = "Unity Editor metadata does not contain a valid heartbeat timestamp.";
                return false;
            }

            if (DateTime.UtcNow - lastUpdatedUtc > MaxMetadataAge)
            {
                error = "Unity Editor metadata for the current project is stale. Reopen or refocus the project's Unity Editor and try again.";
                return false;
            }

            try
            {
                var candidate = Process.GetProcessById(metadata.processId);
                candidate.Refresh();

                if (!candidate.ProcessName.Equals("Unity", StringComparison.OrdinalIgnoreCase))
                {
                    error = $"Resolved process {metadata.processId} is '{candidate.ProcessName}', not Unity.";
                    return false;
                }

                if (candidate.MainWindowHandle == IntPtr.Zero)
                {
                    error = "The Unity Editor for the current project is running, but its main window is not ready yet.";
                    return false;
                }

                process = candidate;
                return true;
            }
            catch (ArgumentException)
            {
                error = $"Unity Editor process {metadata.processId} for the current project is no longer running.";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Failed to inspect Unity Editor process {metadata.processId}: {ex.Message}";
                return false;
            }
        }

        private static bool TryParseUtcTimestamp(string value, out DateTime timestampUtc)
        {
            return DateTime.TryParse(value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out timestampUtc);
        }

        private static bool PathsEqual(string left, string right)
        {
            var normalizedLeft = NormalizePath(left);
            var normalizedRight = NormalizePath(right);

            if (string.IsNullOrEmpty(normalizedLeft) || string.IsNullOrEmpty(normalizedRight))
            {
                return false;
            }

            return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private class EditorInstanceMetadata
        {
            public int schemaVersion { get; set; }
            public int processId { get; set; }
            public string projectRoot { get; set; }
            public string projectName { get; set; }
            public string windowTitle { get; set; }
            public string lastUpdatedUtc { get; set; }
        }
    }
}
