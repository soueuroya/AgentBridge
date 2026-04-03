using System;

namespace AgentBridge.Core.MCP
{
    /// <summary>
    /// Represents an actionable Model Context Protocol (MCP) tool call payload.
    /// This design bypasses free-text chatbots and instead relies entirely on strong typed schemas:
    /// { "tool": "xyz", "input": {...} }
    /// </summary>
    [Serializable]
    public class McpRequest
    {
        /// <summary>
        /// The name of the capability action assigned to the Provider (e.g. "RenameAsset", "ScaleGameObject").
        /// </summary>
        public string tool;

        /// <summary>
        /// The raw context and explicit parameters passed for the provider to parse.
        /// (This uses `object` so it can cleanly serialize dynamically in advanced serializers like Newtonsoft).
        /// </summary>
        public object input;
    }
}
