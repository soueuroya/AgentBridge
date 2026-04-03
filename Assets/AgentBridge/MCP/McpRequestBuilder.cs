using AgentBridge.Core.Interfaces;
using AgentBridge.Core.Registry;

namespace AgentBridge.Core.MCP
{
    /// <summary>
    /// A generic wrapper to serialize both Unity Context and action-specific Arguments into a unified MCP Input JSON block.
    /// </summary>
    [System.Serializable]
    public class McpInputWrapper
    {
        public object context;
        public object arguments;
    }

    /// <summary>
    /// Responsible for adapting Unity Native data and specific Capabilities into strict Model Context Protocol syntax.
    /// Ensures provider-agnostic request logic.
    /// </summary>
    public static class McpRequestBuilder
    {
        /// <summary>
        /// Converts Unity Context and a designated capability exclusively into an MCP compliant request.
        /// Format: { "tool": "ActionName", "input": {...context...} }
        /// </summary>
        public static McpRequest BuildRequest(AgentAction action, IActionContext context)
        {
            return new McpRequest
            {
                tool = action.ActionName,
                input = context.GetRawData() 
            };
        }

        /// <summary>
        /// Safely bundles both Context Data and manual override parameters (arguments) 
        /// into the single MCP 'input' parameter rule without creating free-text bloat.
        /// </summary>
        public static McpRequest BuildRequestWithParameters(AgentAction action, IActionContext context, object customArgs)
        {
            return new McpRequest
            {
                tool = action.ActionName,
                input = new McpInputWrapper
                {
                    context = context.GetRawData(),
                    arguments = customArgs
                }
            };
        }
    }
}
