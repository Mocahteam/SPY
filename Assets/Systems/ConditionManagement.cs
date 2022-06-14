using UnityEngine;
using FYFY;
using System.Data;

public class ConditionManagement : FSystem {

	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
	private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(CapsuleCollider), typeof(Position), typeof(ParticleSystem)), new AnyOfTags("Coin"));

	public GameObject endPanel;

	public static ConditionManagement instance;

	public ConditionManagement()
	{
		instance = this;
	}

	// Transforme une sequence de condition en une chaine de caractére
	public string[] convertionConditionSequence(GameObject condition, string[] chaine)
	{
		//string[] chaine = new string[] { };
		// On regarde si la condition reçue est un élément ou bien un opérator
		// Si c'est un élément, on le traduit en string et on le renvoie 
		if (condition.GetComponent<BaseCondition>().Type == BaseCondition.blockType.Element)
		{
			string[] copyChaine = new string[chaine.Length + 1];
			for (int i = 0; i < chaine.Length; i++)
			{
				copyChaine[i] = chaine[i];
			}
			//chaine = chaine + condition.GetComponent<BaseCondition>().conditionType;
			copyChaine[copyChaine.Length - 1] = "" + condition.GetComponent<BaseCondition>().conditionType;
			return copyChaine;
		}
		else if (condition.GetComponent<BaseCondition>().Type == BaseCondition.blockType.Operator)
		{
			// Si c'est une négation on met "!" puis on fait une récursive sur le container et on renvoie le tous traduit en string
			if (condition.GetComponent<BaseCondition>().conditionType == BaseCondition.ConditionType.NotOperator)
            {
				string[] copyChaine = new string[chaine.Length + 1];
				for (int i = 0; i < chaine.Length; i++)
				{
					copyChaine[i] = chaine[i];
				}
				// On vérifie qu'il y a bien un élément présent
				if (!condition.transform.GetChild(1).gameObject.GetComponent<EndBlockScriptComponent>())
                {
					//chaine = chaine + "NOT" + convertionConditionSequence(condition.transform.GetChild(1).gameObject);
					copyChaine[copyChaine.Length - 1] = "NOT";
					copyChaine = convertionConditionSequence(condition.transform.GetChild(1).gameObject, copyChaine);
				}
                else
                {
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
				return copyChaine;
			}
			else if (condition.GetComponent<BaseCondition>().conditionType == BaseCondition.ConditionType.AndOperator)
			{
				// Si les côté de l'opérateur sont remplit, alors il compte 6 child, sinon cela veux dire que il manque des conditions
				if (condition.transform.childCount == 6)
                {
					string[] copyChaine = new string[chaine.Length + 1];
					for (int i = 0; i < chaine.Length; i++)
					{
						copyChaine[i] = chaine[i];
					}
					//chaine = chaine + "(" + convertionConditionSequence(condition.transform.GetChild(1).gameObject) + "AND" + convertionConditionSequence(condition.transform.GetChild(4).gameObject) + ")";
					copyChaine[copyChaine.Length - 1] = "(";
					copyChaine = convertionConditionSequence(condition.transform.GetChild(1).gameObject, copyChaine);
					string[] newCopyChaine = new string[copyChaine.Length + 1];
					for (int i = 0; i < copyChaine.Length; i++)
					{
						newCopyChaine[i] = copyChaine[i];
					}
					newCopyChaine[newCopyChaine.Length - 1] = "AND";
					newCopyChaine = convertionConditionSequence(condition.transform.GetChild(1).gameObject, newCopyChaine);
					copyChaine = new string[newCopyChaine.Length + 1];
					for (int i = 0; i < newCopyChaine.Length; i++)
					{
						copyChaine[i] = newCopyChaine[i];
					}
					copyChaine[copyChaine.Length - 1] = ")";
					return copyChaine;
				}
                else
                {
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
			}
			else if (condition.GetComponent<BaseCondition>().conditionType == BaseCondition.ConditionType.OrOperator)
			{
				// Si les côté de l'opérateur sont remplit, alors il compte 6 child, sinon cela veux dire que il manque des conditions
				if (condition.transform.childCount == 6)
				{
					string[] copyChaine = new string[chaine.Length + 1];
					for (int i = 0; i < chaine.Length; i++)
					{
						copyChaine[i] = chaine[i];
					}
					//chaine = chaine + "(" + convertionConditionSequence(condition.transform.GetChild(1).gameObject) + "OR" + convertionConditionSequence(condition.transform.GetChild(4).gameObject) + ")";
					copyChaine[copyChaine.Length - 1] = "(";
					copyChaine = convertionConditionSequence(condition.transform.GetChild(1).gameObject, copyChaine);
					string[] newCopyChaine = new string[copyChaine.Length + 1];
					for (int i = 0; i < copyChaine.Length; i++)
					{
						newCopyChaine[i] = copyChaine[i];
					}
					newCopyChaine[newCopyChaine.Length - 1] = "OR";
					newCopyChaine = convertionConditionSequence(condition.transform.GetChild(1).gameObject, newCopyChaine);
					copyChaine = new string[newCopyChaine.Length + 1];
					for (int i = 0; i < newCopyChaine.Length; i++)
					{
						copyChaine[i] = newCopyChaine[i];
					}
					copyChaine[copyChaine.Length - 1] = ")";
					return copyChaine;
				}
				else
				{
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
			}
		}

		return new string[] { };
	}

	public bool ifValid(string[] condition, GameObject scripted)
	{

		string cond = "";
		for (int i = 0; i < condition.Length; i++)
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
		return bool.Parse(v.ToString());
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
				foreach (GameObject go in activableConsoleGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "RedArea": // detectors
				foreach (GameObject go in redDetectorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
				/*
			case 6: // coins
				foreach (GameObject go in coinGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
				*/
		}
		return ifok;
		
	}
}