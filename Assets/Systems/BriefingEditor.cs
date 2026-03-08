using FYFY;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BriefingEditor : FSystem
{
	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public Transform editBriefingPanel;
	public GameObject briefingItemPrefab;

	private GameObject currentBriefingEdit;
	private DataLevelBehaviour overridedBriefing;

	private GameData gameData;

	public static BriefingEditor instance;

	public BriefingEditor()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		Pause = true;
	}

	// see pen in hookedMission prefab and EditBriefing in MissionEditor
	public void prepareBriefingsEditor(DataLevelBehaviour dataLevel, GameObject src)
	{
		if (dataLevel == null)
			Debug.LogError("Missing DataLevelBehaviour component");
		else
		{
			// sauvegarder le bouton qui a appelé la fenêtre afin de pouvoir y remettre le focus dessus à la fin de l'édition
			currentBriefingEdit = src;

			overridedBriefing = dataLevel;
			// remove all old briefing items
			Transform viewportContent = editBriefingPanel.Find("Scroll View").GetChild(0).GetChild(0);
			while (viewportContent.childCount > 3)
			{
				Transform child = viewportContent.GetChild(viewportContent.childCount - 1);
				GameObjectManager.unbind(child.gameObject);
				child.SetParent(null);
				GameObject.Destroy(child.gameObject);
			}

			foreach (TMP_InputField input in editBriefingPanel.GetComponentsInChildren<TMP_InputField>(true))
			{
				if (input.gameObject.name == "MissionPathContent")
					input.text = dataLevel.data.filePath;
				if (input.gameObject.name == "NameContent")
					input.text = dataLevel.data.missionName;
			}

			if (dataLevel.data.overridedDialogs == null)
			{
				XmlNode levelSelected = gameData.levels[dataLevel.data.filePath];
				dataLevel.data.overridedDialogs = new List<Dialog>();
				XmlNodeList XMLDialogs = levelSelected.OwnerDocument.GetElementsByTagName("dialogs");
				if (XMLDialogs.Count > 0)
					Utility.readXMLDialogs(XMLDialogs[0], dataLevel.data.overridedDialogs);
			}

			// add briefing items
			foreach (Dialog dialog in dataLevel.data.overridedDialogs)
			{
				GameObject newItem = GameObject.Instantiate(briefingItemPrefab, viewportContent, false);
				GameObjectManager.bind(newItem);
				foreach (TMP_InputField input in newItem.GetComponentsInChildren<TMP_InputField>(true))
				{
					if (input.name == "Text_input" && dialog.text != null)
						input.text = dialog.text;
					else if (input.name == "ImgPath_input" && dialog.img != null)
						input.text = dialog.img;
					else if (input.name == "ImgDesc_input" && dialog.imgDesc != null)
						input.text = dialog.imgDesc;
					else if (input.name == "ImgSize_input" && dialog.imgHeight != -1)
						input.text = "" + dialog.imgHeight;
					else if (input.name == "CamX_input" && dialog.camX != -1)
						input.text = "" + dialog.camX;
					else if (input.name == "CamY_input" && dialog.camY != -1)
						input.text = "" + dialog.camY;
					else if (input.name == "SoundPath_input" && dialog.sound != null)
						input.text = dialog.sound;
					else if (input.name == "VideoPath_input" && dialog.video != null)
						input.text = dialog.video;
					else if (input.name == "VideoSize_input" && dialog.videoHeight != -1)
						input.text = "" + dialog.videoHeight;
					else
						input.text = "";
				}
				newItem.GetComponentInChildren<Toggle>().isOn = dialog.enableInteraction;
				newItem.GetComponentInChildren<TMP_Dropdown>().value = dialog.briefingType;
			}
		}
	}

	public void saveBriefings()
	{
		if (overridedBriefing != null)
		{
			foreach (TMP_InputField input in editBriefingPanel.GetComponentsInChildren<TMP_InputField>(true))
			{
				if (input.gameObject.name == "MissionPathContent")
					overridedBriefing.data.filePath = input.text;
				if (input.gameObject.name == "NameContent")
					overridedBriefing.data.missionName = input.text.Replace('\"', '\'');
			}

			// save briefing items
			overridedBriefing.data.overridedDialogs = new List<Dialog>();
			Transform viewportContent = editBriefingPanel.Find("Scroll View").GetChild(0).GetChild(0);
			for (int i = 3; i < viewportContent.childCount; i++)
			{
				Transform child = viewportContent.GetChild(i);
				Dialog dialog = new Dialog();
				foreach (TMP_InputField input in child.GetComponentsInChildren<TMP_InputField>(true))
				{
					if (input.name == "Text_input" && input.text != "")
						dialog.text = input.text.Replace('\"', '\'');
					else if (input.name == "ImgPath_input" && input.text != "")
						dialog.img = input.text.Replace('\"', '\'');
					else if (input.name == "ImgDesc_input" && input.text != "")
						dialog.imgDesc = input.text.Replace('\"', '\'');
					else if (input.name == "ImgSize_input" && input.text != "")
						dialog.imgHeight = float.Parse(input.text);
					else if (input.name == "CamX_input" && input.text != "")
						dialog.camX = int.Parse(input.text);
					else if (input.name == "CamY_input" && input.text != "")
						dialog.camY = int.Parse(input.text);
					else if (input.name == "SoundPath_input" && input.text != "")
						dialog.sound = input.text.Replace('\"', '\'');
					else if (input.name == "VideoPath_input" && input.text != "")
						dialog.video = input.text.Replace('\"', '\'');
					else if (input.name == "VideoSize_input" && input.text != "")
						dialog.videoHeight = float.Parse(input.text);
				}
				dialog.enableInteraction = child.GetComponentInChildren<Toggle>().isOn;
				dialog.briefingType = child.GetComponentInChildren<TMP_Dropdown>().value;
				overridedBriefing.data.overridedDialogs.Add(dialog);
			}

			// remettre le focus sur le bouton qui a appelé la fenêtre
			if (currentBriefingEdit != null)
				EventSystem.current.SetSelectedGameObject(currentBriefingEdit);
			currentBriefingEdit = null;
		}
	}

	public void addNewBriefing(GameObject parent)
	{
		GameObject newItem = GameObject.Instantiate(briefingItemPrefab, parent.transform, false);
		GameObjectManager.bind(newItem);
	}

	public void removeItemFromParent(GameObject go)
	{
		if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(go.transform))
		{
			if (go.transform.parent.childCount > 1)
			{
				if (go.transform.GetSiblingIndex() < go.transform.parent.childCount - 1)
					EventSystem.current.SetSelectedGameObject(go.transform.parent.GetChild(go.transform.GetSiblingIndex() + 1).gameObject.GetComponentInChildren<Button>().gameObject);
				else
					EventSystem.current.SetSelectedGameObject(go.transform.parent.GetChild(go.transform.GetSiblingIndex() - 1).gameObject.GetComponentInChildren<Button>().gameObject);
			}
			else
			{
				if (f_buttons.Count > 0)
					EventSystem.current.SetSelectedGameObject(f_buttons.First());
			}
		}
		GameObjectManager.unbind(go);
		GameObject.Destroy(go);
	}

	public void moveItemInParent(GameObject go, int step)
	{
		if (go.transform.GetSiblingIndex() + step < 0 || go.transform.GetSiblingIndex() + step > go.transform.parent.childCount)
			step = 0;
		go.transform.SetSiblingIndex(go.transform.GetSiblingIndex() + step);
	}

	// Used in briefing input fields (see BriefingItem prefab -> TextPanel -> Text_input) to force sync its height with its content
	public void markLayoutForRebuild(RectTransform transform)
	{
		LayoutRebuilder.MarkLayoutForRebuild(transform);
	}
}
