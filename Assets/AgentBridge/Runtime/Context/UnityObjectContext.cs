using UnityEngine;
using AgentBridge.Core.Interfaces;

namespace AgentBridge.Core.Context
{
    /// <summary>
    /// A generic IActionContext wrapper for any Unity Object.
    /// This provides a standardized way to pass Unity asset metadata into the MCP Request Builder.
    /// </summary>
    public class UnityObjectContext : IActionContext
    {
        public string ContextType { get; }
        private readonly Object _target;

        public UnityObjectContext(Object target)
        {
            _target = target;
            ContextType = target != null ? target.GetType().Name : "Null";
        }

        public object GetRawData()
        {
            if (_target == null) return null;

            // Simple serialization wrapper. For more complex types like GameObjects, 
            // the AI will eventually need component lists, which we can expand here.
            return new {
                name = _target.name,
                type = ContextType,
                instanceId = _target.GetInstanceID()
            };
        }
    }
}
