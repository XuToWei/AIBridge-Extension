# AIBridge

[English](./README.md) | 中文

面向 Unity 的稳定文件式 AI 工作流。

![Unity 2019.4+](https://img.shields.io/badge/Unity-2019.4%2B-black?style=flat-square&logo=unity) ![MIT License](https://img.shields.io/badge/License-MIT-blue?style=flat-square) ![文件式工作流](https://img.shields.io/badge/工作流-文件式%20Flow-5b6cff?style=flat-square)

## AIBridge 是什么

AIBridge 通过文件式工作流把 AI 编码助手连接到 Unity Editor，更适合日常开发中的真实任务。它不依赖持续在线的长连接，而是强调可落地的自动化、可复用的流程，以及对 Unity 资源路径和编辑操作更友好的工作方式。

它的目标不是让 AI 只会讨论代码，而是让 AI 真正参与 Unity 项目的实际工作。

简单来说，AIBridge 让 AI 通过可检查、可保存的项目文件来自动化 Unity 工作，而不是强依赖一个持续在线的实时会话。

### 核心亮点

- 在修改前先用 Unity 感知的方式定位资源和规范路径
- 让 AI 真正参与场景、GameObject、组件和 Prefab 操作
- 把编译、构建、截图和 GIF 验证纳入同一条工作流
- 用 Editor 内置的 Flow Workspace 管理可复用的 `.flow.txt` 自动化

## 为什么选择 AIBridge，而不是 UnityMCP

如果你想要 MCP 风格的实时连接，UnityMCP 是一种合适的方案。AIBridge 优化的是另一种取舍，也就是更稳定的文件式工作流，更容易复用、检查，也更适合那些经常会经历编译周期和编辑器重启的项目流程。

这让 AIBridge 更适合下面这类场景：

- 你更关心可持续执行的自动化，而不是长连接会话
- 你希望把工作流程保存成可复查、可重复执行的文件
- 你的项目经常触发 Unity 重新编译，希望减少中断感
- 你希望先通过 Unity 感知的资源索引定位路径，再交给 AI 继续处理

## 你可以用它做什么

- 通过 Unity 感知的索引查找资源，并解析规范的 Unity 路径
- 查看和编辑场景、GameObject、组件、Prefab
- 从 AI 工作流中触发编译与构建流程
- 生成截图和 GIF，用于可视化验证
- 运行可复用的 `.flow.txt` 工作流，处理重复性的 Unity 任务

## 常见 Unity 使用场景

- 在大型项目里先找到正确的脚本、Prefab、场景或 ScriptableObject，再交给 AI 修改
- 创建或调整场景层级、Transform 和组件属性，减少手工摆放和重复操作
- 在一条任务流里完成 Prefab 实例化、结构检查和修改应用
- 在 AI 辅助实现或打包任务里顺手执行编译检查和构建步骤
- 用截图或 GIF 验证 UI 和玩法改动，而不是只靠文字推测结果

## 可复用的 Flow 工作流

AIBridge 支持可复用的 `.flow.txt` 工作流，适合处理那些会反复执行的任务，比如预检查、项目构建、场景自动化等。

Unity Editor 内置的 Flow Workspace 会把这些流程分成两个位置来管理：

- `Flows/`，用于项目内可复用的流程
- `AIBridgeCache/flow-temp/`，用于临时或一次性的流程

这样团队就能把需要长期保留的自动化流程和 AI 临时生成的一次性流程明确区分开来。

这让 AIBridge 不只适合单次命令，也适合沉淀为团队可以持续复用和迭代的 AI 操作流程。

## 安装

在 Unity Package Manager 中使用下面的 Git 地址即可把 AIBridge 加入你的 Unity 项目：

`https://github.com/liyingsong99/AIBridge.git`

你也可以直接克隆或下载本仓库，然后放到项目的 `Packages` 目录下。

## 系统要求

- Unity 2019.4 或更高版本
- .NET 6.0 Runtime，用于随包提供的 CLI 工作流工具

## 许可证

MIT License

## 贡献

欢迎提交 issue 和 pull request。
