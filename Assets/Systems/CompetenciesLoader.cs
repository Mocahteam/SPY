using FYFY;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Charge chaque compétence en GameObject
public class CompetenciesLoader : FSystem
{
	// L'instance
	public static CompetenciesLoader instance;
	public GameObject hiddenCompetencies;
	public GameObject prefabComp;

	private GameData gameData;

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
			if (hiddenCompetencies != null && prefabComp != null)
			{
				// parse all referentials
				List<RawListComp> referentials = gameData.rawReferentials.referentials;
                for (int referentialId = 0; referentialId < referentials.Count; referentialId++)
				{
					// create all competencies
					foreach (RawComp rawComp in referentials[referentialId].list)
					{
						// On instancie la compétence
						GameObject competency = UnityEngine.Object.Instantiate(prefabComp);
						competency.SetActive(false);
						competency.name = rawComp.key;
						Competency comp = competency.GetComponent<Competency>();
						comp.referentialName = referentials[referentialId].name;
						comp.referentialId = referentialId;
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

		Pause = true;
	}
}
