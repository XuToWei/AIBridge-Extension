---
templateId: unity-project-rules
assistant: codex
version: 1
target: root-rule
---
## AIBridge Rules

Use `{{CLI_PATH}}` for Unity Editor automation in this project.

For Unity-project lookup, prefer AIBridge over generic filesystem search whenever possible.

- Prefer `--raw` output for machine-readable responses
- Use AIBridge for compile checks, console log inspection, scene hierarchy changes, GameObject updates, Transform edits, and asset queries
- For Unity files/assets/resources/scripts/configs, use `asset search` / `asset find` first because Unity's AssetDatabase index is faster and more reliable on large projects
- After locating a path, use `asset read_text` for text-based Unity assets before falling back to generic `grep`/filesystem tools
- Use screenshot or GIF commands for visual verification when Play Mode is required

**Quick Reference**:
```bash
{{CLI_EXE_NAME}} compile unity --raw
{{CLI_EXE_NAME}} get_logs --logType Error --raw
{{CLI_EXE_NAME}} gameobject create --name "Cube" --primitiveType Cube --raw
{{CLI_EXE_NAME}} asset search --mode script --keyword "Player" --raw
{{CLI_EXE_NAME}} asset read_text --assetPath "Assets/Scripts/Player.cs" --startLine 1 --maxLines 120 --raw
```

Reference: `{{SKILL_DOC_PATH}}`
