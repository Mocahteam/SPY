using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Famille
	private Family competence_f = FamilyManager.getFamily(new AllOfComponents(typeof(Competence)));

	// Variable
	public GameObject panelInfoComp;

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		foreach(GameObject comp in competence_f)
        {
            if (!comp.GetComponent<Competence>().active)
            {
				comp.GetComponent<Toggle>().interactable = false;
			}
        }
	}

	public void startLevel()
    {
		Debug.Log("Start level selon competence");
    }

	public void infoCompetence(GameObject comp)
	{
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.GetComponent<Competence>().info;
		comp.transform.Find("Label").GetComponent<Text>().fontStyle = FontStyle.Bold;
	}

	public void resetViewInfoCompetence(GameObject comp)
    {
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = "";
		comp.transform.Find("Label").GetComponent<Text>().fontStyle = FontStyle.Normal;
	}
}