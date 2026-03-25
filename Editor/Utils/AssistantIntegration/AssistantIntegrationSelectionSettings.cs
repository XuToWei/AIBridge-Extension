using System.Collections.Generic;
using UnityEditor;

namespace AIBridge.Editor
{
    internal static class AssistantIntegrationSelectionSettings
    {
        private const string KeyPrefix = "AIBridge_AssistantIntegration_";

        public static bool GetSelected(string targetId, bool defaultValue = false)
        {
            return EditorPrefs.GetBool(GetSelectionKey(targetId), defaultValue);
        }

        public static void SetSelected(string targetId, bool value)
        {
            EditorPrefs.SetBool(GetSelectionKey(targetId), value);
        }

        public static void EnsureDefaults(string projectRoot, IReadOnlyList<AssistantIntegrationTarget> targets)
        {
            foreach (var target in targets)
            {
                var key = GetSelectionKey(target.Id);
                if (EditorPrefs.HasKey(key))
                {
                    continue;
                }

                var detected = AssistantIntegrationDetector.Detect(projectRoot, target);
                EditorPrefs.SetBool(key, detected.IsDetected);
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
