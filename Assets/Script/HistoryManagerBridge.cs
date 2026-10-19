using UnityEngine;

public class HistoryManagerBridge : MonoBehaviour
{
    public void undo()
    {
        HistoryManager.instance.undo();
    }

    public void redo()
    {
        HistoryManager.instance.redo();
    }
}
