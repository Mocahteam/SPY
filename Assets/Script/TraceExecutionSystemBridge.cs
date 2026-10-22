using UnityEngine;

public class TraceExecutionSystemBridge : MonoBehaviour
{
    public void OnActionSelected()
    {
        TraceExecutionSystem.instance.OnActionSelected();
    }
}
