using UnityEngine;

public class EditorLevelDataSystemBridge : MonoBehaviour
{
	public void HideToggleChanged(bool newState)
	{
		EditorLevelDataSystem.instance.HideToggleChanged(gameObject, newState);
	}

	public void LimitToggleChanged(bool newState)
	{
		EditorLevelDataSystem.instance.LimitToggleChanged(gameObject, newState);
	}
}