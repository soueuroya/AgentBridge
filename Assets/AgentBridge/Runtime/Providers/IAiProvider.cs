using System.Threading;
using System.Threading.Tasks;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// Base abstraction for any AI model provider.
    /// Guarantees that the system does not hardcode vendor-specific APIs.
    /// </summary>
    public interface IAiProvider
    {
        string Name { get; }
        
        /// <summary>
        /// Sends a generic string payload (usually JSON-RPC for MCP) to the AI Provider,
        /// returning the string response asynchronously.
        /// </summary>
        Task<string> SendRequestAsync(string payload, CancellationToken cancellationToken = default);
    }
}
