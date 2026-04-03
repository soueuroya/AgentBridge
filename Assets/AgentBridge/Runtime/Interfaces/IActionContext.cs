namespace AgentBridge.Core.Interfaces
{
    /// <summary>
    /// Represents a strongly-typed piece of context from the Unity Editor 
    /// (e.g., selected GameObject, active scene, selected audio clip).
    /// </summary>
    public interface IActionContext
    {
        /// <summary>
        /// A string identifying the type of context this is (e.g., "GameObject", "Texture").
        /// </summary>
        string ContextType { get; }
        
        /// <summary>
        /// Returns the raw typed data object representing the context state.
        /// This is used directly by the MCP Request Builder as the JSON 'input', actively forbidding unstructured prompts.
        /// </summary>
        object GetRawData();
    }
}
