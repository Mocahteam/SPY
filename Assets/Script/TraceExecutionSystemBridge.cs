using UnityEngine;

public class TraceExecutionSystemBridge : MonoBehaviour
{
    public void onActionSelected()
    {
        TraceExecutionSystem.instance.onActionSelected();
    }
}
