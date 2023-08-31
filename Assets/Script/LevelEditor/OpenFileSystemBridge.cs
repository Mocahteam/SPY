using UnityEngine;
using UnityEngine.UI;

public class OpenFileSystemBridge : MonoBehaviour
{
    public void onLevelSelected()
    {
        OpenFileSystem.instance.onLevelSelected(gameObject);
    }

    public void loadLevel()
    {
        gameObject.transform.parent.parent.parent.parent.Find("Buttons").Find("LoadButton").GetComponent<Button>().onClick.Invoke();
    }
}
