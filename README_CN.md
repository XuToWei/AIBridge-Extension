# AIBridge

[English](./README.md) | 中文

一个面向 Unity 的 AI 辅助插件，适合做资源定位、场景编辑、构建自动化与视觉验证。

![Unity 2019.4+](https://img.shields.io/badge/Unity-2019.4%2B-black?style=flat-square&logo=unity) ![MIT License](https://img.shields.io/badge/License-MIT-blue?style=flat-square) ![AI 辅助 Unity 操作](https://img.shields.io/badge/工作流-AI%20辅助%20Unity%20操作-5b6cff?style=flat-square)

## AIBridge 是什么？

AIBridge 让 AI 助手以更符合 Unity 实际生产流程的方式参与项目工作。

它不只是生成代码，还可以帮助你定位正确的 Unity 资源、查看和修改场景或 Prefab、执行编译和构建流程，以及生成截图或 GIF 做验证。

它面向的是那些希望 AI 真正参与 Unity 工作，而不只是讨论代码的团队。

## 为什么选择 AIBridge？

AIBridge 和 UnityMCP 解决的是相似问题，但 AIBridge 在稳定性、易用性和可扩展性上具有显著优势。

### 核心优势

**🚀 更高的稳定性**
- 基于文件通信机制，不依赖持续的网络连接或 MCP 协议栈
- 编辑器重启、编译周期、域重载等场景下任务流不会中断
- 命令执行结果可持久化，支持断点续传和任务恢复

**⚡ 更好的易用性**
- 开箱即用，无需配置复杂的 MCP 服务端和客户端
- 标准 CLI 工具，可直接集成到任何 AI 工作流或自动化脚本
- 紧凑的 JSON 输出格式，便于 AI 解析和处理
- 支持一键安装 AGENTS.md 工作流模板，快速配置 AI 协作规范

**🔧 更强的可扩展性**
- 模块化命令架构，易于添加自定义命令和工作流
- 支持通过 Skills 系统扩展 AI 能力边界
- 完整的 Unity 资源感知能力（AssetDatabase、场景层级、组件属性）
- 内置截图和 GIF 录制，支持视觉验证闭环

**📊 实际表现对比**

| 维度 | AIBridge | UnityMCP |
|---|---|---|
| 连接稳定性 | ⭐⭐⭐⭐⭐ 文件通信，无连接中断 | ⭐⭐⭐ 依赖持续网络连接 |
| 编译周期适应 | ⭐⭐⭐⭐⭐ 跨编译周期无缝继续 | ⭐⭐ 编译时连接可能中断 |
| 部署复杂度 | ⭐⭐⭐⭐⭐ 零配置，开箱即用 | ⭐⭐⭐ 需配置 MCP 服务端 |
| AI 集成难度 | ⭐⭐⭐⭐⭐ 标准 CLI + JSON 输出 | ⭐⭐⭐ 需适配 MCP 协议 |
| 任务可追溯性 | ⭐⭐⭐⭐⭐ 完整的执行日志和结果 | ⭐⭐⭐ 依赖会话状态 |
| 扩展性 | ⭐⭐⭐⭐⭐ 模块化架构 + Skills | ⭐⭐⭐ 基于 MCP 工具扩展 |

AIBridge 专为生产环境的 Unity 项目设计，在日常协作、构建自动化、视觉验证等场景中表现更加可靠和高效。

## 你可以用它做什么

- 在修改前先定位正确的 Unity 资源，并解析规范路径。
- 查看和修改场景、GameObject、Transform、组件与 Prefab。
- 从 AI 自动化流程中触发编译、预检查、打包和构建任务。
- 生成截图和 GIF，用于可视化结果验证。
- 控制 Game 视图分辨率，用于不同屏幕尺寸下的一致性视觉测试。
- 让重复出现的 Unity 任务进入更稳定的 AI 自动化流程，而不是每次都从文字描述重新开始。

## 常见 Unity 使用场景

### 1. 先让 AI 找到正确资源，再开始修改
在大型 Unity 项目里，真正的难点往往是先定位到正确的脚本、Prefab、场景或 ScriptableObject。AIBridge 的目标就是先把真实路径找准，再继续后续操作。

### 2. 让 AI 像 Unity 协作者一样修改场景
你可以让 AI 创建对象、调整 Transform、修改组件值、整理层级结构，并直接保存场景，而不是把任务退回到大量手工编辑。

### 3. 自动化重复的构建步骤
预检查、版本号更新、Android 打包、iOS 打包等反复执行的步骤，都可以通过 AI 辅助自动化来处理，减少反复手工操作。

### 4. 批量生成可重复的场景内容
当场景搭建遵循清单、manifest 或固定模式时，AIBridge 更适合稳定地应用同一套结构，而不是一遍遍手工搭建。

### 5. 用真实画面验证结果，而不是靠猜
截图和 GIF 让 AI 辅助任务可以基于实际编辑器结果去验证 UI 或玩法改动，而不是只看文字描述。

### 6. 让 AI 辅助结果更容易被验证
通过把 Unity 感知操作、截图能力和面向构建的自动化结合起来，团队可以基于真实项目状态验证 AI 输出，而不是只依赖模糊的提示词。

## 安装

在 Unity Package Manager 中使用下面的 Git 地址即可把 AIBridge 加入你的 Unity 项目：

`https://github.com/liyingsong99/AIBridge.git`

你也可以直接克隆或下载本仓库，然后放到项目的 `Packages` 目录下。

### 配置 AI 工作流（推荐）

AIBridge 提供了开箱即用的 AGENTS.md 工作流模板，帮助你快速配置 AI 协作规范：

1. 在 Unity 编辑器中打开 `Tools > AIBridge Settings`
2. 切换到 `Skills 安装` 页签
3. 点击 `安装 AGENTS.md` 按钮
4. 确认后将自动安装工作流模板到项目根目录

AGENTS.md 包含：
- 强制工作流规范（Skills 匹配、需求确认、方案实施、检查清单）
- Unity 项目开发原则（禁止 hard code、C# 版本兼容、代码规范）
- AIBridge 编译和诊断检查流程
- 完整的质量保证检查清单

## 系统要求

- Unity 2019.4 或更高版本
- .NET 8.0 Runtime，用于随包提供的 CLI 工具

## CLI 命令速览

随包提供的 AIBridgeCLI 把 AIBridge 的主要工作流暴露成可直接调用的命令。命令默认返回紧凑 JSON，因此很适合接入 AI 与自动化流程。

- **先定位正确的 Unity 资源或工程内文件**，通过 Unity 的 AssetDatabase 获取规范路径
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe asset search --mode script --keyword "Player" --format paths
  ./AIBridgeCache/CLI/AIBridgeCLI.exe asset find --filter "t:Prefab" --format paths
  ./AIBridgeCache/CLI/AIBridgeCLI.exe asset get_path --guid "abc123..."
  ```
- **查看 Prefab 的元信息和层级结构**，在修改 Prefab 资源或实例前先确认现状
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe prefab get_info --prefabPath "Assets/Prefabs/Player.prefab"
  ./AIBridgeCache/CLI/AIBridgeCLI.exe prefab get_hierarchy --prefabPath "Assets/Prefabs/Player.prefab"
  ```
- **查看场景层级和当前编辑器上下文**，让 AI 在修改前先理解现有状态
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe scene get_hierarchy --depth 3 --includeInactive false
  ./AIBridgeCache/CLI/AIBridgeCLI.exe selection get --includeComponents true
  ```
- **查看组件和序列化属性**，避免靠猜测判断 GameObject 上有什么内容
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe inspector get_components --path "Player"
  ./AIBridgeCache/CLI/AIBridgeCLI.exe inspector get_properties --path "Player" --componentName "Transform"
  ```
- **创建或修改场景对象**，把常见编辑器操作变成可自动化的命令
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe gameobject create --name "MyCube" --primitiveType Cube
  ./AIBridgeCache/CLI/AIBridgeCLI.exe transform set_position --path "Player" --x 0 --y 1 --z 0
  ```
- **触发 Unity 侧编译并做解决方案校验**，用于脚本改动后的验证流程
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe compile unity
  ./AIBridgeCache/CLI/AIBridgeCLI.exe compile dotnet
  ```
- **读取 Unity Console 日志**，让 AI 能基于真实报错和警告继续推进任务
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe get_logs --logType Error
  ```
- **执行脚本自动化**，支持编译感知的重复任务自动化
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe batch from_text --text "call editor log 'Hello'\ndelay 1000"
  ./AIBridgeCache/CLI/AIBridgeCLI.exe batch from_file --file "script.txt"
  ```
- **截图或录制 GIF 做视觉验证**，适合 Play Mode 下的结果确认
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe screenshot game
  ./AIBridgeCache/CLI/AIBridgeCLI.exe screenshot gif --frameCount 50
  ```
- **设置或查询 Game 视图分辨率**，用于视觉测试的一致性确认
  ```bash
  ./AIBridgeCache/CLI/AIBridgeCLI.exe gameview set_resolution --width 1920 --height 1080
  ./AIBridgeCache/CLI/AIBridgeCLI.exe gameview get_resolution
  ```

## 许可证

MIT License

## 贡献

欢迎提交 issue 和 pull request。
