namespace AgentBridge.Editor.Execution
{
    /// <summary>
    /// Represents a parsed JSON tool block that the AI provider suggested executing.
    /// This holds the payload in limbo so the user can review it before conversion into a real IAgentCommand.
    /// </summary>
    [System.Serializable]
    public class SuggestedAction
    {
        public string CommandName;
        
        // Exposing the reasoning logic helps users trust the AI before blindly clicking "Approve".
        public string Reason;
        
        // This caches the exact string values the AI tried to invoke to be parsed by Unity's execution factory later.
        public string RawJsonParameters; 
    }
}
