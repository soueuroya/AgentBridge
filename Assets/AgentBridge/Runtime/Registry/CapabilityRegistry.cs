using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AgentBridge.Core.Registry
{
    /// <summary>
    /// The central registry that maps available Context Types to specific Agent Actions.
    /// In a production environment, this could dynamically load actions from Resources or Addressables.
    /// </summary>
    [CreateAssetMenu(fileName = "CapabilityRegistry", menuName = "AgentBridge/Capability Registry")]
    public class CapabilityRegistry : ScriptableObject
    {
        [Tooltip("A predefined list of all AI actions registered in the system.")]
        public List<AgentAction> RegisteredActions = new List<AgentAction>();

        /// <summary>
        /// Retrieves a filtered list of all actions that map to the provided context type.
        /// </summary>
        /// <param name="contextType">The string ContextType (e.g. 'GameObject', 'Texture').</param>
        /// <returns>An enumerable of matching capabilities.</returns>
        public IEnumerable<AgentAction> GetActionsForContext(string contextType)
        {
            if (string.IsNullOrEmpty(contextType))
            {
                return Enumerable.Empty<AgentAction>();
            }

            return RegisteredActions.Where(action => 
                action != null && 
                action.SupportedContextTypes != null && 
                (action.SupportedContextTypes.Contains("*") || 
                 action.SupportedContextTypes.Contains(contextType)));
        }
    }
}
