using UnityEngine;
using UnityEngine.UI;

public class OpenFileSystemBridge : MonoBehaviour
{
    public void OnLevelSelected()
    {
        OpenFileSystem.instance.OnLevelSelected(gameObject);
    }

    public void LoadLevel()
    {
        gameObject.transform.parent.parent.parent.parent.Find("Buttons/LoadButton").GetComponent<Button>().onClick.Invoke();
    }
}
