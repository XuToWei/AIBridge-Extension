using System.Collections.Generic;

namespace AIBridge.Editor
{
    internal static class AssistantIntegrationRegistry
    {
        public static IReadOnlyList<AssistantIntegrationTarget> GetTargets()
        {
            return new[]
            {
                new AssistantIntegrationTarget
                {
                    Id = "claude",
                    DisplayName = "Claude",
                    SupportsSkillDirectory = true,
                    RootRuleFileName = "CLAUDE.md",
                    SkillDirectoryRelativePath = ".claude/skills/aibridge",
                    SkillFileName = "SKILL.md",
                    RootRuleTemplateRelativePath = "Templates~/Rules/Claude.RootRule.md",
                    MissingRootRuleStrategy = MissingRootRuleStrategy.CreateWithInjectedBlock,
                    TemplateId = "unity-integration",
                    RuleTarget = "root-rule"
                },
                new AssistantIntegrationTarget
                {
                    Id = "codex",
                    DisplayName = "Codex",
                    SupportsSkillDirectory = false,
                    RootRuleFileName = "AGENTS.md",
                    SkillDirectoryRelativePath = null,
                    SkillFileName = null,
                    RootRuleTemplateRelativePath = "Templates~/Rules/Codex.RootRule.md",
                    MissingRootRuleStrategy = MissingRootRuleStrategy.CreateWithInjectedBlock,
                    TemplateId = "unity-project-rules",
                    RuleTarget = "root-rule"
                },
                new AssistantIntegrationTarget
                {
                    Id = "cursor",
                    DisplayName = "Cursor",
                    SupportsSkillDirectory = false,
                    RootRuleFileName = ".cursor/rules/aibridge.mdc",
                    SkillDirectoryRelativePath = null,
                    SkillFileName = null,
                    RootRuleTemplateRelativePath = "Templates~/Rules/Cursor.RootRule.md",
                    MissingRootRuleStrategy = MissingRootRuleStrategy.CreateWithInjectedBlock,
                    TemplateId = "unity-project-rules",
                    RuleTarget = "root-rule"
                },
                new AssistantIntegrationTarget
                {
                    Id = "cline",
                    DisplayName = "Cline",
                    SupportsSkillDirectory = false,
                    RootRuleFileName = ".clinerules/aibridge.md",
                    SkillDirectoryRelativePath = null,
                    SkillFileName = null,
                    RootRuleTemplateRelativePath = "Templates~/Rules/Cline.RootRule.md",
                    MissingRootRuleStrategy = MissingRootRuleStrategy.CreateWithInjectedBlock,
                    TemplateId = "unity-project-rules",
                    RuleTarget = "root-rule"
                }
            };
        }
    }
}
