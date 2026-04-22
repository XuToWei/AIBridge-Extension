using System;

namespace AIBridge.Editor.ScriptExecution
{
    /// <summary>
    /// 脚本命令接口，所有脚本命令必须实现此接口
    /// </summary>
    public interface IScriptCommand
    {
        /// <summary>
        /// 命令类型（如 "call"、"delay"、"menu"、"log"）
        /// </summary>
        string Type { get; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <returns>执行结果</returns>
        ScriptCommandResult Execute(ScriptExecutionContext context);
    }

    /// <summary>
    /// 脚本命令执行结果
    /// </summary>
    public class ScriptCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }

        public static ScriptCommandResult Ok(string message = "")
        {
            return new ScriptCommandResult { Success = true, Message = message };
        }

        public static ScriptCommandResult Fail(string message, Exception error = null)
        {
            return new ScriptCommandResult { Success = false, Message = message, Error = error };
        }
    }

    /// <summary>
    /// 脚本执行上下文
    /// </summary>
    public class ScriptExecutionContext
    {
        /// <summary>
        /// 当前脚本路径
        /// </summary>
        public string ScriptPath { get; set; }

        /// <summary>
        /// 当前执行行号（从0开始）
        /// </summary>
        public int CurrentLine { get; set; }

        /// <summary>
        /// 原始命令行文本
        /// </summary>
        public string RawLine { get; set; }

        /// <summary>
        /// 日志回调
        /// </summary>
        public Action<string> LogCallback { get; set; }

        /// <summary>
        /// 输出日志
        /// </summary>
        public void Log(string message)
        {
            LogCallback?.Invoke(message);
        }
    }
}
