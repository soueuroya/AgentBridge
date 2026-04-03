using System;
using UnityEngine;

namespace AgentBridge.Core.Registry
{
    /// <summary>
    /// Represents the data type of a parameter for an AI action.
    /// </summary>
    public enum ParameterType
    {
        String,
        Integer,
        Float,
        Boolean
    }

    /// <summary>
    /// Defines a parameter that an AI action can accept or require.
    /// </summary>
    [Serializable]
    public class ActionParameter
    {
        [Tooltip("The programmatic name of the parameter.")]
        public string Name;

        [Tooltip("A description of what the parameter does, used to inform the AI.")]
        public string Description;

        [Tooltip("The expected data type of the parameter.")]
        public ParameterType Type = ParameterType.String;

        [Tooltip("Whether the AI must provide this parameter, or if it is optional.")]
        public bool IsRequired = true;
    }
}
