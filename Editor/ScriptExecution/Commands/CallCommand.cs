using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AIBridge.Editor.ScriptExecution.Commands
{
    /// <summary>
    /// 调用 AIBridge CLI 命令
    /// 格式: call [command] [args...]
    /// </summary>
    public class CallCommand : IScriptCommand
    {
        public string Type => "call";

        private readonly string _arguments;
        private readonly int _timeout;

        public CallCommand(string arguments, int timeout = 60000)
        {
            _arguments = arguments;
            _timeout = timeout;
        }

        public ScriptCommandResult Execute(ScriptExecutionContext context)
        {
            try
            {
                // 直接使用参数作为 CLI 命令
                var cliArgs = _arguments.Trim();
                
                // 查找 CLI 路径（优先使用 AIBridgeCache，其次使用 Tools~）
                var cliPath = Path.Combine(Directory.GetCurrentDirectory(), "AIBridgeCache", "CLI", "AIBridgeCLI.exe");
                if (!File.Exists(cliPath))
                {
                    // 尝试从 Packages 目录查找
                    cliPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "cn.lys.aibridge", "Tools~", "CLI", "win-x64", "AIBridgeCLI.exe");
                }
                
                if (!File.Exists(cliPath))
                {
                    return ScriptCommandResult.Fail($"AIBridge CLI 不存在，已尝试路径:\n1. AIBridgeCache/CLI/AIBridgeCLI.exe\n2. Packages/cn.lys.aibridge/Tools~/CLI/win-x64/AIBridgeCLI.exe");
                }

                context.Log($"[Call] 执行命令: {cliPath} {cliArgs}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = cliPath,
                    Arguments = cliArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return ScriptCommandResult.Fail("无法启动 AIBridge CLI 进程");
                    }

                    var outputData = string.Empty;
                    var errorData = string.Empty;

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputData += e.Data + "\n";
                            context.Log($"[Output] {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorData += e.Data + "\n";
                            context.Log($"[Error] {e.Data}");
                        }
                    };

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(_timeout))
                    {
                        process.Kill();
                        return ScriptCommandResult.Fail($"命令执行超时 ({_timeout}ms)");
                    }

                    if (process.ExitCode != 0)
                    {
                        return ScriptCommandResult.Fail($"命令执行失败 (ExitCode: {process.ExitCode})\n{errorData}");
                    }

                    return ScriptCommandResult.Ok($"命令执行成功\n{outputData}");
                }
            }
            catch (Exception ex)
            {
                return ScriptCommandResult.Fail($"执行命令时发生异常: {ex.Message}", ex);
            }
        }
    }
}
