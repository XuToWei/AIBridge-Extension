using System.Collections.Generic;
using UnityEditor;

namespace AIBridge.Editor
{
    internal static class AssistantIntegrationSelectionSettings
    {
        private const string KeyPrefix = "AIBridge_AssistantIntegration_";

        public static bool GetSelected(string targetId, bool defaultValue = false)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.TryGetAssistantSelection(targetId, out var selected))
            {
                return selected;
            }

            return defaultValue;
        }

        public static void SetSelected(string targetId, bool value)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (!settings.SetAssistantSelection(targetId, value))
            {
                return;
            }

            settings.SaveSettings();
        }

        public static void EnsureDefaults(string projectRoot, IReadOnlyList<AssistantIntegrationTarget> targets)
        {
            var settings = AIBridgeProjectSettings.Instance;
            var changed = false;

            foreach (var target in targets)
            {
                if (settings.TryGetAssistantSelection(target.Id, out _))
                {
                    continue;
                }

                var key = GetSelectionKey(target.Id);
                if (EditorPrefs.HasKey(key))
                {
                    if (settings.SetAssistantSelection(target.Id, EditorPrefs.GetBool(key, false)))
                    {
                        changed = true;
                    }
                    EditorPrefs.DeleteKey(key);
                    continue;
                }

                var detected = AssistantIntegrationDetector.Detect(projectRoot, target);
                if (settings.SetAssistantSelection(target.Id, detected.IsDetected))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                settings.SaveSettings();
            }
        }

        public static Dictionary<string, bool> LoadSelections(string projectRoot, IReadOnlyList<AssistantIntegrationTarget> targets)
        {
            EnsureDefaults(projectRoot, targets);

            var selections = new Dictionary<string, bool>(targets.Count);
            foreach (var target in targets)
            {
                selections[target.Id] = GetSelected(target.Id);
            }

            return selections;
        }

        private static string GetSelectionKey(string targetId)
        {
            return KeyPrefix + targetId;
        }
    }
}
