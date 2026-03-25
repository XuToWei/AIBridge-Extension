using System;

namespace AIBridge.Editor
{
    /// <summary>
    /// Project-scoped Unity Editor instance metadata used by the CLI focus command.
    /// </summary>
    [Serializable]
    public class EditorInstanceMetadata
    {
        public int schemaVersion;
        public int processId;
        public string projectRoot;
        public string projectName;
        public string windowTitle;
        public string lastUpdatedUtc;
    }
}
