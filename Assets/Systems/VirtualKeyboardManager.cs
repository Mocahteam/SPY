using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;
using TMPro;
using System.Runtime.InteropServices;

/// <summary>
/// Manage virtual keyboard
/// </summary>
public class VirtualKeyboardManager : FSystem {
    private Family f_inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));

	public GameObject virtualKeyboard;

	private TMP_InputField selectedInputField;
	private bool wantToClose = false;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	protected override void onStart()
    {
		selectedInputField = null;
		if (Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser())
		{
			foreach (GameObject go in f_inputFields)
				addCallback(go);

			f_inputFields.addEntryCallback(addCallback);
		}
	}

	private void addCallback(GameObject input)
    {
		input.GetComponent<TMP_InputField>().onSelect.AddListener(delegate {
			if (wantToClose)
				wantToClose = false;
			else
			{
				GameObjectManager.setGameObjectState(virtualKeyboard, true);
				selectedInputField = input.GetComponent<TMP_InputField>();
				selectedInputField.caretPosition = selectedInputField.text.Length;
			}
		});
	}

	public void closeKeyboard()
	{
		GameObjectManager.setGameObjectState(virtualKeyboard, false);
		wantToClose = true;

		if (selectedInputField != null)
		{
			EventSystem.current.SetSelectedGameObject(selectedInputField.gameObject);
			selectedInputField = null;
		}
	}

	public void virtualKeyPressed(string carac)
	{
		if (selectedInputField != null)
		{
			selectedInputField.text += carac;
		}
    }

	public void supprLastCarac()
    {
		if (selectedInputField != null && selectedInputField.text.Length > 0)
		{
			selectedInputField.text = selectedInputField.text.Remove(selectedInputField.text.Length - 1);
		}
    }
}