using UnityEngine;

public class EditorDialogManagerSystemBridge : MonoBehaviour{
	public void selectionMade(GameObject go)
	{
		EditorDialogManagerSystem.instance.setSelection(go);
	}
}