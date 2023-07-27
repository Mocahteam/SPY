using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class ParamCompetenceSystem : FSystem
{
	public static ParamCompetenceSystem instance;

	// Familles
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les compétences
	private Family f_compSelector = FamilyManager.getFamily(new AnyOfTags("CompetencySelector"), new AllOfComponents(typeof(TMP_Dropdown)));
	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private UnityAction localCallback;
	private DataLevelBehaviour overridedBriefing;
	private int previousReferentialSelected = -1;

	// Variables
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
	public Button downloadLevel;
	public Button addToScenario;
	public GameObject savingPanel;
	public GameObject editBriefingPanel;
	public GameObject briefingItemPrefab;
	public TMP_InputField scenarioAbstract;
	public TMP_InputField scenarioName;

	[DllImport("__Internal")]
	private static extern void Save(string content); // call javascript

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[Serializable]
	public class RawComp
	{
		public string key;
		public string parentKey;
		public string name;
		public string description;
		public EditingUtility.RawConstraint[] constraints;
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
		// load competencies (required for level analysis)
		string referentialsPath = new Uri(Application.streamingAssetsPath + "/Competencies/competenciesReferential.json").AbsoluteUri;
		MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath));
		if (Application.platform != RuntimePlatform.WebGLPlayer)
			GameObjectManager.setGameObjectState(downloadLevel.gameObject, false);
	}

	private IEnumerator GetCompetenciesWebRequest(string referentialsPath)
    {
		UnityWebRequest www = UnityWebRequest.Get(referentialsPath);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			localCallback = null;
			localCallback += delegate { MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath)); };
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[6], referentialsPath, www.error), OkButton = gameData.localization[5], CancelButton = gameData.localization[2], call = localCallback });
		}
		else
		{
			createReferentials(www.downloadHandler.text);
		}
	}

	private void createReferentials(string jsonContent)
	{
		foreach (GameObject selector in f_compSelector)
			selector.GetComponent<TMP_Dropdown>().ClearOptions();

		try
		{
			rawReferentials = JsonUtility.FromJson<RawListReferential>(jsonContent);
		} catch (Exception e)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[7], e.Message), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			return;
		}

		// add referential to dropdone
		List<TMP_Dropdown.OptionData> referentials = new List<TMP_Dropdown.OptionData>();
		foreach (RawListComp rlc in rawReferentials.referentials)
			referentials.Add(new TMP_Dropdown.OptionData(rlc.name));
		if (referentials.Count > 0)
		{
			foreach (GameObject selector in f_compSelector)
			{
				selector.GetComponent<TMP_Dropdown>().AddOptions(referentials);
				// Be sure dropdone is active
				GameObjectManager.setGameObjectState(selector, true);
			}
		}
		else
		{
			foreach (GameObject selector in f_compSelector)
				// Hide dropdown
				GameObjectManager.setGameObjectState(selector, false);
		}

		refreshCompetencies();
	}

	// see CompetenciesFilter in TitleScreen
	public void refreshCompetencies()
    {
		createCompetencies(f_compSelector.First().GetComponent<TMP_Dropdown>().value);
	}

	// used in DropdownReferential dropdown 
	public void selectCompetencies(int referentialId)
	{
		if (previousReferentialSelected != referentialId)
		{
			previousReferentialSelected = referentialId;
			createCompetencies(referentialId);
		}
	}

	private void createCompetencies(int referentialId)
	{
		// set all competency selectors with the same value
		foreach (GameObject selector in f_compSelector)
			selector.GetComponent<TMP_Dropdown>().value = referentialId;

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
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[8], referentialId), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			return;
		}

		// create all competencies
		foreach (RawComp rawComp in rawReferentials.referentials[referentialId].list)
		{
			// On instancie la compétence
			GameObject competency = UnityEngine.Object.Instantiate(prefabComp);
			competency.SetActive(false);
			competency.name = rawComp.key;
			Competency comp = competency.GetComponent<Competency>();
			comp.parentKey = rawComp.parentKey;
			comp.id = rawComp.name;
			comp.description = EditingUtility.extractLocale(rawComp.description);
			comp.constraints = rawComp.constraints;
			if (comp.constraints.Length == 0)
				// disable checkbox
				competency.GetComponentInChildren<Toggle>().interactable = false;
			comp.rule = rawComp.rule;

			// On l'attache au content
			competency.transform.SetParent(ContentCompMenu.transform, false);
			// On charge le text de la compétence
			competency.GetComponentInChildren<TMP_Text>().text = EditingUtility.extractLocale(comp.id);
			GameObjectManager.bind(competency);
		}

		MainLoop.instance.StartCoroutine(buildCompetenciesHierarchy());
	}

	private IEnumerator buildCompetenciesHierarchy()
    {
		// move sub-competencies
		Competency[] competencies = ContentCompMenu.transform.GetComponentsInChildren<Competency>(true);
		foreach (Competency comp in competencies)
		{
			// Check if this competency has a parent
			if (comp.parentKey != "")
			{
				// Look for this parent
				foreach (Competency parentComp in competencies)
					if (parentComp.gameObject.name == comp.parentKey)
					{
						// move this competency to its parent
						comp.transform.SetParent(parentComp.transform.Find("SubCompetencies"), false);
						// enable Hide button
						parentComp.transform.Find("Header").Find("ButtonHide").gameObject.SetActive(true);
						break;
					}
			}
			comp.gameObject.SetActive(true);
			yield return null;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(ContentCompMenu.transform as RectTransform);
	}

	// Used in ButtonShowLevels in ParamCompPanel
	public void showCompatibleLevels(bool filter)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser())
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[9], OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
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
						if (!EditingUtility.isCompetencyMatchWithLevel(competency, selectedLevels[l].OwnerDocument))
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
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[10], OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
		else
		{
			// hide competencies panel
			GameObjectManager.setGameObjectState(competenciesPanel, false);
			GameObjectManager.setGameObjectState(compatibleLevelsPanel, true);
			// remove all old buttons
			foreach (Transform child in contentListOfCompatibleLevel.transform)
			{
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
			}
			testLevel.interactable = false;
			downloadLevel.interactable = false;
			addToScenario.interactable = false;
			// Instanciate one button for each level
			List<GameObject> sortedLevels = new List<GameObject>();
			foreach (XmlNode level in selectedLevels)
			{
				foreach (string levelName in gameData.levels.Keys)
					if (gameData.levels[levelName] == level)
					{
						string currentName = levelName.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, "");
						GameObject compatibleLevel = GameObject.Instantiate(levelCompatiblePrefab);
						compatibleLevel.GetComponentInChildren<TMP_Text>().text = currentName;
						int i = 0;
						while (i < sortedLevels.Count)
						{
							if (String.Compare(currentName, sortedLevels[i].GetComponentInChildren<TMP_Text>().text) == -1)
							{
								sortedLevels.Insert(i, compatibleLevel);
								break;
							}
							i++;
						}
						if (i == sortedLevels.Count)
							sortedLevels.Add(compatibleLevel);
						break;
					}
			}
			// Add buttons to UI
			foreach (GameObject level in sortedLevels)
            {
				level.transform.SetParent(contentListOfCompatibleLevel.transform, false);
				GameObjectManager.bind(level);
			}
		}
	}

	public void showLevelInfo(string path, DataLevelBehaviour overridedData = null)
	{
		string absolutePath = new Uri(Application.streamingAssetsPath + "/" + path).AbsoluteUri;

		if (gameData.levels.ContainsKey(absolutePath))
		{
			// Display Title
			contentInfoCompatibleLevel.transform.Find("levelTitle").GetComponent<TMP_Text>().text = path;
			// erase previous miniView
			setMiniView(null);
			// Display miniView
			string imgPath = new Uri(Application.streamingAssetsPath + "/" + path.Replace(".xml", ".png")).AbsoluteUri;
			MainLoop.instance.StartCoroutine(GetMiniViewWebRequest(imgPath));
			XmlNode levelSelected = gameData.levels[absolutePath];
			List<Dialog> defaultDialogs = new List<Dialog>();
			XmlNodeList XMLDialogs = levelSelected.OwnerDocument.GetElementsByTagName("dialogs");
			if (XMLDialogs.Count > 0)
				EditingUtility.readXMLDialogs(XMLDialogs[0], defaultDialogs);

			TMP_Text contentInfo = contentInfoCompatibleLevel.transform.Find("levelTextInfo").GetComponent<TMP_Text>();
			contentInfo.text = "";
			if (levelSelected != null)
			{
				DataLevelBehaviour dlb = contentInfoCompatibleLevel.GetComponent<DataLevelBehaviour>();
				if (overridedData == null)
				{
					dlb.data.src = absolutePath;
					dlb.data.name = Path.GetFileNameWithoutExtension(path);
					dlb.data.overridedDialogs = null; // to load default
				}
				else
					dlb.data = overridedData.data.clone();
				// if no overrided dialogs, load default
				if (dlb.data.overridedDialogs == null)
					dlb.data.overridedDialogs = defaultDialogs;

				// Display introTexts
				contentInfo.text = "<b>"+gameData.localization[30]+" " + (dlb.data.dialogsEqualsTo(defaultDialogs) ? "(" + gameData.localization[31] + ")" : "(" + gameData.localization[32] + ")") + " :</b>\n";
				string txt = "";
				for (int i = 0; i < dlb.data.overridedDialogs.Count; i++)
				{
					Dialog item = dlb.data.overridedDialogs[i];
					txt += "\n---"+ gameData.localization[37] + (i + 1) + "---\n";
					if (item.text != null)
						txt += EditingUtility.extractLocale(item.text) + "\n";
					if (EditingUtility.extractLocale(item.img) != "" || EditingUtility.extractLocale(item.sound) != "" || EditingUtility.extractLocale(item.video) != "")
						txt += "\n" + (EditingUtility.extractLocale(item.img) != "" ? "<<"+ gameData.localization[38] + ">>" : "") + (EditingUtility.extractLocale(item.sound) != "" ? "<<" + gameData.localization[39] + ">>" : "") + (EditingUtility.extractLocale(item.video) != "" ? "<<" + gameData.localization[40] + ">>" : "") + "\n";
				}
				if (txt != "")
					contentInfo.text += txt;
				else
					contentInfo.text += "\t" + gameData.localization[33] + "\n";
				// Display competencies
				contentInfo.text += "\n<b>" + gameData.localization[34] + "</b>\n";
				txt = "";
				foreach (GameObject comp in f_competencies)
				{
					if (EditingUtility.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), levelSelected.OwnerDocument))
						txt += "\t" + comp.GetComponent<Competency>().GetComponentInChildren<TMP_Text>().text + "\n";
				}
				if (txt != "")
					contentInfo.text += txt;
				else
					contentInfo.text += "\t" + gameData.localization[35] + "\n";
				LayoutRebuilder.ForceRebuildLayoutImmediate(contentInfo.transform as RectTransform);
			}
			else
				contentInfo.text += gameData.localization[36];
			testLevel.interactable = true;
			downloadLevel.interactable = true;
			addToScenario.interactable = true;
		}
        else
        {
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[11], path), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
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
		newLevel.GetComponent<DataLevelBehaviour>().data = contentInfoCompatibleLevel.GetComponent<DataLevelBehaviour>().data.clone();
		GameObjectManager.bind(newLevel);
	}

	// Used when PointerOver CategorizeCompetence prefab (see in editor)
	public void infoCompetence(Competency comp)
	{
		if (comp != null)
			panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.description;
	}

	public void removeItemFromParent(GameObject go)
	{
		if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(go.transform)) {
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
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[12], invalidChars), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
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
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[13], scenarioName.text), OkButton = gameData.localization[3], CancelButton = gameData.localization[4], call = localCallback });
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
		scenarioExport += "<scenario name=\""+ scenarioName.text + "\" desc=\""+ scenarioAbstract.text + "\">\n";
		foreach (Transform child in contentScenario.transform)
		{
			DataLevel dataLevel = child.GetComponent<DataLevelBehaviour>().data;
			if (dataLevel.name == null || dataLevel.name == "")
				dataLevel.name = Path.GetFileNameWithoutExtension(dataLevel.src);
			scenarioExport += "\t<level src = \"" + child.GetComponentInChildren<TMP_Text>().text + "\" name = \"" + dataLevel.name + "\"";
			if (dataLevel.overridedDialogs != null && dataLevel.overridedDialogs.Count > 0)
			{
				scenarioExport += " >\n\t\t<dialogs>\n";
				foreach (Dialog dialog in dataLevel.overridedDialogs)
				{
					scenarioExport += "\t\t\t<dialog ";
					scenarioExport += dialog.text != null && dialog.text != "" ? "text =\"" + dialog.text + "\" " : "";
					scenarioExport += dialog.img != null && dialog.img != "" ? "img=\"" + dialog.img + "\" " : "";
					scenarioExport += dialog.imgHeight != -1 ? "imgHeight=\"" + dialog.imgHeight + "\" " : "";
					scenarioExport += dialog.camX != -1 ? "camX=\"" + dialog.camX + "\" " : "";
					scenarioExport += dialog.camY != -1 ? "camY=\"" + dialog.camY + "\" " : "";
					scenarioExport += dialog.sound != null && dialog.sound != "" ? "sound=\"" + dialog.sound + "\" " : "";
					scenarioExport += dialog.video != null && dialog.video != "" ? "video=\"" + dialog.video + "\" " : "";
					scenarioExport += "enableInteraction =\"" + (dialog.enableInteraction ? "1" : "0") + "\" />\n";
				}
				scenarioExport += "\t\t</dialogs>\n\t</level>\n";
			}
			else
				scenarioExport += " />\n";
		}
		scenarioExport += "</scenario>";
		return scenarioExport;
	}

	private void saveToFile(TMP_InputField scenarioName) {
		string scenarioExport = buildScenarioContent();

		try
		{
			// Create all necessary directories if they don't exist
			Directory.CreateDirectory(Application.persistentDataPath + "/Scenario");
			string path = Application.persistentDataPath + "/Scenario/" + scenarioName.text;
			// Add/Replace scenario content in memory
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(scenarioExport);
			TitleScreenSystem.instance.updateScenarioContent(path, doc);
			// Write on disk
			File.WriteAllText(path, scenarioExport);

			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[14], Application.persistentDataPath, scenarioName.text), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
		catch (Exception e)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = EditingUtility.getFormatedText(gameData.localization[15], e.Message), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
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
		{
			GameObjectManager.setGameObjectState(savingPanel, true);
			EventSystem.current.SetSelectedGameObject(savingPanel.transform.Find("Panel").Find("Buttons").Find("CancelButton").gameObject);
		}
	}

	public void showBriefingOverride(DataLevelBehaviour dataLevel)
    {
		if (dataLevel == null)
			Debug.LogError("Missing DataLevelBehaviour component");
		else
		{
			overridedBriefing = dataLevel;
			GameObjectManager.setGameObjectState(compatibleLevelsPanel, false);
			GameObjectManager.setGameObjectState(editBriefingPanel, true);
			EventSystem.current.SetSelectedGameObject(editBriefingPanel.transform.Find("CloseButton").gameObject);
			// remove all old briefing items
			Transform viewportContent = editBriefingPanel.transform.Find("Scroll View").GetChild(0).GetChild(0);
			while (viewportContent.childCount > 3) {
				Transform child = viewportContent.GetChild(viewportContent.childCount - 1);
				GameObjectManager.unbind(child.gameObject);
				child.SetParent(null);
				GameObject.Destroy(child.gameObject);
			}

			foreach (TMP_InputField input in editBriefingPanel.GetComponentsInChildren<TMP_InputField>(true))
			{
				if (input.gameObject.name == "MissionPathContent")
					input.text = dataLevel.data.src;
				if (input.gameObject.name == "NameContent")
					input.text = dataLevel.data.name;
			}

			if (dataLevel.data.overridedDialogs == null)
            {
				XmlNode levelSelected = gameData.levels[dataLevel.data.src];
				dataLevel.data.overridedDialogs = new List<Dialog>();
				XmlNodeList XMLDialogs = levelSelected.OwnerDocument.GetElementsByTagName("dialogs");
				if (XMLDialogs.Count > 0)
					EditingUtility.readXMLDialogs(XMLDialogs[0], dataLevel.data.overridedDialogs);
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
					else
						input.text = "";
				}
				newItem.GetComponentInChildren<Toggle>().isOn = dialog.enableInteraction;
			}
		}
    }

	public void saveBriefingOverride()
    {
		if (overridedBriefing != null)
		{
			string path = "";
			foreach (TMP_InputField input in editBriefingPanel.GetComponentsInChildren<TMP_InputField>(true))
			{
				if (input.gameObject.name == "MissionPathContent")
					path = input.text;
				if (input.gameObject.name == "NameContent")
					overridedBriefing.data.name = input.text;
			}

			// save briefing items
			overridedBriefing.data.overridedDialogs = new List<Dialog>();
			Transform viewportContent = editBriefingPanel.transform.Find("Scroll View").GetChild(0).GetChild(0);
			for (int i = 3; i < viewportContent.childCount; i++)
			{
				Transform child = viewportContent.GetChild(i);
				Dialog dialog = new Dialog();
				foreach (TMP_InputField input in child.GetComponentsInChildren<TMP_InputField>(true))
				{
					if (input.name == "Text_input" && input.text != "")
						dialog.text = input.text;
					else if (input.name == "ImgPath_input" && input.text != "")
						dialog.img = input.text;
					else if (input.name == "ImgSize_input" && input.text != "")
						dialog.imgHeight = float.Parse(input.text);
					else if (input.name == "CamX_input" && input.text != "")
						dialog.camX = int.Parse(input.text);
					else if (input.name == "CamY_input" && input.text != "")
						dialog.camY = int.Parse(input.text);
					else if (input.name == "SoundPath_input" && input.text != "")
						dialog.sound = input.text;
					else if (input.name == "VideoPath_input" && input.text != "")
						dialog.video = input.text;
				}
				dialog.enableInteraction = child.GetComponentInChildren<Toggle>().isOn;
				overridedBriefing.data.overridedDialogs.Add(dialog);
			}
			showLevelInfo(path.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, ""), overridedBriefing);
		}
    }

	public void addNewBriefing(GameObject parent)
    {
		GameObject newItem = GameObject.Instantiate(briefingItemPrefab, parent.transform, false);
		GameObjectManager.bind(newItem);
	}
}