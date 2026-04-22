using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using AIBridge.Editor.ScriptExecution.Commands;

namespace AIBridge.Editor.ScriptExecution
{
    /// <summary>
    /// 脚本解析器，将 .txt 文件解析为命令对象列表
    /// </summary>
    public static class ScriptParser
    {
        /// <summary>
        /// 解析脚本文件
        /// </summary>
        /// <param name="scriptPath">脚本文件路径</param>
        /// <returns>命令列表</returns>
        public static List<IScriptCommand> Parse(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"脚本文件不存在: {scriptPath}");
            }

            var commands = new List<IScriptCommand>();
            var lines = File.ReadAllLines(scriptPath);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 跳过空行和注释
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                try
                {
                    var command = ParseLine(line);
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"解析脚本第 {i + 1} 行失败: {line}\n错误: {ex.Message}", ex);
                }
            }

            return commands;
        }

        /// <summary>
        /// 解析单行命令
        /// </summary>
        private static IScriptCommand ParseLine(string line)
        {
            // 移除行内注释
            var commentIndex = line.IndexOf('#');
            if (commentIndex > 0)
            {
                line = line.Substring(0, commentIndex).Trim();
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            // call [command] [args...]
            if (line.StartsWith("call ", StringComparison.OrdinalIgnoreCase))
            {
                var args = line.Substring(5).Trim();
                
                // 提取超时参数（如果有）
                var timeoutMatch = Regex.Match(args, @"--timeout\s+(\d+)");
                var timeout = timeoutMatch.Success ? int.Parse(timeoutMatch.Groups[1].Value) : 60000;
                
                return new CallCommand(args, timeout);
            }

            // delay [milliseconds]
            if (line.StartsWith("delay ", StringComparison.OrdinalIgnoreCase))
            {
                var msStr = line.Substring(6).Trim();
                if (int.TryParse(msStr, out var ms))
                {
                    return new DelayCommand(ms);
                }
                throw new Exception($"无效的延迟时间: {msStr}");
            }

            // menu [menuPath]
            if (line.StartsWith("menu ", StringComparison.OrdinalIgnoreCase))
            {
                var menuPath = line.Substring(5).Trim();
                return new MenuCommand(menuPath);
            }

            // log "message" 或 log message
            if (line.StartsWith("log ", StringComparison.OrdinalIgnoreCase))
            {
                var message = line.Substring(4).Trim();
                
                // 移除引号（如果有）
                if (message.StartsWith("\"") && message.EndsWith("\""))
                {
                    message = message.Substring(1, message.Length - 2);
                }
                
                return new LogCommand(message);
            }

            throw new Exception($"未知的命令类型: {line}");
        }

        /// <summary>
        /// 验证脚本语法
        /// </summary>
        /// <param name="scriptPath">脚本文件路径</param>
        /// <returns>验证结果（成功返回 null，失败返回错误信息）</returns>
        public static string Validate(string scriptPath)
        {
            try
            {
                Parse(scriptPath);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
