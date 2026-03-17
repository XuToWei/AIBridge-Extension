using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AIBridge.Editor
{
    [Serializable]
    internal sealed class InjectedBlockMetadata
    {
        public string assistant;
        public string templateId;
        public int version;
        public string target;
    }

    internal sealed class InjectionBlockMatch
    {
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public string RawBlock { get; set; }
        public string Body { get; set; }
        public InjectedBlockMetadata Metadata { get; set; }
    }

    internal static class InjectionBlockParser
    {
        private static readonly Regex BlockRegex = new Regex(
            @"<!-- AIBRIDGE:START (?<metadata>\{.*?\}) -->\s*(?<body>.*?)\s*<!-- AIBRIDGE:END -->",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public static InjectionBlockMatch FindMatchingBlock(string content, string assistantId, string templateId, string target)
        {
            var matches = BlockRegex.Matches(content);
            foreach (Match match in matches)
            {
                var metadata = TryParseMetadata(match.Groups["metadata"].Value);
                if (metadata == null)
                {
                    continue;
                }

                if (metadata.assistant == assistantId && metadata.templateId == templateId && metadata.target == target)
                {
                    return new InjectionBlockMatch
                    {
                        StartIndex = match.Index,
                        Length = match.Length,
                        RawBlock = match.Value,
                        Body = match.Groups["body"].Value.Trim(),
                        Metadata = metadata
                    };
                }
            }

            return null;
        }

        public static string BuildBlock(RuleTemplateMetadata metadata, string renderedBody)
        {
            var payload = new InjectedBlockMetadata
            {
                assistant = metadata.Assistant,
                templateId = metadata.TemplateId,
                version = metadata.Version,
                target = metadata.Target
            };

            return "<!-- AIBRIDGE:START " + JsonUtility.ToJson(payload) + " -->\n"
                + renderedBody.Trim() + "\n"
                + "<!-- AIBRIDGE:END -->";
        }

        private static InjectedBlockMetadata TryParseMetadata(string json)
        {
            try
            {
                return JsonUtility.FromJson<InjectedBlockMetadata>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
