using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

// Peuple les listes déroulantes pour sélectionner les référentiels + charge chaque compétence en GameObject
public class CompetenciesLoader : FSystem
{
	private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));
	private Family f_compSelector = FamilyManager.getFamily(new AnyOfTags("CompetencySelector"), new AllOfComponents(typeof(TMP_Dropdown)));

	// L'instance
	public static CompetenciesLoader instance;
	public GameObject hiddenCompetencies; // si null => ne pas transformer chaque compétence en gameObject
	public GameObject prefabComp; // si null => ne pas transformer chaque compétence en gameObject

	private GameData gameData;
	private UnityAction localCallback;

	public CompetenciesLoader()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();
			if (gameData.rawReferentials.referentials.Count == 0)
			{
				// load competencies (required for level analysis)
				string referentialsPath = new Uri(Application.streamingAssetsPath + "/Competencies/competenciesReferential.json").AbsoluteUri;
				MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath));
			}
			else
			{
				createReferentials();
				createCompetencies();
			}
		}
	}

	private IEnumerator GetCompetenciesWebRequest(string referentialsPath)
	{
		UnityWebRequest www = UnityWebRequest.Get(referentialsPath);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Localization loc = gameData.GetComponent<Localization>();
			localCallback = null;
			localCallback += delegate { MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath)); };
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(loc.localization[6], referentialsPath, www.error), OkButton = loc.localization[5], CancelButton = loc.localization[2], call = localCallback });
		}
		else
		{
			while (f_localizationLoaded.Count == 0) // waiting localization loaded
				yield return null;

			try
			{
				gameData.lastReferentialSelected = 0;
				gameData.rawReferentials = JsonUtility.FromJson<RawListReferential>(www.downloadHandler.text);
				createReferentials();
				createCompetencies();
			}
			catch (Exception e)
			{
				Localization loc = gameData.GetComponent<Localization>();
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(loc.localization[7], e.Message), OkButton = loc.localization[0], CancelButton = loc.localization[1], call = localCallback });
			}
		}
	}

	private void createReferentials()
	{
		foreach (GameObject selector in f_compSelector)
			selector.GetComponent<TMP_Dropdown>().ClearOptions();

		// add referential to dropdone
		List<TMP_Dropdown.OptionData> referentials = new List<TMP_Dropdown.OptionData>();
		foreach (RawListComp rlc in gameData.rawReferentials.referentials)
			referentials.Add(new TMP_Dropdown.OptionData(rlc.name));
		if (referentials.Count > 0)
		{
			foreach (GameObject selectorGO in f_compSelector)
			{
				TMP_Dropdown selector = selectorGO.GetComponent<TMP_Dropdown>();
				selector.AddOptions(referentials);
				selector.value = gameData.lastReferentialSelected;
				// Be sure dropdone is active
				GameObjectManager.setGameObjectState(selectorGO, true);
			}
		}
		else
		{
			foreach (GameObject selector in f_compSelector)
				// Hide dropdown
				GameObjectManager.setGameObjectState(selector, false);
		}
	}

	private void createCompetencies()
    {
		if (hiddenCompetencies != null && prefabComp != null)
		{
			// parse all referentials
			foreach (RawListComp referential in gameData.rawReferentials.referentials)
			{
				// create all competencies
				foreach (RawComp rawComp in referential.list)
				{
					// On instancie la compétence
					GameObject competency = UnityEngine.Object.Instantiate(prefabComp);
					competency.SetActive(false);
					competency.name = rawComp.key;
					Competency comp = competency.GetComponent<Competency>();
					comp.referential = referential.name;
					comp.parentKey = rawComp.parentKey;
					comp.id = rawComp.name;
					comp.description = Utility.extractLocale(rawComp.description);
					comp.filters = rawComp.filters;
					comp.rule = rawComp.rule;

					// On l'attache au content
					competency.transform.SetParent(hiddenCompetencies.transform);
					GameObjectManager.bind(competency);
				}
			}
		}
	}

	// used in dropdown referential selectors
	public void saveReferetialSelected(int referentialId)
	{
		gameData.lastReferentialSelected = referentialId;
	}
}
