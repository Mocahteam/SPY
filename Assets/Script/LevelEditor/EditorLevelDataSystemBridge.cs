using UnityEngine;

public class EditorLevelDataSystemBridge : MonoBehaviour
{
	public void hideToggleChanged(bool newState)
	{
		EditorLevelDataSystem.instance.hideToggleChanged(gameObject, newState);
	}

	public void limitToggleChanged(bool newState)
	{
		EditorLevelDataSystem.instance.limitToggleChanged(gameObject, newState);
	}
}