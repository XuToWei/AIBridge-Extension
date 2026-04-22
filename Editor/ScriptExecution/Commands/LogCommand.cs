using System;
using UnityEngine;

namespace AIBridge.Editor.ScriptExecution.Commands
{
    /// <summary>
    /// 日志输出命令
    /// 格式: log "message"
    /// </summary>
    public class LogCommand : IScriptCommand
    {
        public string Type => "log";

        private readonly string _message;

        public LogCommand(string message)
        {
            _message = message;
        }

        public ScriptCommandResult Execute(ScriptExecutionContext context)
        {
            try
            {
                context.Log($"[Log] {_message}");
                Debug.Log($"[AIBridge Script] {_message}");
                return ScriptCommandResult.Ok();
            }
            catch (Exception ex)
            {
                return ScriptCommandResult.Fail($"输出日志失败: {ex.Message}", ex);
            }
        }
    }
}
