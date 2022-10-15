using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System;
using System.Data;

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

	private List<XmlNode> levels = new List<XmlNode>();

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

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		
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

		if (levels.Count == 0)
			// load levels
			loadLevels(Application.streamingAssetsPath);
		// default, select all levels
		List<XmlNode> selectedLevels = new List<XmlNode>();
		foreach (XmlNode level in levels)
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
							foreach (XmlNode tag in selectedLevels[l].OwnerDocument.GetElementsByTagName(constraint.tag))
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
					bool result = (bool)dt.Compute(rule, "");
					if (!result)
						selectedLevels.RemoveAt(l);
				}
            }
		}
		if (nbCompSelected == 0)
			selectedLevels.Clear();
		Debug.Log(selectedLevels.Count);
		/*// On parcourt tous les levels disponibles pour les copier dans une liste temporaire
		List<string> copyLevel = new List<string>();
		int nbCompActive = 0;
		bool conditionStartLevelOk = true;

		bool levelLD = false;
		// On regarde si des competences concernant le level design on été selectionnées
		foreach (GameObject comp in f_competence)
		{
            if (comp.GetComponent<Toggle>().isOn)
            {
				nbCompActive += 1;
				// On fait ça avec le level design
				foreach (string f_key in gameData.GetComponent<FunctionalityParam>().levelDesign.Keys)
				{
                    if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(f_key) && comp.GetComponent<Competence>().compLinkWhitFunc.Contains(f_key))
                    {
						gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Add(f_key);
						addSelectFuncLinkbyFunc(f_key);
					}
					if (comp.GetComponent<Competence>().compLinkWhitFunc.Contains(f_key) && gameData.GetComponent<FunctionalityParam>().levelDesign[f_key])
                    {
						levelLD = true;
                    }
				}
			}
		}

        // Si aucune compétence n'a été selectionnée on ne chargera pas de niveau
        if (nbCompActive <= 0)
        {
			conditionStartLevelOk = false;
		}

        if (conditionStartLevelOk)
        {
			// 2 cas de figures : 
			// Demande de niveau spécial pour la compétence
			// Demande de niveau sans compétence LD
			if (levelLD)
			{
				// On parcourt le dictionnaires des fonctionnalités de level design
				// Si elle fait partie des fonctionnalités selectionnées, alors on enregistre les levels associés à la fonctionnalité
				foreach (string f_key in gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign.Keys)
				{
                    if (gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(f_key))
                    {
						foreach(string level in gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign[f_key])
                        {
							copyLevel.Add(level);
						}
					}
				}
				// On garde ensuite les niveaux qui contienent exclusivement toutes les fonctionalités selectionnées
				foreach (string f_key in gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign.Keys)
				{
					if (gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(f_key))
					{
						for(int i = 0; i < copyLevel.Count;)
                        {
                            if (!gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign[f_key].Contains(copyLevel[i]))
                            {
								copyLevel.Remove(copyLevel[i]);
                            }
                            else
                            {
								i++;
                            }
                        }
					}
				}
			}
			else if (!levelLD)
			{
				// On parcourt le dictionnaire des fonctionnalités level design
				// On supprime de la liste des niveaux possibles tous les niveaux appellant des fonctionnalités de level design
				foreach (List<string> levels in gameData.levelList.Values)
				{
					// On créer une copie de la liste des niveaux disponibles
					foreach (string level in levels)
						copyLevel.Add(level);
				}

				foreach (List<string> levels in gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign.Values)
				{
					foreach(string level in levels)
                    {
						copyLevel.Remove(level);
                    }
				}
			}
		}
        else
        {
			string message = "Erreur, pas de compétence sélectionnée!";
			displayMessageUser(message);
		}

		// Si on a au moins une compétence activée et un niveau en commun
		// On lance un niveau selectionné aléatoirement parmis la liste des niveaux restants
		if (copyLevel.Count != 0)
        {
			if (copyLevel.Count > 1)
            {
				// On selectionne le niveau aléatoirement
				var rand = new System.Random();
				int r = rand.Next(0, copyLevel.Count);
				string levelSelected = copyLevel[r];
				// On split la chaine de caractére pour pouvoir récupérer le dossier ou se trouve le niveau selectionné
				var level = levelSelected.Split('\\');
				string folder = level[level.Length - 2];
				gameData.levelToLoad = (folder, gameData.levelList[folder].IndexOf(levelSelected));
			}
            else
            {
				string levelSelected = copyLevel[0];
				// On split la chaine de caractére pour pouvoir récupérer le dossier ou se trouve le niveau selectionné
				var level = levelSelected.Split('\\');
				string folder = level[level.Length - 2];
				gameData.levelToLoad = (folder, gameData.levelList[folder].IndexOf(levelSelected));
			}
			GameObjectManager.loadScene("MainScene");
		}
		else // Sinon on signale qu'aucune compétence n'est selectionnée ou qu'aucun niveau n'est disponible
        {
			string message = "Pas de niveau disponible pour l'ensemble des compétences selectionnées";
			displayMessageUser(message);
		}*/
	}

	private void loadLevels(string path)
    {
		// try to load all child files
		foreach (string fileName in Directory.GetFiles(path))
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(fileName);
				EditingUtility.removeComments(doc);
				// a valid level must have only one tag "level"
				if (doc.GetElementsByTagName("level").Count == 1)
					levels.Add(doc.GetElementsByTagName("level")[0]);
			}
            catch{}
		}

		// explore subdirectories
		foreach (string directory in Directory.GetDirectories(path))
			loadLevels(directory);
	}

	// Used when PointerOver CategorizeCompetence prefab (see in editor)
	public void infoCompetence(Competency comp)
	{
		if (comp != null)
			panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.description;
	}

	// Affiche le panel message avec le bon message
	public void displayMessageUser(string message)
    {
		messageForUser.text = message;
		panelInfoUser.SetActive(true);
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