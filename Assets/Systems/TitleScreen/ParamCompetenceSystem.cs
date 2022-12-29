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
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine.Networking;

public class ParamCompetenceSystem : FSystem
{
	public static ParamCompetenceSystem instance;

	// Familles
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les Toogles compétences
	private UnityAction localCallback;

	// Variables
	public TMP_Dropdown referentialSelector; // Liste déroulante listant les référentiels
	public GameObject panelInfoComp; // Panneau d'information des compétences
	public GameObject prefabComp; // Prefab de l'affichage d'une compétence
	public GameObject ContentCompMenu; // Panneau qui contient la liste des catégories et compétences
	public GameObject compatibleLevelsPanel; // Le panneau permettant de gérer les niveaux compatibles
	public GameObject competenciesPanel; // Le panneau permettant de gérer la sélection des compétences
	public GameObject levelCompatiblePrefab; // Le préfab d'un bouton d'un niveau compatible
	public GameObject contentListOfCompatibleLevel; // Le panneau contenant l'ensemble des niveaux compatibles
	public GameObject contentInfoCompatibleLevel; // Le panneau contenant les info d'un niveau (miniView, dialogues, competences, ...)
	public GameObject deletableElement; // Un niveau que l'on peut supprimer
	public GameObject contentScenario; // Le panneau contenant les niveaux sélectionnés pour le scénario
	public Button testLevel;
	public Button addToScenario;
	public GameObject savingPanel;

	[DllImport("__Internal")]
	private static extern void Save(string content); // call javascript

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
		public string name;
		public List<RawComp> list = new List<RawComp>();
	}

	[Serializable]
	public class RawListReferential
	{
		public List<RawListComp> referentials = new List<RawListComp>();
	}

	private GameData gameData;
	private RawListReferential rawReferentials;

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
	public void loadPanelSelectComp()
	{
		string referentialsPath = Application.streamingAssetsPath + "/Competencies/competenciesReferential.json";
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath));
		else
		{
            try
            {
				createReferentials(File.ReadAllText(referentialsPath));
			}
            catch (Exception e)
			{
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors de l'accès au document " + referentialsPath + " : " + e.Message, OkButton = "", CancelButton = "OK", call = localCallback });
			}
		}
	}

	private IEnumerator GetCompetenciesWebRequest(string referentialsPath)
    {
		UnityWebRequest www = UnityWebRequest.Get(referentialsPath);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors de l'accès au document " + referentialsPath + " : " + www.error, OkButton = "", CancelButton = "OK", call = localCallback });
		}
		else
		{
			createReferentials(www.downloadHandler.text);
		}
	}

	private void createReferentials(string jsonContent)
	{
		referentialSelector.ClearOptions();

		try
		{
			rawReferentials = JsonUtility.FromJson<RawListReferential>(jsonContent);
		} catch (Exception e)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "La liste des référentiels est mal formée : " + e.Message, OkButton = "", CancelButton = "OK", call = localCallback });
			return;
		}

		// add referential to dropdone
		List<TMP_Dropdown.OptionData> referentials = new List<TMP_Dropdown.OptionData>();
		foreach (RawListComp rlc in rawReferentials.referentials)
			referentials.Add(new TMP_Dropdown.OptionData(rlc.name));
		if (referentials.Count > 0)
		{
			referentialSelector.AddOptions(referentials);
			// Be sure dropdone is active
			GameObjectManager.setGameObjectState(referentialSelector.gameObject, true);
		}
		else
		{
			// Hide dropdown
			GameObjectManager.setGameObjectState(referentialSelector.gameObject, false);
		}

		createCompetencies(referentialSelector.value);
	}

	// used in DropdownReferential dropdown 
	public void createCompetencies(int referentialId)
	{
		// remove all old competencies
		for (int i = ContentCompMenu.transform.childCount - 1; i >= 0; i--)
        {
			Transform child = ContentCompMenu.transform.GetChild(i);
			GameObjectManager.unbind(child.gameObject);
			child.SetParent(null);
			GameObject.Destroy(child.gameObject);
        }

		if (referentialId >= rawReferentials.referentials.Count || referentialId < 0)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Référentiel numéro "+referentialId+" non défini.", OkButton = "", CancelButton = "OK", call = localCallback });
			return;
		}

		// create all competencies
		foreach (RawComp rawComp in rawReferentials.referentials[referentialId].list)
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
			competency.transform.SetParent(ContentCompMenu.transform, false);
			// On charge le text de la compétence
			competency.GetComponentInChildren<TMP_Text>().text = rawComp.name;
			GameObjectManager.bind(competency);
		}

		// move sub-competencies
		Competency[] competencies = ContentCompMenu.transform.GetComponentsInChildren<Competency>();
		foreach (Competency comp in competencies)
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
		LayoutRebuilder.ForceRebuildLayoutImmediate(ContentCompMenu.transform as RectTransform);
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
	public void showCompatibleLevels(bool filter)
	{
		// default, select all levels
		List<XmlNode> selectedLevels = new List<XmlNode>();
		foreach (XmlNode level in gameData.levels.Values)
			selectedLevels.Add(level);
		if (filter)
		{
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
		}
		
		if (selectedLevels.Count == 0)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Aucun niveau compatible avec votre sélection", OkButton = "", CancelButton = "OK", call = localCallback });
		}
		else
		{
			// hide competencies panel
			GameObjectManager.setGameObjectState(competenciesPanel, false);
			// remove all old buttons
			foreach (Transform child in contentListOfCompatibleLevel.transform)
			{
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
			}
			testLevel.interactable = false;
			addToScenario.interactable = false;
			// Instanciate one button for each level
			foreach (XmlNode level in selectedLevels)
			{
				GameObject compatibleLevel = GameObject.Instantiate(levelCompatiblePrefab, contentListOfCompatibleLevel.transform, false);
				foreach (string levelName in gameData.levels.Keys)
					if (gameData.levels[levelName] == level)
					{
						compatibleLevel.GetComponentInChildren<TMP_Text>().text = levelName.Replace(Application.streamingAssetsPath + "/", "");
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
		string imgPath = Application.streamingAssetsPath + "/" + path.Replace(".xml", ".png");
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			MainLoop.instance.StartCoroutine(GetMiniViewWebRequest(imgPath));
		}
		else
		{
			try 
			{
				Texture2D tex2D = new Texture2D(2, 2); //create new "empty" texture
				byte[] fileData = File.ReadAllBytes(imgPath); //load image from SPY/path
				if (tex2D.LoadImage(fileData))
					setMiniView(tex2D);
			}
			catch
			{
				setMiniView(null);
			}
		}

		XmlNode levelSelected = gameData.levels[Application.streamingAssetsPath + "/" + path];

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
		testLevel.interactable = true;
		addToScenario.interactable = true;
	}

	private IEnumerator GetMiniViewWebRequest(string miniViewUri)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(miniViewUri);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			setMiniView(null);
		}
		else
		{
			Texture2D tex2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
			setMiniView(tex2D);
		}
	}

	private void setMiniView(Texture2D tex2D)
    {
		if (tex2D != null)
		{
			GameObjectManager.setGameObjectState(contentInfoCompatibleLevel.transform.Find("LevelMiniView").gameObject, true);
			Image img = contentInfoCompatibleLevel.transform.Find("LevelMiniView").GetComponent<Image>();
			img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
			img.preserveAspect = true;
		}
		else
			GameObjectManager.setGameObjectState(contentInfoCompatibleLevel.transform.Find("LevelMiniView").gameObject, false);
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

	public void refreshUI(RectTransform competency)
	{
		MainLoop.instance.StartCoroutine(delayRefreshUI(competency));
	}

	private IEnumerator delayRefreshUI(RectTransform competency)
    {
		yield return null;
		Competency comp = competency.GetComponentInParent<Competency>();
		while (comp != null)
		{
			competency = comp.transform as RectTransform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(competency);
			comp = competency.parent.GetComponentInParent<Competency>();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(competency.transform.parent as RectTransform);
	}

	/// <summary>
	/// Called when trying to save
	/// </summary>
	private bool CheckSaveNameValidity(string nameCandidate)
	{
		bool isValid = true;

		isValid = nameCandidate != "";

		char[] chars = Path.GetInvalidFileNameChars();

		foreach (char c in chars)
			if (nameCandidate.IndexOf(c) != -1)
			{
				isValid = false;
				break;
			}

		return isValid;
	}

	public void saveScenario(TMP_InputField scenarioName)
	{
		if (!CheckSaveNameValidity(scenarioName.text))
		{
			localCallback = null;
			string invalidChars = "";
			foreach (char someChar in Path.GetInvalidFileNameChars())
				if (Char.IsPunctuation(someChar) || Char.IsSymbol(someChar))
					invalidChars += someChar+" ";
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Le nom du scenario est invalide. Il ne peut être vide et contenir les caratères suivants : "+ invalidChars, OkButton = "", CancelButton = "OK", call = localCallback });
			// Be sure saving windows is enabled
			GameObjectManager.setGameObjectState(scenarioName.transform.parent.parent.gameObject, true);
		}
		else
		{
			// remove file extension
			if (!scenarioName.text.EndsWith(".xml"))
				scenarioName.text += ".xml";
			if (File.Exists(Application.persistentDataPath + "/Scenario/" + scenarioName.text))
			{
				localCallback = null;
				localCallback += delegate { saveToFile(scenarioName); };
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Un scénario avec le nom \"" + scenarioName.text + "\" existe déjà, souhaitez-vous le remplacer ?", OkButton = "Oui", CancelButton = "Non", call = localCallback });
				// Be sure saving windows is enabled
				GameObjectManager.setGameObjectState(scenarioName.transform.parent.parent.gameObject, true);
			}
			else
				saveToFile(scenarioName);
		}
	}

	private string buildScenarioContent()
    {
		string scenarioExport = "<?xml version=\"1.0\"?>\n";
		scenarioExport += "<scenario>\n";
		foreach (Transform child in contentScenario.transform)
			scenarioExport += "\t<level name = \"" + child.GetComponentInChildren<TMP_Text>().text + "\" />\n";
		scenarioExport += "</scenario>";
		return scenarioExport;
	}

	private void saveToFile(TMP_InputField scenarioName) {
		string scenarioExport = buildScenarioContent();

		try
		{
			// Create all necessary directories if they don't exist
			Directory.CreateDirectory(Application.persistentDataPath + "/Scenario");
			File.WriteAllText(Application.persistentDataPath + "/Scenario/" + scenarioName.text, scenarioExport);
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Le scénario a été enregistré dans le fichier : " + Application.persistentDataPath + "/Scenario/" + scenarioName.text, OkButton = "", CancelButton = "OK", call = localCallback });
		}
		catch (Exception e)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur, le scénario n'a pas été enregistré.\n" + e, OkButton = "", CancelButton = "OK", call = localCallback });
		}
		// Be sure saving windows is disabled
		GameObjectManager.setGameObjectState(scenarioName.transform.parent.parent.gameObject, false);
	}

	// See ButtonSaveScenario
	public void displaySavingPanel()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			Save(buildScenarioContent());
		else
			GameObjectManager.setGameObjectState(savingPanel, true);
	}
}