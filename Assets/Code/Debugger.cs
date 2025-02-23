using UnityEngine;

public class Debugger : MonoBehaviour
{
    public static Debugger Instance;
    public bool enableDebugLogs = true;

    void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("[Debugger] Multiple Debugger instances detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[Debugger] Debugger initialized.");
    }

    
    public void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}
