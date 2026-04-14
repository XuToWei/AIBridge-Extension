---
name: aibridge
description: "Unity CLI Tool. Execute compile, asset search, gameobject manipulation, transform operations, component inspection, scene/prefab management, screenshot capture, and GIF recording. Supports multi-command execution and runtime extension."
commands: [compile, asset, gameobject, transform, inspector, selection, scene, prefab, screenshot, get_logs, focus, batch, multi, menu_item, editor]
capabilities: [asset-lookup, scene-editing, build-automation, visual-verification, component-inspection, hierarchy-manipulation, prefab-management, console-monitoring, editor-control]
triggers: [unity, compile, gameobject, transform, component, scene, prefab, screenshot, gif, console, log, asset, hierarchy, inspector, selection, menu, editor, focus, batch]
---

# AI Bridge Unity Skill

## AI Operating Rules

**Compile Priority:**
- Use `compile unity` (default) - requires Unity Editor running
- Use `compile dotnet` (optional) - separate solution-build validation only, NOT a fallback

**Asset Lookup Priority:**
1. `asset search/find --format paths` (Unity AssetDatabase index - fastest)
2. `asset get_path` (only when starting from GUID)
3. `asset load` (only for metadata confirmation)
4. Host AI native file-read tool (for file contents)
5. `asset read_text` (fallback when native reads unavailable)
6. Generic grep/filesystem search (last resort)

**Special Constraints:**
- `focus` - Windows-only, CLI-only, triggers Unity refresh/compile
- `screenshot` - Requires Play Mode
- `multi` - Preferred for batch operations

---

## Invocation

**CLI Path:** `./AIBridgeCache/CLI/AIBridgeCLI.exe` (run from Unity project root)

**Alias (used in examples below):** `$CLI`

**OS Syntax:**
- Windows: `./AIBridgeCache/CLI/AIBridgeCLI.exe <command> <action> [options]`
- macOS/Linux: `dotnet ./AIBridgeCache/CLI/AIBridgeCLI.dll <command> <action> [options]`
- PowerShell: `& "./AIBridgeCache/CLI/AIBridgeCLI.exe" <command> <action> [options]`

**Global Options:**
- `--timeout <ms>` - Timeout (default: 5000)
- `--raw` / `--pretty` - JSON output (default: raw)
- `--json <json>` / `--stdin` - Complex parameters
- `--help` - Show help

**Cache Directory:** `AIBridgeCache/` (commands, results, screenshots)

---

## Command Reference

### 0. `focus` - Bring Unity to Foreground

CLI-only, Windows-only. Triggers Unity refresh/compile via Windows API.

```bash
$CLI focus
# Output: {"Success":true,"ProcessId":1234,"WindowTitle":"MyProject - Unity 6000.0.51f1","Error":null}
```

### 1. `editor` - Editor Control

```bash
$CLI editor log --message "Hello" [--logType Warning|Error]
$CLI editor undo [--count 3]
$CLI editor redo
$CLI editor compile  # Simple compile, use `compile` command for full features
$CLI editor refresh [--forceUpdate true]
$CLI editor play|stop|pause
$CLI editor get_state
```

### 1.1 `compile` - Compilation Operations

```bash
$CLI compile unity  # Default (requires Unity Editor)
$CLI compile dotnet [--solution MyGame.sln]  # Optional validation

# Output: {"success":true,"status":"success","duration":5.2,"errorCount":0,"warningCount":3,...}
```

**Unity compile response fields:**

| Field | Description |
|-------|-------------|
| `success` | Whether build succeeded |
| `status` | "success", "failed", "idle", or "timeout" |
| `duration` | Build duration in seconds |
| `errorCount` | Number of errors |
| `warningCount` | Number of warnings |
| `errors` | Array of error details (file, line, column, code, message) |
| `warnings` | Array of warning details (limited to 20) |

**Unity compile parameters:**

| Parameter | Description | Default |
|-----------|-------------|---------|
| `--timeout` | Total compilation timeout in ms | `120000` |
| `--poll-interval` | Status polling interval in ms | `500` |
| `--transport-timeout` | Single command round-trip timeout in ms | `min(30000, timeout)` |

**Dotnet compile parameters (explicit solution build check):**

| Parameter | Description | Default |
|-----------|-------------|---------|
| `--solution` | Solution file path. If omitted, auto-detect from project root; if ambiguous, pass explicitly | auto-detect |
| `--configuration` | Build configuration | `Debug` |
| `--verbosity` | MSBuild verbosity | `minimal` |
| `--timeout` | Timeout in ms | `300000` |
| `--no-filter` | Disable error filtering | `false` |
| `--exclude` | Custom exclude paths (comma separated) | - |

**NOTE**:

- `compile unity` requires Unity Editor to be running, automatically polls for completion
- If Unity is already compiling or temporarily busy, `compile unity` will attach to the current compilation and keep polling until a final result or the outer timeout is reached
- `compile unity` does not fall back to `dotnet build`; Unity compile and solution build are intentionally separate validations
- `--timeout` controls the full compile wait window, while `--transport-timeout` controls each CLI↔Unity communication attempt
- `compile dotnet` runs independently without Unity, auto-detects a single root-level `.sln` or `.slnx` when `--solution` is omitted, and has intelligent error filtering
- Use `compile start` and `compile status` for low-level manual compilation control

### 2. `gameobject` - GameObject Operations

```bash
$CLI gameobject create --name "Cube" --primitiveType Cube [--parentPath "Parent"]
$CLI gameobject destroy --path "Cube" [--instanceId 12345]
$CLI gameobject find --name "Player" [--tag "Enemy"] [--withComponent "BoxCollider"] [--maxResults 10]
$CLI gameobject set_active --path "Player" --active false [--toggle true]
$CLI gameobject rename --path "OldName" --newName "NewName"
$CLI gameobject duplicate --path "Original"
$CLI gameobject get_info --path "Player"
```

### 3. `transform` - Transform Operations

```bash
$CLI transform get --path "Player"
$CLI transform set_position --path "Player" --x 0 --y 1 --z 0 [--local true]
$CLI transform set_rotation --path "Player" --x 0 --y 90 --z 0
$CLI transform set_scale --path "Player" --x 2 --y 2 --z 2 [--uniform 2]
$CLI transform set_parent --path "Child" --parentPath "Parent"
$CLI transform look_at --path "Player" --targetPath "Enemy" [--targetX 0 --targetY 0 --targetZ 10]
$CLI transform reset --path "Player"
$CLI transform set_sibling_index --path "Child" --index 0 [--first true]
```

### 4. `inspector` - Component Operations

```bash
$CLI inspector get_components --path "Player"
$CLI inspector get_properties --path "Player" --componentName "Transform"
$CLI inspector set_property --path "Player" --componentName "Rigidbody" --propertyName "mass" --value 10
$CLI inspector add_component --path "Player" --typeName "Rigidbody"
$CLI inspector remove_component --path "Player" --componentName "Rigidbody"
```

### 5. `selection` - Selection Operations

```bash
$CLI selection get [--includeComponents true]
$CLI selection set --path "Player" [--assetPath "Assets/Prefabs/Player.prefab"]
$CLI selection clear
$CLI selection add --path "Enemy1"
$CLI selection remove --path "Enemy1"
```

### 6. `scene` - Scene Operations

```bash
$CLI scene load --scenePath "Assets/Scenes/Main.unity" [--mode additive]
$CLI scene save [--saveAs "Assets/Scenes/NewScene.unity"]
$CLI scene get_hierarchy [--depth 3] [--includeInactive false]
$CLI scene get_active
$CLI scene new [--setup empty]
```

### 7. `prefab` - Prefab Operations

```bash
$CLI prefab instantiate --prefabPath "Assets/Prefabs/Player.prefab" [--posX 5 --posY 0 --posZ 0]
$CLI prefab save --gameObjectPath "Player" --savePath "Assets/Prefabs/Player.prefab"
$CLI prefab unpack --gameObjectPath "Player(Clone)" [--completely true]
$CLI prefab get_info --prefabPath "Assets/Prefabs/Player.prefab"
$CLI prefab get_hierarchy --prefabPath "Assets/Prefabs/Player.prefab" [--depth 4] [--includeInactive false]
$CLI prefab apply --gameObjectPath "Player(Clone)"
```

### 8. `asset` - AssetDatabase Operations

```bash
# Search (recommended - Unity AssetDatabase index)
$CLI asset search --mode script --keyword "Player" --format paths
$CLI asset search --mode prefab --keyword "UI" --format paths
$CLI asset search --filter "t:ScriptableObject" --format paths
# Modes: all, prefab, scene, script, texture, material, audio, animation, shader, font, model, so

# Find (precise control)
$CLI asset find --filter "t:Prefab" --format paths [--searchInFolders "Assets/Textures"] [--maxResults 50]

# Import / Refresh
$CLI asset import --assetPath "Assets/Textures/icon.png"
$CLI asset refresh

# Get Path / Load Metadata
$CLI asset get_path --guid "abc123..."
$CLI asset load --assetPath "Assets/Prefabs/Player.prefab"

# Fallback: read text (use host AI native file-read tool first)
$CLI asset read_text --assetPath "Assets/Scripts/Player.cs" --startLine 1 --maxLines 120
```

**Note:** `format=paths` returns Unity asset paths only (efficient). `format=full` returns asset objects with metadata.

### 9. `menu_item` - Invoke Menu Item

```bash
$CLI menu_item --menuPath "GameObject/Create Empty"
$CLI menu_item --menuPath "Assets/Create/Folder"
```

### 10. `get_logs` - Get Console Logs

```bash
$CLI get_logs [--count 100] [--logType Error|Warning]
```

### 11. `batch` - Batch Commands

```bash
$CLI batch execute --commands '[{"type":"editor","params":{"action":"log","message":"Step 1"}}]'
$CLI batch from_file --file "commands.json"
```

### 12. `multi` - Execute Multiple Commands (Recommended)

```bash
$CLI multi --cmd 'editor log --message Step1&gameobject create --name Cube --primitiveType Cube'
$CLI multi --stdin  # Read from stdin (one per line)
```

---

**Skill Version**: 1.0
**Package**: cn.lys.aibridge
