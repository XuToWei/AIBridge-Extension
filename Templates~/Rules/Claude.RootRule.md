---
templateId: unity-integration
assistant: claude
version: 1
target: root-rule
---
## AIBridge Unity Integration

Use `{{CLI_PATH}}` to interact with Unity Editor through AIBridge.

**Skill**: `aibridge`

**When to Use**:
- Read Unity console errors and warnings
- Trigger compile checks and inspect results
- Create or modify GameObjects, Components, Scenes, and Prefabs
- Search assets and capture screenshots or GIFs from Play Mode

**Search Priority**:
- For Unity project files/assets/resources/scripts/configs, prefer AIBridge over generic filesystem search
- Use `asset search` / `asset find` first to resolve canonical asset paths via Unity's AssetDatabase index
- Use `asset read_text` to inspect text-based Unity assets after locating the path
- Fall back to generic grep/read tools only when AIBridge cannot cover the target

**Quick Reference**:
```bash
# CLI Path
{{CLI_PATH}}

# Common Commands
{{CLI_EXE_NAME}} compile unity --raw
{{CLI_EXE_NAME}} get_logs --logType Error --raw
{{CLI_EXE_NAME}} asset search --mode script --keyword "Player" --raw
{{CLI_EXE_NAME}} asset read_text --assetPath "Assets/Scripts/Player.cs" --startLine 1 --maxLines 120 --raw
{{CLI_EXE_NAME}} gameobject create --name "Cube" --primitiveType Cube --raw
{{CLI_EXE_NAME}} transform set_position --path "Player" --x 0 --y 1 --z 0 --raw
```

**Skill Documentation**: [AIBridge Skill]({{SKILL_DOC_PATH}})
