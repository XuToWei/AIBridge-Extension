# AIBridge

English | [中文](./README_CN.md)

Durable, file-based AI workflows for Unity.

![Unity 2019.4+](https://img.shields.io/badge/Unity-2019.4%2B-black?style=flat-square&logo=unity) ![MIT License](https://img.shields.io/badge/License-MIT-blue?style=flat-square) ![File-based flows](https://img.shields.io/badge/Workflow-File--based%20flows-5b6cff?style=flat-square)

## What AIBridge is

AIBridge connects AI coding assistants with the Unity Editor through a file-based workflow that stays practical during day-to-day development. Instead of relying on a live socket session, it focuses on durable commands, repeatable automation, and Unity-aware lookup that fits real project work.

It is built for teams who want AI to do useful Unity work, not just chat about code.

In plain terms, AIBridge helps AI tools automate Unity work through durable, inspectable project files instead of a live session.

### Highlights

- Unity-aware asset and path lookup before editing files or assets
- Real scene, GameObject, component, and prefab operations from AI workflows
- Compile, build, screenshot, and GIF verification in the same workflow loop
- Reusable `.flow.txt` automation with an Editor-side Flow Workspace

## Why AIBridge, compared with UnityMCP

UnityMCP is a good fit when you want an MCP-style live connection to Unity. AIBridge is optimized for a different tradeoff, stable file-based workflows that are easier to reuse, inspect, and fit projects that regularly cross compile cycles and editor restarts.

That makes AIBridge a strong choice when you care about:

- durable automation instead of a long-lived connection
- reusable workflow files that can be saved, reviewed, and rerun
- lower-friction work in projects where Unity recompiles often
- Unity-aware asset and path lookup before opening files or editing content

## What you can do

- Find assets by Unity-aware indexing and resolve canonical Unity paths
- Inspect and edit scenes, GameObjects, components, and prefabs
- Trigger compile and build workflows from AI-driven automation
- Capture screenshots and animated GIFs for visual verification
- Run reusable `.flow.txt` workflows for repeated Unity tasks

## Common Unity workflows

- Find the correct script, prefab, scene, or ScriptableObject before AI starts editing
- Create or update scene hierarchy, transforms, and component values without manual setup work
- Instantiate prefabs, inspect prefab structure, and apply prefab changes inside one task flow
- Run compile checks and build steps as part of an AI-assisted implementation or packaging task
- Verify UI or gameplay changes with screenshots or GIFs instead of relying on text-only guesses

## Reusable flow workflows

AIBridge supports reusable `.flow.txt` workflows for tasks you want to run more than once, such as preflight checks, project builds, or scene automation.

The built-in Flow Workspace in the Unity Editor helps organize those workflows in two places:

- `Flows/` for reusable project workflows
- `AIBridgeCache/flow-temp/` for temporary or one-off flows

This gives teams a clean split between reusable automation they want to keep in the project and temporary flows generated during one-off AI tasks.

That makes AIBridge useful not only for one command at a time, but also for repeatable AI operating procedures your team can keep and refine.

## Installation

Add AIBridge to your Unity project with Unity Package Manager using this Git URL:

`https://github.com/liyingsong99/AIBridge.git`

You can also clone or download this repository and place it under your project's `Packages` folder.

## Requirements

- Unity 2019.4 or later
- .NET 6.0 Runtime for the bundled CLI workflow tools

## License

MIT License

## Contributing

Issues and pull requests are welcome.
