# AIBridge

[English](./README.md) | 中文

一个面向 Unity 的 AI 辅助插件，适合做资源定位、场景编辑、构建自动化与视觉验证。

![Unity 2019.4+](https://img.shields.io/badge/Unity-2019.4%2B-black?style=flat-square&logo=unity) ![MIT License](https://img.shields.io/badge/License-MIT-blue?style=flat-square) ![AI 辅助 Unity 操作](https://img.shields.io/badge/工作流-AI%20辅助%20Unity%20操作-5b6cff?style=flat-square)

## AIBridge 是什么？

AIBridge 让 AI 助手以更符合 Unity 实际生产流程的方式参与项目工作。

它不只是生成代码，还可以帮助你定位正确的 Unity 资源、查看和修改场景或 Prefab、执行编译和构建流程，以及生成截图或 GIF 做验证。

它面向的是那些希望 AI 真正参与 Unity 工作，而不只是讨论代码的团队。

## 为什么选择 AIBridge？

AIBridge 和 UnityMCP 解决的是相似问题，但优化方向不同。

UnityMCP 更偏向 AI 客户端和 Unity Editor 之间的实时 MCP 连接；AIBridge 更偏向稳定、可检查的 Unity 任务执行方式，包括 Unity 感知的资源定位、可重复执行的自动化，以及更适合跨编译周期和编辑器重启继续推进的项目操作。

如果你想要的是 MCP 风格的实时编辑器连接，UnityMCP 是很合适的方案；如果你想要的是更适合日常项目协作的、可检查且可重复执行的 Unity 任务流，AIBridge 更适合这个取舍。

| 维度 | AIBridge | UnityMCP |
|---|---|---|
| 主要模型 | 稳定的 Unity 任务执行 | 实时 MCP / Editor 连接 |
| 更适合 | 可复用、可检查的任务流 | 强交互的实时工具调用 |
| 复用方式 | 面向项目的长期任务操作 | 依赖客户端会话中的工具调用 |
| 典型优势 | 构建、验证、长期可复用流程 | 从 MCP 客户端直接控制 Editor |

## 你可以用它做什么

- 在修改前先定位正确的 Unity 资源，并解析规范路径。
- 查看和修改场景、GameObject、Transform、组件与 Prefab。
- 从 AI 自动化流程中触发编译、预检查、打包和构建任务。
- 生成截图和 GIF，用于可视化结果验证。
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

## 系统要求

- Unity 2019.4 或更高版本
- .NET 6.0 Runtime，用于随包提供的 CLI 工具

## CLI 输出模式

随包提供的 AIBridgeCLI 面向 AI 与自动化工作流优化，命令输出默认是便于机器解析的紧凑 JSON；如果需要更适合人工阅读的文本输出，可以显式使用 `--pretty`。

## 许可证

MIT License

## 贡献

欢迎提交 issue 和 pull request。
