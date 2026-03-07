using System.Collections.Generic;

namespace AIBridgeCLI.Commands
{
    /// <summary>
    /// Prefab command builder: instantiate, save, unpack, get_info, get_hierarchy, apply
    /// </summary>
    public class PrefabCommandBuilder : BaseCommandBuilder
    {
        public override string Type => "prefab";
        public override string Description => "Prefab operations (instantiate, inspect, save, unpack, apply)";

        public override string[] Actions => new[]
        {
            "instantiate", "save", "unpack", "get_info", "get_hierarchy", "apply"
        };

        protected override Dictionary<string, List<ParameterInfo>> ActionParameters => new Dictionary<string, List<ParameterInfo>>
        {
            ["instantiate"] = new List<ParameterInfo>
            {
                new ParameterInfo("prefabPath", "Path to the prefab asset", true),
                new ParameterInfo("posX", "X position", false, "0"),
                new ParameterInfo("posY", "Y position", false, "0"),
                new ParameterInfo("posZ", "Z position", false, "0")
            },
            ["save"] = new List<ParameterInfo>
            {
                new ParameterInfo("gameObjectPath", "Path to the GameObject (uses selection if not specified)", false),
                new ParameterInfo("savePath", "Path to save the prefab", true)
            },
            ["unpack"] = new List<ParameterInfo>
            {
                new ParameterInfo("gameObjectPath", "Path to the prefab instance (uses selection if not specified)", false),
                new ParameterInfo("completely", "Unpack completely (recursive)", false, "false")
            },
            ["get_info"] = new List<ParameterInfo>
            {
                new ParameterInfo("prefabPath", "Path to the prefab asset", false),
                new ParameterInfo("gameObjectPath", "Path to the prefab instance", false)
            },
            ["get_hierarchy"] = new List<ParameterInfo>
            {
                new ParameterInfo("prefabPath", "Path to the prefab asset", true),
                new ParameterInfo("depth", "Max depth to traverse", false, "5"),
                new ParameterInfo("includeInactive", "Include inactive GameObjects", false, "true"),
                new ParameterInfo("includeComponents", "Include component type names", false, "true")
            },
            ["apply"] = new List<ParameterInfo>
            {
                new ParameterInfo("gameObjectPath", "Path to the prefab instance (uses selection if not specified)", false)
            }
        };
    }
}
