using FYFY;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Peuple les listes déroulantes pour sélectionner les référentiels + charge chaque compétence en GameObject
public class ReferentialLoader : FSystem
{
	private Family f_compSelector = FamilyManager.getFamily(new AnyOfTags("CompetencySelector"), new AllOfComponents(typeof(TMP_Dropdown)));

	// L'instance
	public static ReferentialLoader instance;

	private GameData gameData;

	public ReferentialLoader()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();
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

		Pause = true;
	}

	// used in dropdown referential selectors
	public void saveReferetialSelected(int referentialId)
	{
		gameData.lastReferentialSelected = referentialId;
	}
}
