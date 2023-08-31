using System;
using System.Collections.Generic;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class EditorDialogManagerSystem : FSystem
{
	public GameObject dialogEditPanel;
	public GameObject dialogEditPopup;
	public GameObject viewPortContent;
	public GameObject listEntryPrefab;
	public GameObject selected;
	
	public Toggle moveCameraToggle;
	public InputField dialogTextField;
	public List<InputField> cameraFields;
	public PaintableGrid paintableGrid;

	public static EditorDialogManagerSystem instance;

	public EditorDialogManagerSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		dialogEditPanel.SetActive(false);
	}

	// Use to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}

	public void showPanel()
	{
		if (selected)
		{
			var component = selected.GetComponent<DialogListEntry>();
			dialogTextField.text = component.dialogText;
			moveCameraToggle.isOn = component.cameraMove;
			if (component.cameraMove)
			{
				cameraFields[0].text = component.cameraMoveX.ToString();
				cameraFields[0].text = component.cameraMoveY.ToString();
			}
		}

		dialogTextField.Select();
		dialogTextField.ActivateInputField();
		dialogEditPanel.SetActive(true);
	}

	public void setSelection(GameObject go)
	{
		selected = go;
	}

	public void newButtonPressed()
	{
		dialogTextField.text = "";
		var newEntry = Object.Instantiate(listEntryPrefab, viewPortContent.transform);
		selected = newEntry;
		GameObjectManager.bind(newEntry);

		showPanel();
	}

	public void editButtonPressed()
	{
		if(selected)
			showPanel();
	}

	public void deleteButtonPressed()
	{
		if (selected)
		{
			selected.transform.SetParent(null);
			GameObjectManager.unbind(selected);
			Object.Destroy(selected);
		}

		selected = null;
	}

	public void cancelButtonPressed()
	{
		dialogEditPanel.SetActive(false);
		selected = null;
	}

	public void confirmButtonPressed()
	{
		selected.GetComponentInChildren<TMP_Text>().text = dialogTextField.text;
		var component = selected.GetComponent<DialogListEntry>();
		component.dialogText = dialogTextField.text;
		component.cameraMove = moveCameraToggle.isOn;
		if (component.cameraMove)
		{
			component.cameraMoveX = int.Parse(cameraFields[0].text);
			component.cameraMoveY = int.Parse(cameraFields[1].text);
		}

		dialogEditPanel.SetActive(false);
		selected = null;
	}

	public void checkCoordRange(GameObject go)
	{
		var grid = paintableGrid.grid; 
		var gridsize = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
		
		var dimension = go.name == "CameraX" ? 0 : 1;
		if (string.IsNullOrEmpty(go.GetComponent<InputField>().text))
		{
			go.GetComponent<InputField>().text = "0";
			return;
		}

		var coord = int.Parse(go.GetComponent<InputField>().text);
		coord = Math.Max(0, coord);
		coord = Math.Min(gridsize[dimension] - 1, coord);
		go.GetComponent<InputField>().text = coord.ToString();
	}

	public void dialogToggleChanged()
	{
		foreach (var field in cameraFields)
		{
			field.text = "0";
			field.interactable = moveCameraToggle.isOn;
		}
	}
}