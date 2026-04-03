using System.Threading;
using System.Threading.Tasks;

namespace AgentBridge.Core.Interfaces
{
    /// <summary>
    /// Represents the client side of the Model Context Protocol (MCP) connection.
    /// Handles connecting to an MCP server, sending JSON-RPC requests, and returning responses.
    /// </summary>
    public interface IMcpClient
    {
        bool IsConnected { get; }
        
        /// <summary>
        /// Attempts to connect to the configured MCP server.
        /// </summary>
        Task ConnectAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Disconnects from the MCP server.
        /// </summary>
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        
        // Future extensions for Request/Notification handling will go here, e.g.:
        // Task<string> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken = default);
    }
}
