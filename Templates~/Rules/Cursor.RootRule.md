---
templateId: unity-project-rules
assistant: cursor
version: 1
target: root-rule
---
## AIBridge Rules

Use `{{CLI_PATH}}` for Unity Editor automation in this project.

For Unity-project lookup, prefer AIBridge over generic filesystem search whenever possible.

- AIBridgeCLI outputs raw JSON by default; use `--pretty` only when human-readable output is needed
- Use AIBridge for compile checks, console log inspection, scene hierarchy changes, GameObject updates, Transform edits, and asset queries
- Use `compile unity` as the default compile command for Unity projects; use `compile dotnet` only for a separate solution-build validation, not as a fallback for Unity compile failures
- For Unity files/assets/resources/scripts/configs, use `asset search` / `asset find` with `format=paths` first because Unity's AssetDatabase index is faster and more reliable on large projects while returning only the canonical asset paths AI usually needs
- Use `asset get_path` only when starting from a GUID and `asset load` only when metadata confirmation helps
- After locating a path, prefer the host AI's native file-read tool for text-based Unity assets
- Use `asset read_text` only as a fallback when native reads are unavailable or when a Unity-side line window is specifically needed before falling back to generic `grep`/filesystem tools
- Use screenshot or GIF commands for visual verification when Play Mode is required

**Quick Reference**:
```bash
{{CLI_PATH}} compile unity
{{CLI_PATH}} get_logs --logType Error
{{CLI_PATH}} gameobject create --name "Cube" --primitiveType Cube
{{CLI_PATH}} asset search --mode script --keyword "Player" --format paths
{{CLI_PATH}} asset get_path --guid "abc123..."

# Fallback only
{{CLI_PATH}} asset read_text --assetPath "Assets/Scripts/Player.cs" --startLine 1 --maxLines 120
```

Reference: `{{SKILL_DOC_PATH}}`
