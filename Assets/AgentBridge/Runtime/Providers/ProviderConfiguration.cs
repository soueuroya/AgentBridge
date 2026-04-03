using UnityEngine;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// Configuration settings for connecting to an external AI API endpoint.
    /// </summary>
    [CreateAssetMenu(fileName = "NewProviderConfig", menuName = "AgentBridge/Provider Configuration")]
    public class ProviderConfiguration : ScriptableObject
    {
        [Tooltip("The URL endpoint for the AI provider (e.g. OpenAI, Anthropic, or an LM Studio local server URL).")]
        public string EndpointUrl;

        [Tooltip("Identifier for the model to use, if required by the API (e.g. 'gpt-4o', 'claude-3-opus').")]
        public string ModelName;

        [Tooltip("The API key, if authentication is required. Leave blank for local server instances.")]
        public string ApiKey;

        [Tooltip("Request timeout duration in seconds.")]
        public int TimeoutSeconds = 30;
    }
}
