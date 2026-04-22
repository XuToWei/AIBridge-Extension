using System;
using UnityEditor;

namespace AIBridge.Editor.ScriptExecution.Commands
{
    /// <summary>
    /// 执行编辑器菜单项命令
    /// 格式: menu [menuPath]
    /// </summary>
    public class MenuCommand : IScriptCommand
    {
        public string Type => "menu";

        private readonly string _menuPath;

        public MenuCommand(string menuPath)
        {
            _menuPath = menuPath;
        }

        public ScriptCommandResult Execute(ScriptExecutionContext context)
        {
            try
            {
                context.Log($"[Menu] 执行菜单项: {_menuPath}");

                // 尝试执行菜单项，如果不存在会返回 false
                bool success = EditorApplication.ExecuteMenuItem(_menuPath);
                
                if (!success)
                {
                    return ScriptCommandResult.Fail($"菜单项不存在或执行失败: {_menuPath}");
                }

                context.Log($"[Menu] 菜单项执行完成");
                return ScriptCommandResult.Ok($"菜单项 {_menuPath} 执行成功");
            }
            catch (Exception ex)
            {
                return ScriptCommandResult.Fail($"执行菜单项失败: {ex.Message}", ex);
            }
        }
    }
}
