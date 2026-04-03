using System.Threading;
using System.Threading.Tasks;

namespace AgentBridge.Core.Interfaces
{
    /// <summary>
    /// Represents an MCP Server hosted within or managed by Unity.
    /// This allows external applications to read Unity's tools and resources.
    /// </summary>
    public interface IMcpServer
    {
        bool IsRunning { get; }
        
        /// <summary>
        /// Starts the MCP Server, listening for incoming JSON-RPC connections (e.g., via stdio or SSE).
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Stops the MCP Server and closes active connections.
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
