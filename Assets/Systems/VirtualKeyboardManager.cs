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

	protected override void onStart()
    {
		selectedInputField = null;
		foreach (GameObject go in f_inputFields)
			addCallback(go);

		f_inputFields.addEntryCallback(addCallback);
	}

	private void addCallback(GameObject input)
    {
		input.GetComponent<TMP_InputField>().onSelect.AddListener(delegate {
			if (PlayerPrefs.GetInt("interaction") == 1) // 0 means mouse/keyboard; 1 means touch-sensitive
			{
				if (wantToClose)
					wantToClose = false;
				else
				{
					GameObjectManager.setGameObjectState(virtualKeyboard, true);
					selectedInputField = input.GetComponent<TMP_InputField>();
					selectedInputField.caretPosition = selectedInputField.text.Length;
					if (selectedInputField.characterValidation == TMP_InputField.CharacterValidation.Integer)
						GameObjectManager.setGameObjectState(virtualKeyboard.transform.Find("Panel").Find("Alphabet").gameObject, false);
					else
						GameObjectManager.setGameObjectState(virtualKeyboard.transform.Find("Panel").Find("Alphabet").gameObject, true);
				}
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