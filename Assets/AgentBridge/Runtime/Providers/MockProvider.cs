using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// A mock AI provider for safely testing AgentBridge tools internally 
    /// without sending queries over the network or spending API credits.
    /// </summary>
    public class MockProvider : IAiProvider
    {
        public string Name => "Mock Testing Provider";

        public async Task<string> SendRequestAsync(string payload, CancellationToken cancellationToken = default)
        {
            // Simulating an asynchronous network delay cleanly inside Unity editor boundaries
            await Task.Delay(1000, cancellationToken);
            
            Debug.Log($"[MockProvider] Simulated request received payload of length: {payload.Length}");
            
            // Returns a hardcoded JSON snippet pretending to be a successful reply.
            return "{\"status\": \"mock_success\", \"message\": \"This is a simulated AI response.\"}";
        }
    }
}
