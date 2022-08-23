using UnityEngine;
using FYFY;
using System.Data;
using System.Collections.Generic;

public class ConditionManagement : FSystem {

	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));

	public GameObject endPanel;

	public static ConditionManagement instance;

	public ConditionManagement()
	{
		instance = this;
	}

	// Transforme une sequence de condition en une chaine de caractére
	public void convertionConditionSequence(GameObject condition, List<string> chaine){
		// Check if condition is a BaseCondition
		if (condition.GetComponent<BaseCondition>())
		{
			// On regarde si la condition reçue est un élément ou bien un opérator
			// Si c'est un élément, on le traduit en string et on le renvoie 
			if (condition.GetComponent<BaseCaptor>())
				chaine.Add("" + condition.GetComponent<BaseCaptor>().captorType);
			else
			{
				BaseOperator bo;
				if (condition.TryGetComponent<BaseOperator>(out bo))
				{
					Transform conditionContainer = bo.transform.GetChild(0);
					// Si c'est une négation on met "!" puis on fait une récursive sur le container et on renvoie le tous traduit en string
					if (bo.operatorType == BaseOperator.OperatorType.NotOperator)
					{
						// On vérifie qu'il y a bien un élément présent, son container doit contenir 3 enfants (icone, une BaseCondition et le ReplacementSlot)
						if (conditionContainer.childCount == 3)
						{
							chaine.Add("NOT");
							convertionConditionSequence(conditionContainer.GetComponentInChildren<BaseCondition>().gameObject, chaine);
						}
						else
						{
							GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
						}
					}
					else if (bo.operatorType == BaseOperator.OperatorType.AndOperator)
					{
						// Si les côtés de l'opérateur sont remplis, alors il compte 5 childs (2 ReplacementSlots, 2 BaseCondition et 1 icone), sinon cela veux dire que il manque des conditions
						if (conditionContainer.childCount == 5)
						{
							chaine.Add("(");
							convertionConditionSequence(conditionContainer.GetChild(0).gameObject, chaine);
							chaine.Add("AND");
							convertionConditionSequence(conditionContainer.GetChild(3).gameObject, chaine);
							chaine.Add(")");
						}
						else
						{
							GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
						}
					}
					else if (bo.operatorType == BaseOperator.OperatorType.OrOperator)
					{
						// Si les côtés de l'opérateur sont remplis, alors il compte 5 childs, sinon cela veux dire que il manque des conditions
						if (conditionContainer.childCount == 5)
						{
							chaine.Add("(");
							convertionConditionSequence(conditionContainer.GetChild(0).gameObject, chaine);
							chaine.Add("OR");
							convertionConditionSequence(conditionContainer.GetChild(3).gameObject, chaine);
							chaine.Add(")");
						}
						else
						{
							GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
						}
					}
				}
				else
				{
					Debug.LogError("Unknown BaseCondition!!!");
				}
			}
		} else
			GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
	}

	public bool ifValid(List<string> condition, GameObject scripted)
	{

		string cond = "";
		for (int i = 0; i < condition.Count; i++)
		{
			if (condition[i] == "(" || condition[i] == ")" || condition[i] == "OR" || condition[i] == "AND" || condition[i] == "NOT")
			{
				cond = cond + condition[i] + " ";
			}
			else
			{
				cond = cond + verifCondition(condition[i], scripted) + " ";
			}
		}

		DataTable dt = new DataTable();
		var v = dt.Compute(cond, "");
		bool result;
        try
        {
			result = bool.Parse(v.ToString());
		} catch
        {
			result = false;
        }
		return result;
	}

	public bool verifCondition(string ele, GameObject scripted)
    {
		
		bool ifok = false;
		// get absolute target position depending on player orientation and relative direction to observe
		// On commence par identifier quelle case doit être regardé pour voir si la condition est respecté
		Vector2 vec = new Vector2();
		switch (scripted.GetComponent<Direction>().direction)
		{
			case Direction.Dir.North:
				vec = new Vector2(0, 1);
				break;
			case Direction.Dir.South:
				vec = new Vector2(0, -1);
				break;
			case Direction.Dir.East:
				vec = new Vector2(1, 0);
				break;
			case Direction.Dir.West:
				vec = new Vector2(-1, 0);
				break;
		}

		// check target position
		switch (ele)
		{
			case "Wall": // walls
				foreach (GameObject go in wallGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "FieldGate": // doors
				foreach (GameObject go in doorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "Enemie": // ennemies
				foreach (GameObject go in droneGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "Terminal": // consoles
				foreach (GameObject go in activableConsoleGO) {
					vec = new Vector2(0, 0);
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
					}
				break;
			case "RedArea": // detectors
				foreach (GameObject go in redDetectorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "Exit": // exits
				foreach (GameObject go in exitGO)
				{
					vec = new Vector2(0, 0);
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				}
				break;
		}
		return ifok;
		
	}
}