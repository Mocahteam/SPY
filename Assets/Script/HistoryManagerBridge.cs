using UnityEngine;

public class HistoryManagerBridge : MonoBehaviour
{
    public void Undo()
    {
        HistoryManager.instance.Undo();
    }

    public void Redo()
    {
        HistoryManager.instance.Redo();
    }
}
