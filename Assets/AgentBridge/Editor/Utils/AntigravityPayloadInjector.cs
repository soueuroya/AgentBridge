using UnityEditor;
using UnityEngine;

public static class AntigravityPayloadInjector
{
    [MenuItem("Window/AgentBridge/Tests/Trigger Payload Injection")]
    public static void InjectNow()
    {
        Debug.Log("[Antigravity] Injecting component payload directly into the target...");
        GameObject target = GameObject.Find("player_character_finalV3_NO_COMPONENTS");
        
        if (target != null)
        {
            if (target.GetComponent<Rigidbody>() == null)
            {
                var rb = target.AddComponent<Rigidbody>();
                rb.mass = 75f; // Standard adult weight
                Debug.Log("[Antigravity] Attached Rigidbody.");
            }
            if (target.GetComponent<CapsuleCollider>() == null)
            {
                var col = target.AddComponent<CapsuleCollider>();
                col.height = 2f;
                Debug.Log("[Antigravity] Attached CapsuleCollider.");
            }
            if (target.GetComponent<CharacterController>() == null)
            {
                target.AddComponent<CharacterController>();
                Debug.Log("[Antigravity] Attached CharacterController for standard movement.");
            }
            
            // Clean up the name contextually
            target.name = "PlayerCharacter_Resolved";
            
            Debug.Log("[Antigravity] Payload successfully resolved!");
        }
        else
        {
            Debug.LogError("[Antigravity] Target object could not be found in the current scene.");
        }
    }
}
