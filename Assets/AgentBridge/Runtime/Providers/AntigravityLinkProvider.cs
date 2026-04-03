using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// The high-performance implementation of the Antigravity Link.
    /// Redirects AI requests through a local switchboard node (BridgeLinkNode) 
    /// for real-time agentic interaction.
    /// </summary>
    public class AntigravityLinkProvider : IAiProvider
    {
        public string Name => "Antigravity Real-Time Link";

        private readonly ProviderConfiguration _config;
        private readonly string _linkUrl;

        public AntigravityLinkProvider(ProviderConfiguration config)
        {
            _config = config;
            _linkUrl = "http://localhost:11500/"; // New link port (avoids 11411 browser conflict)
        }

        public async Task<string> SendRequestAsync(string payload, CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(_linkUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // Our link server manages the handshake internally. 
                // It will only respond once the AI has finished its work.
                webRequest.timeout = _config != null ? _config.TimeoutSeconds : 600;

                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        webRequest.Abort();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    throw new Exception($"[Antigravity Link Error] {webRequest.error}. Ensure the BridgeLinkNode service is running in the background.");
                }

                return webRequest.downloadHandler.text;
            }
        }
    }
}
