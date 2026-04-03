using System.Threading;
using System.Threading.Tasks;

namespace AgentBridge.Core.Interfaces
{
    /// <summary>
    /// Abstraction for an AI model provider (e.g., Anthropic, OpenAI, Local LLM).
    /// Responsible for taking context and generating a response or action intent.
    /// </summary>
    public interface IAgentProvider
    {
        string ProviderName { get; }
        
        /// <summary>
        /// Sends a prompt and optional context to the AI provider.
        /// </summary>
        /// <param name="prompt">The user's prompt or the system's objective.</param>
        /// <param name="context">Optional context data (e.g., currently selected objects).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A string response from the agent (which might be a JSON payload for tools).</returns>
        Task<string> ExecutePromptAsync(string prompt, IActionContext context = null, CancellationToken cancellationToken = default);
    }
}
