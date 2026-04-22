using System.Collections.Generic;

namespace AIBridge.Editor
{
    /// <summary>
    /// AI 助手集成目标注册表
    /// 
    /// Skills 目录支持说明：
    /// - Claude: 支持 .claude/skills/ 目录（Agent Skills 开放标准）
    /// - Cursor: 支持 .cursor/skills/ 目录（Agent Skills 开放标准）
    /// - Codex: 不支持独立 Skills 目录，使用包内路径引用
    /// - Cline: 不支持 Skills 概念，仅支持 .clinerules/ 规则文件
    /// </summary>
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
                    SupportsSkillDirectory = true,
                    RootRuleFileName = ".cursor/rules/aibridge.mdc",
                    SkillDirectoryRelativePath = ".cursor/skills/aibridge",
                    SkillFileName = "SKILL.md",
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
