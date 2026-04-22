using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AIBridge.Editor.ScriptExecution
{
    /// <summary>
    /// 脚本执行状态，用于持久化和恢复
    /// </summary>
    [Serializable]
    public class ScriptExecutionState
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
        /// 执行状态
        /// </summary>
        public ExecutionStatus Status { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 执行日志（最近100条）
        /// </summary>
        public List<string> Logs { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        private const string StateFilePath = "AIBridgeCache/script-state.json";

        public ScriptExecutionState()
        {
            Logs = new List<string>();
            Status = ExecutionStatus.Idle;
        }

        /// <summary>
        /// 保存状态到文件
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(StateFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(StateFilePath, json);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ScriptExecutor] 保存状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载状态
        /// </summary>
        public static ScriptExecutionState Load()
        {
            try
            {
                if (!File.Exists(StateFilePath))
                {
                    return new ScriptExecutionState();
                }

                var json = File.ReadAllText(StateFilePath);
                var state = JsonConvert.DeserializeObject<ScriptExecutionState>(json);
                return state ?? new ScriptExecutionState();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ScriptExecutor] 加载状态失败: {ex.Message}");
                return new ScriptExecutionState();
            }
        }

        /// <summary>
        /// 清除状态文件
        /// </summary>
        public static void Clear()
        {
            try
            {
                if (File.Exists(StateFilePath))
                {
                    File.Delete(StateFilePath);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ScriptExecutor] 清除状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加日志（保留最近100条）
        /// </summary>
        public void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Logs.Add($"[{timestamp}] {message}");

            // 保留最近100条日志
            if (Logs.Count > 100)
            {
                Logs.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 已暂停
        /// </summary>
        Paused,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 错误
        /// </summary>
        Error
    }
}
