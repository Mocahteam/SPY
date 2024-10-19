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
	private Family f_askToRefreshCompetencies = FamilyManager.getFamily(new AllOfComponents(typeof(AskToRefreshCompetencies)));
	private Family f_askToTestLevel = FamilyManager.getFamily(new AllOfComponents(typeof(AskToTestLevel)));
	private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));
	private UnityAction localCallback;
	private DataLevelBehaviour overridedBriefing;
	private int previousReferentialSelected = -1;
	private GameObject selectedScenarioGO;
	private bool competenciesLoadedAndReady;

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
	public Button testLevelBt;
	public Button downloadLevelBt;
	public Button addToScenario;
	public GameObject savingPanel;
	public GameObject editBriefingPanel;
	public GameObject briefingItemPrefab;
	public TMP_InputField scenarioAbstract;
	public TMP_InputField scenarioName;
	public GameObject scenarioContent;
	public GameObject loadingScenarioContent;
	public GameObject mainCanvas;
	public TMP_InputField levelFilterByName;

	[DllImport("__Internal")]
	private static extern void Save(string content, string defaultName); // call javascript

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern void DownloadLevel(string uri); // call javascript

	[Serializable]
	public class RawComp
	{
		public string key;
		public string parentKey;
		public string name;
		public string description;
		public Utility.RawFilter[] filters;
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
			GameObjectManager.setGameObjectState(downloadLevelBt.gameObject, false);

		f_askToRefreshCompetencies.addEntryCallback(delegate (GameObject go) {
			foreach (AskToRefreshCompetencies refresh in go.GetComponents<AskToRefreshCompetencies>())
				GameObjectManager.removeComponent(refresh);
			refreshCompetencies();
		});

		f_askToTestLevel.addEntryCallback(delegate (GameObject go)
		{
			foreach (AskToTestLevel test in go.GetComponents<AskToTestLevel>())
				GameObjectManager.removeComponent(test);
			testLevelPath(go.GetComponent<AskToTestLevel>().url);
		});

		selectedScenarioGO = null;
	}

	private IEnumerator GetCompetenciesWebRequest(string referentialsPath)
    {
		competenciesLoadedAndReady = false;
		UnityWebRequest www = UnityWebRequest.Get(referentialsPath);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			localCallback = null;
			localCallback += delegate { MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath)); };
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[6], referentialsPath, www.error), OkButton = gameData.localization[5], CancelButton = gameData.localization[2], call = localCallback });
		}
		else
		{
			while (f_localizationLoaded.Count == 0) // waiting localization loaded
				yield return null;
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
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[7], e.Message), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
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
		// save previous competencies selected
		List<string> competenciesEnabled = new List<string>();
		foreach (GameObject comp in f_competencies)
			if (comp.GetComponentInChildren<Toggle>().isOn)
				competenciesEnabled.Add(comp.GetComponent<Competency>().id);

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
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[8], referentialId), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
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
			comp.description = Utility.extractLocale(rawComp.description);
			comp.filters = rawComp.filters;
			if (comp.filters.Length == 0)
				// disable checkbox
				competency.GetComponentInChildren<Toggle>().interactable = false;
			comp.rule = rawComp.rule;
			// restaurer la sélection
			competency.GetComponentInChildren<Toggle>().isOn = competenciesEnabled.Contains(comp.id);

			// On l'attache au content
			competency.transform.SetParent(ContentCompMenu.transform, false);
			// On charge le text de la compétence
			competency.GetComponentInChildren<TMP_Text>().text = Utility.extractLocale(comp.id);
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
		competenciesLoadedAndReady = true;
		refreshLevelInfo();
	}

	public void traceLoadindScenarioEditor()
	{
		GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
		{
			verb = "opened",
			objectType = "scenarioEditor"
		});
	}

	// Used in CreateScenario button (main menu)
	public void showCompatibleLevels()
	{
		MainLoop.instance.StartCoroutine(delayshowCompatibleLevels());

		if (Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser())
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[9], OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
	}

	private IEnumerator delayshowCompatibleLevels()
	{
		while (!competenciesLoadedAndReady)
			yield return null;

		filterCompatibleLevels(false);
	}

	// see ShowAllLevels GameObject
	public void resetFilters()
    {
		// unselect all competencies
		foreach (GameObject comp in f_competencies)
			comp.GetComponentInChildren<Toggle>().isOn = false;
		// reset name filtering
		levelFilterByName.text = "";
		filterCompatibleLevels(false);
	}

	// see ButtonShowLevels in ParamCompPanel and Filter GameObject (to filter level by name)
	public void filterCompatibleLevels(bool nameFiltering) { 
		// default, select all levels
		List<XmlNode> selectedLevels = new List<XmlNode>();
		foreach (KeyValuePair<string, XmlNode> level in gameData.levels)
			if (level.Key != Utility.testFromScenarioEditor && level.Key != Utility.testFromLevelEditor && level.Key != Utility.testFromUrl) // we don't add new line for tested levels
				selectedLevels.Add(level.Value);

		// now, identify selected competencies
		string competenciesSelected = "";
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
					if (!Utility.isCompetencyMatchWithLevel(competency, selectedLevels[l].OwnerDocument))
						selectedLevels.RemoveAt(l);
				}
				if (competenciesSelected != "")
					competenciesSelected += "; ";
				competenciesSelected += competency.name;
			}
		}

		TMP_Dropdown selectComp = f_compSelector.First().GetComponent<TMP_Dropdown>();

		if (!nameFiltering)
		{
			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "filtered",
				objectType = "competencies",
				activityExtensions = new Dictionary<string, string>() {
						{ "context", selectComp.options[selectComp.value].text },
						{ "content", competenciesSelected }
					}
			});
		}

		if (selectedLevels.Count == 0)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[10], OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
		else
		{
			// get name filter
			string namefilter = levelFilterByName.text.ToLower();
			// hide competencies panel
			GameObjectManager.setGameObjectState(competenciesPanel, false);
			GameObjectManager.setGameObjectState(compatibleLevelsPanel, true);
			// remove all old buttons
			for (int i = contentListOfCompatibleLevel.transform.childCount - 1; i >=0; i--)
			{
				Transform child = contentListOfCompatibleLevel.transform.GetChild(i);
				GameObjectManager.unbind(child.gameObject);
				child.SetParent(null);
				GameObject.Destroy(child.gameObject);
			}
			testLevelBt.interactable = false;
			downloadLevelBt.interactable = false;
			addToScenario.interactable = false;
			// Instanciate one button for each level
			List<GameObject> sortedLevels = new List<GameObject>();
			foreach (XmlNode level in selectedLevels)
			{
				foreach (string levelName in gameData.levels.Keys)
					if (gameData.levels[levelName] == level)
					{
						string currentName = Utility.extractFileName(levelName);
						if (currentName.ToLower().Contains(namefilter))
						{
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
						}
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
		if (gameData.selectedScenario == Utility.testFromScenarioEditor)
		{
			loadScenario(Utility.editingScenario);
			DataLevel dl = gameData.scenarios[gameData.selectedScenario].levels[0];
			showLevelInfo(Utility.extractFileName(dl.src), dl);
			gameData.selectedScenario = "";
		}
	}

	// See ButtonLoadScenario
	public void displayLoadingPanel(string filter)
	{
		selectedScenarioGO = null;
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("LoadingPanel").gameObject, true);
		// remove all old scenario
		foreach (Transform child in loadingScenarioContent.transform)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		//create scenario buttons and filter field in loading panel
		List<string> sortedScenarios = new List<string>();
		foreach (string key in gameData.scenarios.Keys)
		{
			if (key != Utility.testFromScenarioEditor && key != Utility.testFromLevelEditor && key != Utility.testFromUrl && key != Utility.editingScenario && key.Contains(filter)) // we don't add new line for tested levels
				sortedScenarios.Add(key);
		}
		sortedScenarios.Sort();
		foreach (string key in sortedScenarios)
		{
			GameObject scenarioItem = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/ScenarioAvailable") as GameObject, loadingScenarioContent.transform);
			scenarioItem.GetComponent<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(scenarioItem);
		}
	}

	// see LoadButton in LoadingPanel in TitleScreen scene
	public void loadScenario()
	{
		if (selectedScenarioGO != null)
			loadScenario(selectedScenarioGO.GetComponentInChildren<TMP_Text>().text);
	}

	private void loadScenario(string scenarioKey)
    {
		if (gameData.scenarios.ContainsKey(scenarioKey))
		{
			//remove all old scenario
			foreach (Transform child in scenarioContent.transform)
			{
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
			}

			scenarioName.text = gameData.scenarios[scenarioKey].name;
			scenarioAbstract.text = gameData.scenarios[scenarioKey].description;

			foreach (DataLevel levelPath in gameData.scenarios[scenarioKey].levels)
			{
				GameObject newLevel = GameObject.Instantiate(deletableElement, scenarioContent.transform);
				newLevel.GetComponentInChildren<TMP_Text>().text = Utility.extractFileName(levelPath.src);
				LayoutRebuilder.ForceRebuildLayoutImmediate(newLevel.transform as RectTransform);
				newLevel.GetComponent<DataLevelBehaviour>().data = levelPath.clone();
				GameObjectManager.bind(newLevel);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(scenarioContent.transform as RectTransform);
		}
	}

	public void onScenarioSelected(GameObject go)
	{
		selectedScenarioGO = go;
	}

	private void refreshLevelInfo()
    {
		string path = contentInfoCompatibleLevel.transform.Find("levelTitle").GetComponent<TMP_Text>().text;
		if (path != "")
		{
			DataLevel dataLevel = contentInfoCompatibleLevel.GetComponent<DataLevelBehaviour>().data;
			showLevelInfo(path, dataLevel);
		}
    }

	public void showLevelInfo(string path, DataLevelBehaviour overridedData = null)
	{
		showLevelInfo(path, overridedData == null ? null : overridedData.data);
	}

	private void showLevelInfo(string path, DataLevel overridedData = null) { 
		string absolutePath = new Uri(Application.persistentDataPath + "/" + path).AbsoluteUri;
		string mainPath = Application.persistentDataPath;
		if (!gameData.levels.ContainsKey(absolutePath))
		{
			absolutePath = new Uri(Application.streamingAssetsPath + "/" + path).AbsoluteUri;
			mainPath = Application.streamingAssetsPath;
		}

		if (gameData.levels.ContainsKey(absolutePath))
		{
			// Display Title
			contentInfoCompatibleLevel.transform.Find("levelTitle").GetComponent<TMP_Text>().text = path;
			// erase previous miniView
			setMiniView(null);
			// Display miniView
			string imgPath = new Uri(mainPath + "/" + path.Replace(".xml", ".png")).AbsoluteUri;
			MainLoop.instance.StartCoroutine(GetMiniViewWebRequest(imgPath));
			XmlNode levelSelected = gameData.levels[absolutePath];
			List<Dialog> defaultDialogs = new List<Dialog>();
			XmlNodeList XMLDialogs = levelSelected.OwnerDocument.GetElementsByTagName("dialogs");
			if (XMLDialogs.Count > 0)
				Utility.readXMLDialogs(XMLDialogs[0], defaultDialogs);

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
					dlb.data = overridedData.clone();
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
						txt += Utility.extractLocale(item.text) + "\n";
					if (Utility.extractLocale(item.img) != "" || Utility.extractLocale(item.sound) != "" || Utility.extractLocale(item.video) != "")
						txt += "\n" + (Utility.extractLocale(item.img) != "" ? "<<"+ gameData.localization[38] + ">>" : "") + (Utility.extractLocale(item.sound) != "" ? "<<" + gameData.localization[39] + ">>" : "") + (Utility.extractLocale(item.video) != "" ? "<<" + gameData.localization[40] + ">>" : "") + "\n";
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
					if (Utility.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), levelSelected.OwnerDocument))
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
			testLevelBt.interactable = true;
			downloadLevelBt.interactable = true;
			addToScenario.interactable = true;
		}
        else
        {
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[11], path), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
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

	public void saveScenario(TMP_InputField scenarioName)
	{
		if (!Utility.CheckSaveNameValidity(scenarioName.text))
		{
			localCallback = null;
			string invalidChars = "";
			foreach (char someChar in Path.GetInvalidFileNameChars())
				if (Char.IsPunctuation(someChar) || Char.IsSymbol(someChar))
					invalidChars += someChar+" ";
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[12], invalidChars), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			// Be sure saving windows is enabled
			GameObjectManager.setGameObjectState(scenarioName.transform.parent.parent.gameObject, true);
		}
		else
		{
			// add file extension
			if (!scenarioName.text.EndsWith(".xml"))
				scenarioName.text += ".xml";
			if (Application.platform != RuntimePlatform.WebGLPlayer && File.Exists(Application.persistentDataPath + "/Scenario/" + scenarioName.text))
			{
				localCallback = null;
				localCallback += delegate { saveToFile(scenarioName); };
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[13], scenarioName.text), OkButton = gameData.localization[3], CancelButton = gameData.localization[4], call = localCallback });
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
		scenarioExport += "<scenario name=\""+ scenarioName.text.Replace('\"', '\'') + "\" desc=\""+ scenarioAbstract.text.Replace('\"', '\'') + "\">\n";
		foreach (Transform child in contentScenario.transform)
		{
			DataLevel dataLevel = child.GetComponent<DataLevelBehaviour>().data;
			if (dataLevel.name == null || dataLevel.name == "")
				dataLevel.name = Path.GetFileNameWithoutExtension(dataLevel.src);
			scenarioExport += "\t<level src=\"" + child.GetComponentInChildren<TMP_Text>().text + "\" name=\"" + dataLevel.name + "\"";
			if (dataLevel.overridedDialogs != null && dataLevel.overridedDialogs.Count > 0)
			{
				scenarioExport += " >\n\t\t<dialogs>\n";
				foreach (Dialog dialog in dataLevel.overridedDialogs)
				{
					scenarioExport += "\t\t\t<dialog ";
					scenarioExport += dialog.text != null && dialog.text != "" ? "text=\"" + dialog.text + "\" " : "";
					scenarioExport += dialog.img != null && dialog.img != "" ? "img=\"" + dialog.img + "\" " : "";
					scenarioExport += dialog.imgHeight != -1 ? "imgHeight=\"" + dialog.imgHeight + "\" " : "";
					scenarioExport += dialog.camX != -1 ? "camX=\"" + dialog.camX + "\" " : "";
					scenarioExport += dialog.camY != -1 ? "camY=\"" + dialog.camY + "\" " : "";
					scenarioExport += dialog.sound != null && dialog.sound != "" ? "sound=\"" + dialog.sound + "\" " : "";
					scenarioExport += dialog.video != null && dialog.video != "" ? "video=\"" + dialog.video + "\" " : "";
					scenarioExport += "enableInteraction=\"" + (dialog.enableInteraction ? "1" : "0") + "\" ";
					scenarioExport += "briefingType=\"" + dialog.briefingType + "\" />\n";
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

		// generate XML structure from string
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(scenarioExport);
		Utility.removeComments(doc);

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			Save(buildScenarioContent(), scenarioName.text);
			// Add/Replace scenario content in memory
			string fakeUri = Application.streamingAssetsPath + "/Scenario/LocalFiles/" + scenarioName.text;
			TitleScreenSystem.instance.updateScenarioContent(new Uri(fakeUri).AbsoluteUri, doc);
		}
		else
		{
			try
			{
				// Create all necessary directories if they don't exist
				Directory.CreateDirectory(Application.persistentDataPath + "/Scenario");
				string path = Application.persistentDataPath + "/Scenario/" + scenarioName.text;
				// Write on disk
				File.WriteAllText(path, scenarioExport);
				// Add/Replace scenario content in memory
				TitleScreenSystem.instance.updateScenarioContent(path, doc);

				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[14], Application.persistentDataPath, "Scenario", scenarioName.text), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			}
			catch (Exception e)
			{
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[15], e.Message), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			}
		}
		// Be sure saving windows is disabled
		GameObjectManager.setGameObjectState(scenarioName.transform.parent.parent.gameObject, false);
	}

	// See ButtonSaveScenario
	public void displaySavingPanel(TMP_InputField scenarName)
	{
		GameObjectManager.setGameObjectState(savingPanel, true);
		// init savingPanel to the name of scenario
		savingPanel.GetComponentInChildren<TMP_InputField>(true).text = scenarName.text;
		EventSystem.current.SetSelectedGameObject(savingPanel.transform.Find("Panel").Find("Buttons").Find("CancelButton").gameObject);
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
				newItem.GetComponentInChildren<TMP_Dropdown>().value = dialog.briefingType;
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
					overridedBriefing.data.name = input.text.Replace('\"', '\'');
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
						dialog.text = input.text.Replace('\"', '\'');
					else if (input.name == "ImgPath_input" && input.text != "")
						dialog.img = input.text.Replace('\"', '\'');
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
				}
				dialog.enableInteraction = child.GetComponentInChildren<Toggle>().isOn;
				dialog.briefingType = child.GetComponentInChildren<TMP_Dropdown>().value;
				overridedBriefing.data.overridedDialogs.Add(dialog);
			}
			showLevelInfo(Utility.extractFileName(path), overridedBriefing);
		}
    }

	public void addNewBriefing(GameObject parent)
    {
		GameObject newItem = GameObject.Instantiate(briefingItemPrefab, parent.transform, false);
		GameObjectManager.bind(newItem);
	}

	//Used on scenario editing window (see button ButtonTestLevel)
	public void testLevel(DataLevelBehaviour dlb)
	{
		// We save the scenario currently edited
		// We can't use GameObjectManager because the update has to be done immediately due to scene loading in testLevel function
		TitleScreenSystem.instance.LoadLevelOrScenario(Utility.editingScenario, buildScenarioContent());
		testLevel(dlb.data, Utility.testFromScenarioEditor);
	}

	private void testLevel(DataLevel dl, string context)
    {
		gameData.selectedScenario = context;
		WebGlScenario test = new WebGlScenario();
		test.levels = new List<DataLevel>();
		test.levels.Add(dl);
		gameData.scenarios[context] = test;
		gameData.levelToLoad = 0;
		GameObjectManager.loadScene("MainScene");
	}

	private void testLevelPath(string levelToLoad)
	{
		DataLevel dl = new DataLevel();
		dl.src = new Uri(Application.streamingAssetsPath + "/" + levelToLoad).AbsoluteUri;
		dl.name = Path.GetFileNameWithoutExtension(dl.src);
		testLevel(dl, Utility.testFromUrl);
	}

	public void downloadLevel(DataLevelBehaviour dlb)
	{
		DownloadLevel(dlb.data.src);
	}
}