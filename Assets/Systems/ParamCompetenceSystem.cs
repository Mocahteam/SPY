using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System;
using System.Data;
using UnityEngine.Events;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Familles
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les Toogles compétences

	// Variables
	public GameObject panelInfoComp; // Panneau d'information des compétences
	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, conflit dans la selection des compétences etc...)
	public GameObject prefabComp; // Prefab de l'affichage d'une compétence
	public GameObject ContentCompMenu; // Panneau qui contient la liste des catégories et compétences
	public TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressés à l'utilisateur
	public GameObject compatibleLevelsPanel; // Le panneau permettant de gérer les niveaux compatibles
	public GameObject levelCompatiblePrefab; // Le préfab d'un bouton d'un niveau compatible
	public GameObject contentListOfCompatibleLevel; // Le panneau contenant l'ensemble des niveaux compatibles
	public GameObject contentInfoCompatibleLevel; // Le panneau contenant les info d'un niveau (miniView, dialogues, competences, ...)
	public GameObject deletableElement; // Un niveau que l'on peut supprimer
	public GameObject contentScenario; // Le panneau contenant les niveaux sélectionnés pour le scénario

	[Serializable]
	public class RawParams
	{
		public string attribute;
		public string constraint;
		public string value;
		public string tag2;
		public string attribute2;
	}

	[Serializable]
	public class RawConstraint
	{
		public string label;
		public string tag;
		public RawParams[] parameters;
	}

	[Serializable]
	public class RawComp
    {
		public string key;
		public string parentKey;
		public string name;
		public string description;
		public RawConstraint[] constraints;
		public string rule;
    }

	[Serializable]
	public class RawListComp
    {
		public List<RawComp> list = new List<RawComp>();
	}

	private RawListComp competencies;
	private GameData gameData;

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
	}

	// used on TitleScreen scene
	public void openPanelSelectComp()
	{
		RawListComp raw_competencies = JsonUtility.FromJson<RawListComp>(File.ReadAllText(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Competencies" + Path.DirectorySeparatorChar + "PIAF.json"));
		// create all competencies
		foreach(RawComp rawComp in raw_competencies.list)
        {
			// On instancie la compétence
			GameObject competency = UnityEngine.Object.Instantiate(prefabComp);
			competency.name = rawComp.key;
			Competency comp = competency.GetComponent<Competency>();
			comp.parentKey = rawComp.parentKey;
			comp.description = rawComp.description;
			comp.constraints = rawComp.constraints;
			if (comp.constraints.Length == 0)
				// disable checkbox
				competency.GetComponentInChildren<Toggle>().interactable = false;
			comp.rule = rawComp.rule;

			// On l'attache au content
			competency.transform.SetParent(ContentCompMenu.transform);
			// On charge le text de la compétence
			competency.GetComponentInChildren<TMP_Text>().text = rawComp.name;
			GameObjectManager.bind(competency);
		}

		// move sub-competencies
		Competency[] competencies = ContentCompMenu.transform.GetComponentsInChildren<Competency>();
		foreach(Competency comp in competencies)
        {
			// Check if this competency has a parent
			if (comp.parentKey != "")
				// Look for this parent
				foreach (Competency parentComp in competencies)
					if (parentComp.gameObject.name == comp.parentKey)
					{
						// move this competency to its parent
						comp.transform.SetParent(parentComp.transform.Find("SubCompetencies"), false);
						// enable Hide button
						parentComp.transform.Find("Header").Find("ButtonHide").gameObject.SetActive(true);
						LayoutRebuilder.ForceRebuildLayoutImmediate(parentComp.transform.Find("SubCompetencies") as RectTransform);
					}
        }
	}

	public void cleanCompPanel()
    {
		for (int i = ContentCompMenu.transform.childCount - 1; i >= 0; i--)
		{
			Transform child = ContentCompMenu.transform.GetChild(i);
			GameObjectManager.unbind(child.gameObject);
			UnityEngine.Object.Destroy(child.gameObject);
		}

	}

	// Used in ButtonShowLevels in ParamCompPanel
	public void showCompatibleLevels()
    {
		// default, select all levels
		List<XmlNode> selectedLevels = new List<XmlNode>();
		foreach (XmlNode level in gameData.levels.Values)
			selectedLevels.Add(level);
		// now, filtering level that not check constraints
		int nbCompSelected = 0;
		foreach (GameObject comp in f_competencies)
        {
			// process only selected competencies
			if (comp.GetComponentInChildren<Toggle>().isOn)
            {
				nbCompSelected++;
				Competency competency = comp.GetComponent<Competency>();
				// parse all levels
				for (int l = selectedLevels.Count - 1; l >= 0; l--)
				{
					if (!isCompetencyMatchWithLevel(competency, selectedLevels[l].OwnerDocument))
						selectedLevels.RemoveAt(l);
				}
            }
		}
		if (nbCompSelected == 0)
			selectedLevels.Clear();
		Debug.Log(selectedLevels.Count);
		if (selectedLevels.Count == 0)
			displayMessageUser("Aucun niveau compatible avec votre sélection", "", "OK", delegate { } );
		else
        {
			// display compatible panel
			GameObjectManager.setGameObjectState(compatibleLevelsPanel, true);
			// remove all old buttons
			foreach(Transform child in contentListOfCompatibleLevel.transform)
            {
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
            }
			compatibleLevelsPanel.transform.Find("ButtonTestLevel").GetComponent<Button>().interactable = false;
			compatibleLevelsPanel.transform.Find("ButtonAddToScenario").GetComponent<Button>().interactable = false;
			// Instanciate one button for each level
			foreach (XmlNode level in selectedLevels)
			{
				GameObject compatibleLevel = GameObject.Instantiate(levelCompatiblePrefab, contentListOfCompatibleLevel.transform);
				foreach (string levelName in gameData.levels.Keys)
					if (gameData.levels[levelName] == level)
					{
						compatibleLevel.GetComponentInChildren<TMP_Text>().text = levelName.Replace(Application.streamingAssetsPath+Path.DirectorySeparatorChar, "");
						break;
					}
				GameObjectManager.bind(compatibleLevel);
			}
        }
	}

	private bool isCompetencyMatchWithLevel(Competency competency, XmlDocument level)
    {

		// check all constraints of the competency
		Dictionary<string, List<XmlNode>> constraintsState = new Dictionary<string, List<XmlNode>>();
		foreach (RawConstraint constraint in competency.constraints)
		{

			if (constraintsState.ContainsKey(constraint.label))
			{
				// if a constraint with this label is defined and no XmlNode identified, useless to check this new one
				if (constraintsState[constraint.label].Count == 0)
					continue;
			}
			else
			{
				// init this constraint with all XmlNode of required tag
				List<XmlNode> tagList = new List<XmlNode>();
				foreach (XmlNode tag in level.GetElementsByTagName(constraint.tag))
					tagList.Add(tag);
				constraintsState.Add(constraint.label, tagList);
			}

			// check if this constraint is true
			List<XmlNode> tags = constraintsState[constraint.label];
			foreach (RawParams parameter in constraint.parameters)
			{
				int levelAttrValue;
				switch (parameter.constraint)
				{
					// Check if the value of an attribute of the tag is equal to a given value
					case "=":
						for (int t = tags.Count - 1; t >= 0; t--)
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null || tags[t].Attributes.GetNamedItem(parameter.attribute).Value != parameter.value)
								tags.RemoveAt(t);
						break;
					// Check if the value of an attribute of the tag is not equal to a given value
					case "<>":
						for (int t = tags.Count - 1; t >= 0; t--)
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null || tags[t].Attributes.GetNamedItem(parameter.attribute).Value == parameter.value)
								tags.RemoveAt(t);
						break;
					// Check if the value of an attribute of the tag is greater than a given value (for limit attribute consider -1 as infinite value)
					case ">":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(parameter.attribute).Value);
									if (levelAttrValue <= int.Parse(parameter.value) && (parameter.attribute != "limit" || levelAttrValue != -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is smaller than a given value (for limit attribute consider -1 as infinite value)
					case "<":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(parameter.attribute).Value);
									if (levelAttrValue >= int.Parse(parameter.value) || (parameter.attribute == "limit" && levelAttrValue == -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is greater than or equal a given value (for limit attribute consider -1 as infinite value)
					case ">=":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(parameter.attribute).Value);
									if (levelAttrValue < int.Parse(parameter.value) && (parameter.attribute != "limit" || levelAttrValue != -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is smaller than or equal a given value (for limit attribute consider -1 as infinite value)
					case "<=":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(parameter.attribute).Value);
									if (levelAttrValue > int.Parse(parameter.value) || (parameter.attribute == "limit" && levelAttrValue == -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the attribute of the tag is included inside a given value
					case "include":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null || !parameter.value.Contains(tags[t].Attributes.GetNamedItem(parameter.attribute).Value))
								tags.RemoveAt(t);
						}
						break;
					// Check if the value of an attribute of a tag is equal to the value of an attribute of another tag
					case "sameValue":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(parameter.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								bool found = false;
								foreach (XmlNode node in tags[t].OwnerDocument.GetElementsByTagName(parameter.tag2))
								{
									if (node != tags[t] && node.Attributes.GetNamedItem(parameter.attribute2) != null && node.Attributes.GetNamedItem(parameter.attribute2).Value == tags[t].Attributes.GetNamedItem(parameter.attribute).Value)
									{
										found = true;
										break;
									}
								}
								if (!found)
									tags.RemoveAt(t);
							}
						}
						break;
					// Check if a tag contains at least one child
					case "hasChild":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (!tags[t].HasChildNodes)
								tags.RemoveAt(t);
						}
						break;
				}
			}
		}
		// check the rule (combination of constraints)
		string rule = competency.rule;
		foreach (string key in constraintsState.Keys)
		{
			rule = rule.Replace(key, "" + constraintsState[key].Count);
		}
		DataTable dt = new DataTable();
		if (rule != "")
			return (bool)dt.Compute(rule, "");
		else
			return false;
	}

	public void showLevelInfo(string path)
	{
		// Display Title
		contentInfoCompatibleLevel.transform.Find("levelTitle").GetComponent<TMP_Text>().text = path;
		// Display miniView
		string imgPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + path.Replace(".xml", ".png");
		if (File.Exists(imgPath))
		{

			GameObjectManager.setGameObjectState(contentInfoCompatibleLevel.transform.Find("LevelMiniView").gameObject, true);
			Texture2D tex2D = new Texture2D(2, 2); //create new "empty" texture
			byte[] fileData = File.ReadAllBytes(imgPath); //load image from SPY/path
			if (tex2D.LoadImage(fileData))
			{
				Image img = contentInfoCompatibleLevel.transform.Find("LevelMiniView").GetComponent<Image>();
				img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
				img.preserveAspect = true;
			}
		}
		else
			GameObjectManager.setGameObjectState(contentInfoCompatibleLevel.transform.Find("LevelMiniView").gameObject, false);
		XmlNode levelSelected = gameData.levels[Application.streamingAssetsPath+Path.DirectorySeparatorChar+path];

		TMP_Text contentInfo = contentInfoCompatibleLevel.transform.Find("levelTextInfo").GetComponent<TMP_Text>();
		contentInfo.text = "";
		if (levelSelected != null)
		{
			// Display introTexts
			contentInfo.text = "<b>Textes de briefing :</b>\n";
			string txt = "";
			foreach (XmlNode dialogs in levelSelected.OwnerDocument.GetElementsByTagName("dialogs"))
				foreach (XmlNode dialog in dialogs.ChildNodes)
					if (dialog.Attributes.GetNamedItem("text") != null)
						txt += "\t" + dialog.Attributes.GetNamedItem("text").Value + "\n";
			if (txt != "")
				contentInfo.text += txt;
			else
				contentInfo.text += "\tAucun texte défini\n";
			LayoutRebuilder.ForceRebuildLayoutImmediate(contentInfo.transform as RectTransform);
			// Display competencies
			contentInfo.text += "\n<b>Compétences en jeu :</b>\n";
			txt = "";
			foreach (GameObject comp in f_competencies)
			{
				if (isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), levelSelected.OwnerDocument))
					txt += "\t" + comp.GetComponent<Competency>().GetComponentInChildren<TMP_Text>().text + "\n";
			}
			if (txt != "")
				contentInfo.text += txt;
			else
				contentInfo.text += "\tAucune compétence identifiée pour ce niveau\n";
		}
		else
			contentInfo.text += "Aucune information à afficher sur ce niveau.";
		compatibleLevelsPanel.transform.Find("ButtonTestLevel").GetComponent<Button>().interactable = true;
		compatibleLevelsPanel.transform.Find("ButtonAddToScenario").GetComponent<Button>().interactable = true;
	}

	// Used on ButtonAddToScenario
	public void addCurrentLevelToScenario()
    {
		GameObject newLevel = GameObject.Instantiate(deletableElement, contentScenario.transform);
		newLevel.GetComponentInChildren<TMP_Text>().text = contentInfoCompatibleLevel.transform.Find("levelTitle").GetComponent<TMP_Text>().text;
		LayoutRebuilder.ForceRebuildLayoutImmediate(newLevel.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentScenario.transform as RectTransform);
		GameObjectManager.bind(newLevel);
	}

	// Used when PointerOver CategorizeCompetence prefab (see in editor)
	public void infoCompetence(Competency comp)
	{
		if (comp != null)
			panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.description;
	}

	public void removeLevelFromScenario(GameObject go)
    {
		GameObjectManager.unbind(go);
		GameObject.Destroy(go);
    }

	public void moveLevelInScenario(GameObject go, int step)
    {
		if (go.transform.GetSiblingIndex() + step < 0 || go.transform.GetSiblingIndex() + step > go.transform.parent.childCount)
			step = 0;
		go.transform.SetSiblingIndex(go.transform.GetSiblingIndex() + step);
	}

	//Used on 
	public void testLevel(TMP_Text levelToLoad)
	{
		if (contentScenario.transform.childCount == 0)
			startLevel(levelToLoad.text);
		else
			displayMessageUser("Si vous n'avez pas sauvegarder votre scénario, les dernières modifications seront perdues.\nEtes-vous sûr de vouloir continuer ?", "Oui", "Annuler", delegate { startLevel(levelToLoad.text); });
	}

	private void startLevel (string levelToLoad)
	{
		gameData.scenarioName = "testLevel";
		gameData.scenario = new List<string>();
		gameData.levelToLoad = Application.streamingAssetsPath + Path.DirectorySeparatorChar + levelToLoad;
		gameData.scenario.Add(gameData.levelToLoad);
		GameObjectManager.loadScene("MainScene");
	}

	public delegate void callback(string param);

	// Affiche le panel message avec le bon message
	public void displayMessageUser(string message, string OkButton, string CancelButton, UnityAction call)
    {
		messageForUser.text = message;
		GameObject buttons = panelInfoUser.transform.Find("Panel").Find("Buttons").gameObject;
		GameObjectManager.setGameObjectState(buttons.transform.GetChild(0).gameObject, OkButton != "");
		buttons.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = OkButton;
		buttons.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
		buttons.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(call);
		GameObjectManager.setGameObjectState(buttons.transform.GetChild(1).gameObject, CancelButton != "");
		buttons.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text = CancelButton;
		panelInfoUser.SetActive(true); // not use GameObjectManager here else ForceRebuildLayout doesn't work
		LayoutRebuilder.ForceRebuildLayoutImmediate(messageForUser.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(messageForUser.transform.parent as RectTransform);
	}

	public void refreshUI(RectTransform competency)
    {
		Competency comp = competency.GetComponentInParent<Competency>();
		while(comp != null)
		{
			competency = comp.transform as RectTransform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(competency);
			comp = competency.parent.GetComponentInParent<Competency>();
        }
		LayoutRebuilder.ForceRebuildLayoutImmediate(competency.transform.parent as RectTransform);
	}
}