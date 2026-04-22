using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace AIBridge.Editor.ScriptExecution
{
    /// <summary>
    /// 脚本执行器，负责逐行执行脚本并管理执行状态
    /// </summary>
    [InitializeOnLoad]
    public class ScriptExecutor
    {
        private static ScriptExecutor _instance;
        private ScriptExecutionState _state;
        private List<IScriptCommand> _commands;
        private bool _isExecuting;

        /// <summary>
        /// 当前执行状态
        /// </summary>
        public static ScriptExecutionState CurrentState => Instance._state;

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public static bool IsExecuting => Instance._isExecuting;

        /// <summary>
        /// 日志更新事件
        /// </summary>
        public static event Action<string> OnLogUpdated;

        /// <summary>
        /// 状态更新事件
        /// </summary>
        public static event Action<ExecutionStatus> OnStatusChanged;

        private static ScriptExecutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScriptExecutor();
                }
                return _instance;
            }
        }

        static ScriptExecutor()
        {
            // 编辑器启动时自动恢复执行状态
            EditorApplication.delayCall += () =>
            {
                Instance.Initialize();
            };
        }

        private ScriptExecutor()
        {
            _state = new ScriptExecutionState();
            _commands = new List<IScriptCommand>();

            // 订阅编译事件
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        private void Initialize()
        {
            // 加载上次的执行状态
            _state = ScriptExecutionState.Load();

            // 如果上次是运行中状态，自动恢复执行
            if (_state.Status == ExecutionStatus.Running)
            {
                Log($"检测到未完成的脚本执行，自动恢复: {_state.ScriptPath}");
                ResumeExecution();
            }
        }

        /// <summary>
        /// 开始执行脚本
        /// </summary>
        public static void Execute(string scriptPath)
        {
            Instance.ExecuteInternal(scriptPath);
        }

        /// <summary>
        /// 暂停执行
        /// </summary>
        public static void Pause()
        {
            Instance.PauseInternal();
        }

        /// <summary>
        /// 恢复执行
        /// </summary>
        public static void Resume()
        {
            Instance.ResumeExecution();
        }

        /// <summary>
        /// 停止执行
        /// </summary>
        public static void Stop()
        {
            Instance.StopInternal();
        }

        private void ExecuteInternal(string scriptPath)
        {
            if (_isExecuting)
            {
                Log("已有脚本正在执行，请先停止当前脚本");
                return;
            }

            try
            {
                // 解析脚本
                Log($"开始解析脚本: {scriptPath}");
                _commands = ScriptParser.Parse(scriptPath);
                Log($"脚本解析完成，共 {_commands.Count} 条命令");

                // 初始化状态
                _state = new ScriptExecutionState
                {
                    ScriptPath = scriptPath,
                    CurrentLine = 0,
                    Status = ExecutionStatus.Running,
                    StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _state.Save();

                _isExecuting = true;
                NotifyStatusChanged(ExecutionStatus.Running);

                // 开始执行
                EditorApplication.update += ExecuteNextCommand;
            }
            catch (Exception ex)
            {
                LogError($"脚本执行失败: {ex.Message}");
                _state.Status = ExecutionStatus.Error;
                _state.ErrorMessage = ex.Message;
                _state.Save();
                NotifyStatusChanged(ExecutionStatus.Error);
            }
        }

        private void ResumeExecution()
        {
            if (_isExecuting)
            {
                Log("脚本已在执行中");
                return;
            }

            if (_state.Status != ExecutionStatus.Running && _state.Status != ExecutionStatus.Paused)
            {
                Log("没有可恢复的脚本执行");
                return;
            }

            try
            {
                // 重新解析脚本
                Log($"恢复执行脚本: {_state.ScriptPath}，从第 {_state.CurrentLine + 1} 行开始");
                _commands = ScriptParser.Parse(_state.ScriptPath);

                _state.Status = ExecutionStatus.Running;
                _state.Save();

                _isExecuting = true;
                NotifyStatusChanged(ExecutionStatus.Running);

                // 继续执行
                EditorApplication.update += ExecuteNextCommand;
            }
            catch (Exception ex)
            {
                LogError($"恢复执行失败: {ex.Message}");
                _state.Status = ExecutionStatus.Error;
                _state.ErrorMessage = ex.Message;
                _state.Save();
                NotifyStatusChanged(ExecutionStatus.Error);
            }
        }

        private void PauseInternal()
        {
            if (!_isExecuting)
            {
                Log("没有正在执行的脚本");
                return;
            }

            Log("暂停脚本执行");
            _isExecuting = false;
            _state.Status = ExecutionStatus.Paused;
            _state.Save();

            EditorApplication.update -= ExecuteNextCommand;
            NotifyStatusChanged(ExecutionStatus.Paused);
        }

        private void StopInternal()
        {
            if (!_isExecuting && _state.Status != ExecutionStatus.Paused)
            {
                Log("没有正在执行的脚本");
                return;
            }

            Log("停止脚本执行");
            _isExecuting = false;
            _state.Status = ExecutionStatus.Idle;
            _state.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _state.Save();

            EditorApplication.update -= ExecuteNextCommand;
            NotifyStatusChanged(ExecutionStatus.Idle);

            // 清除状态
            ScriptExecutionState.Clear();
        }

        private void ExecuteNextCommand()
        {
            if (!_isExecuting || _state.Status != ExecutionStatus.Running)
            {
                EditorApplication.update -= ExecuteNextCommand;
                return;
            }

            // 检查是否执行完成
            if (_state.CurrentLine >= _commands.Count)
            {
                Log("脚本执行完成");
                _isExecuting = false;
                _state.Status = ExecutionStatus.Completed;
                _state.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _state.Save();

                EditorApplication.update -= ExecuteNextCommand;
                NotifyStatusChanged(ExecutionStatus.Completed);

                // 清除状态
                ScriptExecutionState.Clear();
                return;
            }

            try
            {
                // 执行当前命令
                var command = _commands[_state.CurrentLine];
                Log($"[{_state.CurrentLine + 1}/{_commands.Count}] 执行命令: {command.Type}");

                var context = new ScriptExecutionContext
                {
                    ScriptPath = _state.ScriptPath,
                    CurrentLine = _state.CurrentLine,
                    LogCallback = Log
                };

                var result = command.Execute(context);

                if (!result.Success)
                {
                    LogError($"命令执行失败: {result.Message}");
                    _isExecuting = false;
                    _state.Status = ExecutionStatus.Error;
                    _state.ErrorMessage = result.Message;
                    _state.Save();

                    EditorApplication.update -= ExecuteNextCommand;
                    NotifyStatusChanged(ExecutionStatus.Error);
                    return;
                }

                // 移动到下一行
                _state.CurrentLine++;
                _state.Save();
            }
            catch (Exception ex)
            {
                LogError($"执行命令时发生异常: {ex.Message}");
                _isExecuting = false;
                _state.Status = ExecutionStatus.Error;
                _state.ErrorMessage = ex.Message;
                _state.Save();

                EditorApplication.update -= ExecuteNextCommand;
                NotifyStatusChanged(ExecutionStatus.Error);
            }
        }

        private void OnCompilationStarted(object obj)
        {
            // 编译开始时暂停执行
            if (_isExecuting)
            {
                Log("检测到编译开始，暂停脚本执行");
                PauseInternal();
            }
        }

        private static void Log(string message)
        {
            Debug.Log($"[ScriptExecutor] {message}");
            Instance._state.AddLog(message);
            Instance._state.Save();
            OnLogUpdated?.Invoke(message);
        }

        private static void LogError(string message)
        {
            Debug.LogError($"[ScriptExecutor] {message}");
            Instance._state.AddLog($"[ERROR] {message}");
            Instance._state.Save();
            OnLogUpdated?.Invoke($"[ERROR] {message}");
        }

        private static void NotifyStatusChanged(ExecutionStatus status)
        {
            OnStatusChanged?.Invoke(status);
        }
    }
}
