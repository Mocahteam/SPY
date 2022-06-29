using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Famille
	private Family competence_f = FamilyManager.getFamily(new AllOfComponents(typeof(Competence)));

	// Variable
	public GameObject panelSelectComp; // Panneau de selection des compétences
	public GameObject panelInfoComp; // Panneau d'information des compétences
	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, conflit dans la selection des compétences etc...)
	public string pathParamComp = "/StreamingAssets/ParamPrefab/ParamCompetence.csv"; // Chemin d'acces du fichier CSV contenant les info à charger des competences

	private GameData gameData;
	private TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressé à l'utilisateur
	private List<string> listCompSelectUser = new List<string>(); // Enregistre temporairement les compétences séléctionnées par le user
	private List<string> listCompSelectUserSave = new List<string>(); // Contient les compétences selectionnées par le user

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		messageForUser = panelInfoUser.transform.Find("Panel").Find("Message").GetComponent<TMP_Text>();

		// Va disparaitre une fois le préfab fini
		openPanelSelectComp(); 
	}

	IEnumerator noSelect(GameObject comp)
    {
		yield return null;

		listCompSelectUser = new List<string>(listCompSelectUserSave);
		resetSelectComp();
		foreach(string level in listCompSelectUserSave)
        {
			foreach(GameObject c in competence_f)
            {
				if(c.name == level)
                {
					selectComp(c, false);
				}
			}
		}
		MainLoop.instance.StopCoroutine(noSelect(comp));
	}

	public void openPanelSelectComp()
    {
		try
		{
			// On charge les données pour chaque compétence
			loadParamComp();
			// On désactive les compétences pas encore implémenté
			desactiveToogleComp();
			// Note pour chaque compétence les niveaux ou elle est présente
			readXMLinfo();
		}
		catch
		{
			string message = "Erreur chargement fichier de parametrage des compétences!\n";
			message += "Vérifier que le fichier csv et les informations contenues sont au bon format";
			displayMessageUser(message);
			// Permetra de fermer le panel de selection des competence lorsque le user apuie sur le bouton ok du message d'erreur
			//panelSelectComp.GetComponent<ParamCompetenceSystemBridge>().closePanelParamComp = true;
		}
	}

	public void startLevel()
    {
		// On parcourt tous les levels disponible pour les copier dans une liste temporaire
		List<string> copyLevel = new List<string>();
		foreach(List<string> levels in gameData.levelList.Values)
		{
			// On créer une copie de la liste des niveaux disponible
			foreach(string level in levels)
			copyLevel.Add(level);
		}

		int nbCompActive = 0;

		// On parcours chaque compétence selectionner
		foreach (GameObject comp in competence_f)
		{
			// Si La compétence est activé
			if (comp.GetComponent<Toggle>().isOn)
			{
				nbCompActive += 1;
				// On parcourt ce qui reste des niveaux possible et pour chaque niveau on regarde si il est présent dans la compétence selectionnée
				// Si se n'est pas le cas on le supprime de la liste
				for(int i = 0; i < copyLevel.Count;) {
					bool levelOk = false;
					foreach(string level in comp.GetComponent<Competence>().listLevel)
                    {
						if(level == copyLevel[i])
                        {
							levelOk = true;
						}
                    }

                    if (levelOk)
                    {
						i++;
                    }
                    else
                    {
						copyLevel.Remove(copyLevel[i]);
					}
                }
			}
		}

		// Si on a au moins une compétence activé et un niveau en commun
		// On lance un niveau selectionné aléatoirement parmis la liste des niveaux restant
		if (nbCompActive != 0 && copyLevel.Count != 0)
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
		else // Sinon on signal que aucune compétence n'est selectionné ou que aucun niveau n'est disponible
        {
			string message = "";
			// Si pas de competence selectionnée
			if (nbCompActive == 0)
            {
				message = "Pas de compétence sélectionnée";
            }
			else if (copyLevel.Count == 0) // Si pas de niveau dispo
            {
				message = "Pas de niveau disponible pour l'ensemble des compétences selectionnées";
			}
            else
            {
				message = "Erreur run level ";
			}
			displayMessageUser(message);
		}
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

	// On parcourt toutes les compétences
	// On desactive toutes les compétences non implémenté et les compétences ne pouvant plus être selectionné
	// On selectionne automatiquement les competences linker
	public void selectComp(GameObject comp, bool userSelect)
    {
        if (userSelect)
        {
			addOrRemoveCompSelect(comp, true);
		}
        else
        {
			comp.GetComponent<Toggle>().isOn = true;
		}

		bool error = false;

		foreach (string compSelect in comp.GetComponent<Competence>().compLink)
		{
			foreach (GameObject c in competence_f)
			{
				if (c.name == compSelect && comp.GetComponent<Competence>().active)
				{
					if (c.GetComponent<Toggle>().interactable)
					{
						selectComp(c, false);
					}
					else
					{
						error = true;
						break;
					}
				}
			}
		}


		foreach (string compSelect in comp.GetComponent<Competence>().compNoPossible)
		{
			foreach (GameObject c in competence_f)
			{
				if (c.name == compSelect)
				{
					if (!c.GetComponent<Toggle>().isOn)
					{
						c.GetComponent<Toggle>().interactable = false;
					}
					else
					{
						error = true;
						break;
					}
				}
			}
		}

        if (error)
        {
			string message = "Conflit concernant l'interactibilité de la compétence sélectionné";
			displayMessageUser(message);
			// Deselectionner la compétence
			stratCoroutineNoSelect(comp);
		}
	}

	private void stratCoroutineNoSelect(GameObject comp)
    {
		MainLoop.instance.StartCoroutine(noSelect(comp));
	}

	public void unselectComp(GameObject comp, bool userUnselect)
    {
        if (!userUnselect)
        {
			comp.GetComponent<Toggle>().isOn = false;
		}

		foreach(GameObject competence in competence_f)
        {
			foreach(string compName in comp.GetComponent<Competence>().compLinkUnselect)
            {
				if(compName == competence.name)
                {
					unselectComp(competence, false);
				}
			}

			foreach (string compName in comp.GetComponent<Competence>().compNoPossible)
			{
				if (compName == competence.name)
				{
					competence.GetComponent<Toggle>().interactable = true;
                    if (competence.GetComponent<Toggle>().isOn)
                    {
						unselectComp(competence, false);
					}
				}
			}
		}

		desactiveToogleComp();
		addOrRemoveCompSelect(comp, false);
	}
	// Chargement des parametre des compétences
	private void loadParamComp()
    {
		StreamReader reader = new StreamReader("" + Application.dataPath + pathParamComp);
		bool endOfFile = false;
		while (!endOfFile)
		{
			string data_string = reader.ReadLine();
			if (data_string == null)
			{
				endOfFile = true;
				break;
			}
			var data = data_string.Split(';');

			// On parcourt les élément est on charge les parametre de la compétence sur la bonne compétence
			// Nom de la competence, titre, info
			foreach (GameObject comp in competence_f)
			{
				// Si c'est la compétence
				if (comp.name == data[0])
				{
					// On charge le text de la compétence
					comp.transform.Find("Label").GetComponent<Text>().text = data[1];
					comp.transform.Find("Label").GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
					// On charge les info de la compétence qui sera affiché lorsque l'on survolera celle-ci avec la souris
					comp.GetComponent<Competence>().info = data[2];
					// (temporaire) On charge si la compétence peut être selectionnée (est-elle implémentée)
					comp.GetComponent<Competence>().active = Convert.ToBoolean(data[3]);
					// On charge le vecteur des compétences qui seront automatiquement selectionnées si la compétence est séléctionnée
					var data_link = data[4].Split(',');
					foreach (string value in data_link)
					{
						comp.GetComponent<Competence>().compLink.Add(value);
					}
					// On charge le vecteur des compétences qui seront automatiquement grisées si la compétence est séléctionnée
					var data_noPossible = data[5].Split(',');
					foreach (string value in data_noPossible)
					{
						comp.GetComponent<Competence>().compNoPossible.Add(value);
					}
					// On charge le vecteur des compétences qui seront automatiquement deselectionné si la compétence est déséléctionnée
					var data_unlink = data[6].Split(',');
					foreach (string value in data_unlink)
					{
						comp.GetComponent<Competence>().compLinkUnselect.Add(value);
					}
				}
			}
		}
	}

	// Désactive les toogles pas encore implémenté
	private void desactiveToogleComp()
    {
		foreach (GameObject comp in competence_f)
		{
			if (!comp.GetComponent<Competence>().active)
			{
				comp.GetComponent<Toggle>().interactable = false;
			}
		}
	}

	// Lis tous les fichiers XML des niveaux de chaque dossier afin de charger quelle compétence se trouve dans quel niveau  
	private void readXMLinfo()
    {
        foreach (List<string> levels in gameData.levelList.Values)
        {
			foreach (string level in levels)
			{
				XmlDocument doc = new XmlDocument();
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					doc.LoadXml(level);
					loadInfo(doc, level);
				}
				else
				{
					doc.Load(level);
					loadInfo(doc, level);
				}
			}
		}
	}

	// Parcourt le noeud d'information est apelle les bonnes fonctions pour traiter l'information du niveau
	private void loadInfo(XmlDocument doc, string namelevel)
	{
		XmlNode root = doc.ChildNodes[1];
		foreach (XmlNode child in root.ChildNodes)
		{
			switch (child.Name)
			{
				case "info":
					foreach (XmlNode infoNode in child.ChildNodes)
					{
                        switch (infoNode.Name)
                        {
							case "comp":
								addInfo(infoNode, namelevel);
								break;
						}
					}
					break;
			}
		}
	}

	// Associe à chaque compétence renseigner sa présence dans le niveau
	private void addInfo(XmlNode node, string namelevel)
    {
		foreach (GameObject comp in competence_f)
		{
			if (node.Attributes.GetNamedItem("name").Value == comp.name)
			{
				comp.GetComponent<Competence>().listLevel.Add(namelevel);
			}
		}
	}

	// Ajoute ou retire la compétence de la liste des compétences selectionner manuellement par l'utilisateur
	public void addOrRemoveCompSelect(GameObject comp, bool value)
	{
        if (value)
        {
			// Si la compétence n'est pas encore noté comme avoir été selectionné par le user
            if (!listCompSelectUser.Contains(comp.name))
            {
				listCompSelectUser.Add(comp.name);
			}
		}
        else
        {
			// Si la compétence avait été séléctionné par le user
			if(listCompSelectUser.Contains(comp.name)){
				listCompSelectUser.Remove(comp.name);
			}
		}
	}

	// Reset toutes les compétences en "non selectionné"
	private void resetSelectComp()
    {
		foreach (GameObject comp in competence_f)
		{
			comp.GetComponent<Toggle>().isOn = false;
		}
	}

	// Enregistre la liste des compétence séléctionné par le user
	public void saveListUser()
    {
		listCompSelectUserSave = new List<string>(listCompSelectUser);
	}

	// Ferme le panel de selection des compétences
	// Décoche toutes les compétences cochées
	// vide les listes de suivis des compétences selectionné
	public void closeSelectCompetencePanel()
    {
		panelSelectComp.SetActive(false);
		resetSelectComp();
		listCompSelectUser = new List<string>();
		listCompSelectUserSave = new List<string>();
	}

	// Affiche le panel message avec le bon message
	public void displayMessageUser(string message)
    {
		messageForUser.text = message;
		panelInfoUser.SetActive(true);
	}

	public void changeSizeButtonCategory(GameObject button, float value)
    {
        if(button.GetComponent<RectTransform>().sizeDelta.x == value)
        {
			button.GetComponent<RectTransform>().sizeDelta = new Vector2(button.GetComponent<RectTransform>().sizeDelta.x + 2, button.GetComponent<RectTransform>().sizeDelta.y + 2);
		}
        else
        {
			button.GetComponent<RectTransform>().sizeDelta = new Vector2(button.GetComponent<RectTransform>().sizeDelta.x - 2, button.GetComponent<RectTransform>().sizeDelta.y - 2);	
		}

    }
}