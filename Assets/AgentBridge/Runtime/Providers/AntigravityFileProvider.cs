using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// A custom AI Provider that acts as a physical bridge between the Unity Runtime 
    /// and a local AI agent (like Antigravity) running externally with filesystem access.
    /// It writes requests to a JSON file and poles for a response JSON file.
    /// </summary>
    public class AntigravityFileProvider : IAiProvider
    {
        public string Name => "Antigravity Bridge (File-Stream)";

        private static readonly string SYSTEM_DIR = Path.Combine(Directory.GetCurrentDirectory(), ".agentbridge_bridge");
        private static readonly string REQUEST_FILE = Path.Combine(SYSTEM_DIR, "ActiveRequest.json");
        private static readonly string RESPONSE_FILE = Path.Combine(SYSTEM_DIR, "ActiveResponse.json");

        private readonly ProviderConfiguration _config;

        public AntigravityFileProvider(ProviderConfiguration config)
        {
            _config = config;
        }

        public async Task<string> SendRequestAsync(string payload, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(SYSTEM_DIR))
            {
                Directory.CreateDirectory(SYSTEM_DIR);
            }

            // Clean up any old dangling responses
            if (File.Exists(RESPONSE_FILE))
            {
                File.Delete(RESPONSE_FILE);
            }

            // 1. Write the payload out for Antigravity to read
            File.WriteAllText(REQUEST_FILE, payload);
            Debug.Log($"[AntigravityFileProvider] Payload written to {REQUEST_FILE}. Awaiting Antigravity response...");

            // 2. Poll for the AI's response up to the Timeout
            int timeoutMs = (_config != null ? _config.TimeoutSeconds : 60) * 1000;
            int elapsedMs = 0;
            int pollIntervalMs = 500;

            while (elapsedMs < timeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(RESPONSE_FILE))
                {
                    try
                    {
                        // Slight delay to ensure the AI has finished writing the physical file
                        await Task.Delay(100); 
                        string rawResponse = File.ReadAllText(RESPONSE_FILE);
                        
                        // Clean up so the system is ready for the next command
                        File.Delete(REQUEST_FILE);
                        File.Delete(RESPONSE_FILE);
                        
                        return rawResponse;
                    }
                    catch (IOException)
                    {
                        // File might be locked by the AI writing it, try again next tick
                    }
                }

                await Task.Delay(pollIntervalMs, cancellationToken);
                elapsedMs += pollIntervalMs;
            }

            // Cleanup attempt if we timed out
            if (File.Exists(REQUEST_FILE)) File.Delete(REQUEST_FILE);
            
            throw new TimeoutException($"Antigravity failed to write {RESPONSE_FILE} within {timeoutMs / 1000} seconds.");
        }
    }
}
