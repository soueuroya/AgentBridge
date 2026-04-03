namespace AgentBridge.Editor.Execution
{
    /// <summary>
    /// Forces a command to expose its delta intent strings before physically altering the Editor.
    /// This prevents AI hallucinations from hiding destructive changes inside blackbox variables.
    /// </summary>
    public interface IPreviewableCommand : IAgentCommand
    {
        /// <summary>
        /// Reads the specific properties of the target *prior* to mutation.
        /// </summary>
        string GetBeforeState();

        /// <summary>
        /// Projects the exact change the AI has modeled *after* the command executes.
        /// </summary>
        string GetAfterState();
    }
}
