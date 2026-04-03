using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// A generic HTTP implementation of an AI provider.
    /// Can be utilized to connect to standard OpenAI-compatible or direct proxy endpoints.
    /// </summary>
    public class GenericHttpProvider : IAiProvider
    {
        public string Name => "Generic HTTP Provider";

        private readonly ProviderConfiguration _config;

        public GenericHttpProvider(ProviderConfiguration config)
        {
            _config = config;
        }

        public async Task<string> SendRequestAsync(string payload, CancellationToken cancellationToken = default)
        {
            if (_config == null || string.IsNullOrEmpty(_config.EndpointUrl))
            {
                throw new InvalidOperationException("Provider configuration or Endpoint is missing.");
            }

            using (UnityWebRequest webRequest = new UnityWebRequest(_config.EndpointUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(_config.ApiKey))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {_config.ApiKey}");
                }

                webRequest.timeout = _config.TimeoutSeconds;

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
                    throw new Exception($"HTTP Error {webRequest.responseCode}: {webRequest.error}\nDetails: {webRequest.downloadHandler.text}");
                }

                return webRequest.downloadHandler.text;
            }
        }
    }
}
