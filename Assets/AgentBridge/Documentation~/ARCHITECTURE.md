# Architecture Overview

AgentBridge is specifically built upon a decoupled interface pattern to cleanly separate dangerous Unity Editor state mutations from asynchronous AI Network parsing.

## 1. The Context Engine
Instead of writing scattered scripts that scrape parameters haphazardly, the system relies cleanly upon `IActionContext`.
- **Runtime Component:** Variables are strongly typed inside purely serializable objects (like `ProjectScanSummary`), entirely bypassing Unity Editor namespace compilation limitations (allowing cleanly isolated JSON generation).
- **Editor Component:** UI windows track exactly what the user clicks via native hooks like `Selection.selectionChanged` and maps that Object into structured datasets (avoiding free-form string context generation).

## 2. The Capability Mapping Registry
AI action intent is strictly data-driven via Unity **ScriptableObjects**, meaning no code recompilation is required to add AI skills!
- `AgentAction` configs dictate exactly what contexts they apply to (using strings like `"GameObject"`, `"Texture"`, or `"*"`.
- They outline mandatory variables via `ActionParameter`. 
- Frontend user interfaces immediately query `CapabilityRegistry.GetActionsForContext()` preventing the AI from attempting logic errors like "Add a Rigidbody" to an isolated Audio file.

## 3. The Execution Engine
Commands *must* implement `IAgentCommand` (and optionally `IPreviewableCommand`). Inbound AI JSON strings are deserialized and factory-mapped cleanly back into these C# structs securely.
- **Validation Blocks**: All commands force a `Validate()` pass. If the target string or AssetDatabase path is hallucinated/null, the task is destroyed flawlessly.
- **Whitelist Strictness**: The `CommandExecutionEngine` maintains an explicit array of allowable strings. If an AI hallucinates the capability `DeleteRootProject`, it fails instantly at the interceptor whitelist.
- **Undo Reliability**: All targeted commands package their physical Editor operations underneath `Undo.RecordObject` or similar tracking buffers perfectly.
- **Visual Diffing System**: Passing logic through the `ActionPreviewWindow` prevents zero-click executions, forcing human developers to physically approve Before/After models explicitly showing intent!

## 4. MCP Provider Serialization
To protect generic AI proxy calls, everything filters down natively into `Assets/AgentBridge/MCP/McpRequestBuilder.cs`, mapping your highly specific Unity configurations explicitly into the standard `{ tool: string, input: object }` JSON constraints without conversational markdown trailing.
