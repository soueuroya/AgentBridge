# Model Context Protocol (MCP) Integration Specification

AgentBridge enforces communication strictly conforming to standard MCP framework layouts. Because Unity requires massive variables natively, raw string prompts completely degrade AI consistency resulting in Unity Editor errors. We circumvent this constraint entirely through structural formatting.

## Request Execution Structure
By implementing the `McpRequest.cs` structural wrapper, standard Unity JSON serializers are forced to bind AI capability constraints directly to explicitly named endpoints without conversational logic hooks:

```json
{
  "tool": "RenameGameObjectCommand",
  "input": {
    "targetProperties": {
      "name": "OldPlayerNode",
      "type": "GameObject"
    },
    "newNameArgument": "PlayerRoot_Refactored"
  }
}
```

### Why we strictly forbid Free-Text Arrays
Feeding instructions natively as strings (e.g., *"Rename this item to PlayerRoot"*) frequently breaks automated parsing pipelines because AI providers wrap execution code in arbitrary markdown formats (`"Sure! Here is the execution: ..."`). 

By structuring the payload accurately to native tooling parameters, providers will simply output rigid JSON values straight into the Execution factories, dropping markdown blocks!

## Extending MCP Action Parameters
If you are contributing additional `AgentAction` ScriptableObjects for a custom AI tool capability:
1. Identify exactly what parameter the internal Editor mechanism requires (e.g., `isReadable` bool for a Texture tool).
2. Expose it plainly to the frontend via the `AgentAction.Parameters` list.
3. The internal `McpRequestBuilder` handles wrapping your manual override attributes side-by-side perfectly within the unified JSON `"input"` tree structure natively!

## Payload Batch Compression
When mass-selecting items in Unity, `AgentBridge` pushes the logic into the `BatchProcessorWindow.cs`. It compresses repeated data overhead aggressively by generating single large schema blocks: 

```json
{
   "tool": "ExecuteBatchRenaming",
   "input": {
      "itemArray": [
         { "original": "Cube1" },
         { "original": "Cube2" },
         { "original": "Cube3" }
      ]
   }
}
```
*Tip: Always guarantee your bound AI provider is designed natively to interpret density tokens successfully before feeding it highly compressed array blocks in Batch Mode!*
