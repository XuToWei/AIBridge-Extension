using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AIBridge.Editor.ScriptExecution;
using Newtonsoft.Json;
using UnityEngine;

namespace AIBridge.Editor
{
    /// <summary>
    /// 批处理命令执行：执行脚本文件或脚本文本
    /// </summary>
    public class BatchCommand : ICommand
    {
        public string Type => "batch";
        public bool RequiresRefresh => true;

        public string SkillDescription => @"### `batch` - 批处理脚本执行

```bash
$CLI batch from_file --file ""script.txt""
$CLI batch from_text --text ""call editor log 'Hello'\ndelay 1000""
```";

        public CommandResult Execute(CommandRequest request)
        {
            var action = request.GetParam<string>("action");
            if (string.IsNullOrEmpty(action))
            {
                return CommandResult.Failure(request.id, "Missing 'action' parameter");
            }

            if (action == "from_file")
            {
                return ExecuteFromFile(request);
            }
            else if (action == "from_text")
            {
                return ExecuteFromText(request);
            }
            else
            {
                return CommandResult.Failure(request.id, $"Unknown action: {action}");
            }
        }

        /// <summary>
        /// 从文件执行脚本
        /// </summary>
        private CommandResult ExecuteFromFile(CommandRequest request)
        {
            var filePath = request.GetParam<string>("file");
            if (string.IsNullOrEmpty(filePath))
            {
                return CommandResult.Failure(request.id, "Missing 'file' parameter");
            }

            // 验证文件存在
            if (!File.Exists(filePath))
            {
                return CommandResult.Failure(request.id, $"Script file not found: {filePath}");
            }

            // 验证文件扩展名
            if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Failure(request.id, "Script file must be .txt format");
            }

            // 调用 ScriptExecutor 执行脚本（异步）
            return ExecuteScriptViaExecutor(request.id, filePath, false);
        }

        /// <summary>
        /// 从文本执行脚本
        /// </summary>
        private CommandResult ExecuteFromText(CommandRequest request)
        {
            var scriptPath = request.GetParam<string>("scriptPath");
            if (string.IsNullOrEmpty(scriptPath))
            {
                return CommandResult.Failure(request.id, "Missing 'scriptPath' parameter");
            }

            var keepFile = request.GetParam<bool>("keepFile");

            // 调用 ScriptExecutor 执行脚本（异步）
            return ExecuteScriptViaExecutor(request.id, scriptPath, !keepFile);
        }

        /// <summary>
        /// 通过 ScriptExecutor 执行脚本（异步执行，通过事件回调写入结果）
        /// </summary>
        private CommandResult ExecuteScriptViaExecutor(string requestId, string scriptPath, bool deleteAfterExecution)
        {
            try
            {
                // 检查是否已有脚本正在执行
                if (ScriptExecutor.IsExecuting)
                {
                    return CommandResult.Failure(requestId, "Another script is already executing. Please wait for it to complete.");
                }

                // 订阅执行完成事件
                Action<ExecutionStatus> onStatusChanged = null;
                onStatusChanged = (status) =>
                {
                    // 只处理完成或错误状态
                    if (status == ExecutionStatus.Completed || status == ExecutionStatus.Error)
                    {
                        // 取消订阅
                        ScriptExecutor.OnStatusChanged -= onStatusChanged;

                        // 获取执行状态
                        var state = ScriptExecutor.CurrentState;

                        // 删除临时文件（如果需要）
                        if (deleteAfterExecution && File.Exists(scriptPath))
                        {
                            try
                            {
                                File.Delete(scriptPath);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Failed to delete temporary script file: {ex.Message}");
                            }
                        }

                        // 构建结果
                        CommandResult result;
                        if (status == ExecutionStatus.Completed)
                        {
                            result = CommandResult.Success(requestId, new
                            {
                                scriptPath = scriptPath,
                                status = "completed",
                                startTime = state.StartTime,
                                endTime = state.EndTime,
                                logs = state.Logs
                            });
                        }
                        else
                        {
                            result = CommandResult.Failure(requestId, $"Script execution failed: {state.ErrorMessage}");
                        }

                        // 写入结果文件
                        WriteResultFile(requestId, result);
                    }
                };

                // 订阅状态变化事件
                ScriptExecutor.OnStatusChanged += onStatusChanged;

                // 调用 ScriptExecutor 启动异步执行
                ScriptExecutor.Execute(scriptPath);

                // 立即返回 null（表示异步处理，结果将通过文件写入）
                return null;
            }
            catch (Exception ex)
            {
                return CommandResult.Failure(requestId, $"Script execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入结果文件到 AIBridgeCache/results/
        /// </summary>
        private void WriteResultFile(string requestId, CommandResult result)
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var resultsDir = Path.Combine(projectRoot, "AIBridgeCache", "results");

                if (!Directory.Exists(resultsDir))
                {
                    Directory.CreateDirectory(resultsDir);
                }

                var resultPath = Path.Combine(resultsDir, $"{requestId}.json");
                var json = JsonConvert.SerializeObject(result, Formatting.None);
                File.WriteAllText(resultPath, json, Encoding.UTF8);

                AIBridgeLogger.LogDebug($"Batch command result written to: {resultPath}");
            }
            catch (Exception ex)
            {
                AIBridgeLogger.LogError($"Failed to write batch command result: {ex.Message}");
            }
        }
    }
}
