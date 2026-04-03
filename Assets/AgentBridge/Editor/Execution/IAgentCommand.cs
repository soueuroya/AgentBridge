namespace AgentBridge.Editor.Execution
{
    /// <summary>
    /// Base interface for any automated action the AI attempts to perform inside the Unity Editor.
    /// Strongly forces all commands to run through a standardized safety protocol.
    /// </summary>
    public interface IAgentCommand
    {
        string CommandName { get; }
        
        /// <summary>
        /// Indicates if this command has destructive potential (e.g. modifying asset structures permanently).
        /// </summary>
        bool IsDangerous { get; }

        /// <summary>
        /// Always called before execution. Ensures context states or parameters are perfectly safe.
        /// </summary>
        bool Validate(out string errorMessage);

        /// <summary>
        /// Applies the action. MUST register inside Undo.RegisterCompleteObjectUndo (or similar Native Editor tools) where applicable.
        /// </summary>
        void Execute();
    }
}
