using System;
using System.Threading;

namespace AIBridge.Editor.ScriptExecution.Commands
{
    /// <summary>
    /// 延迟命令
    /// 格式: delay [milliseconds]
    /// </summary>
    public class DelayCommand : IScriptCommand
    {
        public string Type => "delay";

        private readonly int _milliseconds;

        public DelayCommand(int milliseconds)
        {
            _milliseconds = milliseconds;
        }

        public ScriptCommandResult Execute(ScriptExecutionContext context)
        {
            try
            {
                context.Log($"[Delay] 等待 {_milliseconds}ms...");
                Thread.Sleep(_milliseconds);
                context.Log($"[Delay] 等待完成");
                return ScriptCommandResult.Ok($"延迟 {_milliseconds}ms 完成");
            }
            catch (Exception ex)
            {
                return ScriptCommandResult.Fail($"延迟执行失败: {ex.Message}", ex);
            }
        }
    }
}
