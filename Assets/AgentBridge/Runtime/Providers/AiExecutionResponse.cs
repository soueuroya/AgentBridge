using System;
using System.Collections.Generic;

namespace AgentBridge.Core.Providers
{
    /// <summary>
    /// Represents the standardized structural response parsed from an AI provider.
    /// Acts as the intermediate DTO before factory translation into an IAgentCommand.
    /// </summary>
    [Serializable]
    public class AiExecutionResponse
    {
        public string command;
        public string targetName;
        public int targetInstanceId;
        public Dictionary<string, string> parameters = new Dictionary<string, string>();
    }
}
