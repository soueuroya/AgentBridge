using System.Collections.Generic;
using UnityEngine;

namespace AgentBridge.Core.Registry
{
    /// <summary>
    /// Represents a single available AI action defined in the project. 
    /// These are data-driven via ScriptableObjects.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAgentAction", menuName = "AgentBridge/Agent Action")]
    public class AgentAction : ScriptableObject
    {
        [Tooltip("The unique identifier name for this action.")]
        public string ActionName;

        [TextArea(3, 10)]
        [Tooltip("Detailed description of what this action does, which will be provided to the AI.")]
        public string Description;
        
        [Tooltip("The ContextTypes this action applies to (e.g. 'GameObject', 'Texture'). Use '*' to apply to ALL context types.")]
        public List<string> SupportedContextTypes = new List<string>();
        
        [Tooltip("The parameters this action takes from the AI.")]
        public List<ActionParameter> Parameters = new List<ActionParameter>();
    }
}
