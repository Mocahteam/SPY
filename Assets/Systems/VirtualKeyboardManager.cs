using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;
using TMPro;
using System.Collections;

/// <summary>
/// Manage virtual keyboard
/// </summary>
public class VirtualKeyboardManager : FSystem {
    private Family f_inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));

	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	public GameObject virtualKeyboard;

	private TMP_InputField selectedInputField;
	private TMP_InputField mirrorInputField;
	private GameObject cancelNextSelectionOfInputField = null;

	protected override void onStart()
    {
		mirrorInputField = virtualKeyboard.GetComponentInChildren<TMP_InputField>();
		selectedInputField = null;
		foreach (GameObject go in f_inputFields)
			addCallback(go);

		f_inputFields.addEntryCallback(addCallback);
	}

	private void addCallback(GameObject input)
    {
		if (input != mirrorInputField.gameObject)
		{
			input.GetComponent<TMP_InputField>().onSelect.AddListener(delegate
			{
				if (PlayerPrefs.GetInt("interaction") == 1 && f_dragging.Count == 0 && input.GetComponent<TMP_InputField>().interactable && f_newEnd.Count == 0) // 0 means mouse/keyboard; 1 means touch-sensitive
				{
					if (cancelNextSelectionOfInputField == input)
						cancelNextSelectionOfInputField = null;
					else
						// on décalle l'ouvertur du clavier pour laisser le temps de voir l'input field actif
						input.GetComponent<TMP_InputField>().StartCoroutine(delayOpenVirtualKeyboard(input));
				}
			});
		}
	}

	private IEnumerator delayOpenVirtualKeyboard(GameObject input)
    {
		yield return new WaitForSeconds(0.75f);
		GameObjectManager.setGameObjectState(virtualKeyboard, true);
		selectedInputField = input.GetComponent<TMP_InputField>();
		mirrorInputField.text = selectedInputField.text;
		mirrorInputField.transform.Find("Text Area").Find("Placeholder").GetComponent<TMP_Text>().text = selectedInputField.transform.Find("Text Area").Find("Placeholder").GetComponent<TMP_Text>().text;
		mirrorInputField.characterValidation = selectedInputField.characterValidation;
		if (mirrorInputField.characterValidation == TMP_InputField.CharacterValidation.Integer)
			GameObjectManager.setGameObjectState(virtualKeyboard.transform.Find("Panel").Find("Alphabet").gameObject, false);
		else
			GameObjectManager.setGameObjectState(virtualKeyboard.transform.Find("Panel").Find("Alphabet").gameObject, true);
		EventSystem.current.SetSelectedGameObject(virtualKeyboard.transform.Find("Panel").Find("Close").gameObject);
	}

	public void closeKeyboard()
	{
		GameObjectManager.setGameObjectState(virtualKeyboard, false);

		if (selectedInputField != null)
		{
			selectedInputField.text = mirrorInputField.text;
			cancelNextSelectionOfInputField = selectedInputField.gameObject; // pour éviter la réouverture du clavier
			EventSystem.current.SetSelectedGameObject(cancelNextSelectionOfInputField);
			selectedInputField.StartCoroutine(delayOnDeselect(selectedInputField)); // Pour une navigation plus fluide et éviter d'avoir à refaire Echap pour sortir de la saisie

			selectedInputField = null;
		}
	}

	private IEnumerator delayOnDeselect(TMP_InputField input)
    {
		yield return new WaitForSeconds(0.5f);
        input.OnDeselect(null);
	}

	public void virtualKeyPressed(string carac)
	{
		if (selectedInputField != null)
		{
			mirrorInputField.text += carac;
			if (mirrorInputField.characterValidation == TMP_InputField.CharacterValidation.Integer)
				while (mirrorInputField.text.StartsWith("0"))
					mirrorInputField.text = mirrorInputField.text.Substring(1);
			selectedInputField.text = mirrorInputField.text;
		}
    }

	public void supprLastCarac()
    {
		if (selectedInputField != null && mirrorInputField.text.Length > 0)
		{
			mirrorInputField.text = mirrorInputField.text.Remove(mirrorInputField.text.Length - 1);
			selectedInputField.text = mirrorInputField.text;
		}
    }
}