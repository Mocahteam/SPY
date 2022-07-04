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
	private Family competence_f = FamilyManager.getFamily(new AllOfComponents(typeof(Competence))); // Les Toogles compétence
	private Family menuElement_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Competence), typeof(Category))); // Les Toogles compétences et les Catégories qui les réunnis en groupes
	private Family category_f = FamilyManager.getFamily(new AllOfComponents(typeof(Category))); // Les category qui contiendra des sous category ou des competences

	// Variable
	public GameObject panelSelectComp; // Panneau de selection des compétences
	public GameObject panelInfoComp; // Panneau d'information des compétences
	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, conflit dans la selection des compétences etc...)
	public GameObject scrollViewComp; // Le controleur du scroll pour les compétences
	public string pathParamComp = "/StreamingAssets/ParamPrefab/ParamCompetence.csv"; // Chemin d'acces du fichier CSV contenant les info à charger des competences
	public GameObject prefabCateComp; // Prefab de l'affichage d'une catégorie de competence
	public GameObject prefabComp; // Prefab de l'affichage d'une competence
	public GameObject ContentCompMenu; // Panneau qui contient la liste des catégories et compétences

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

	// Permet d'attacher à chaque catégorie les sous-categorie et compétences qui la compose
	IEnumerator attacheComptWithCat()
    {
		yield return null;

		foreach (GameObject cat in category_f)
		{
			foreach(GameObject element in menuElement_f)
            {
				if (element.GetComponent<MenuComp>().catParent == cat.name)
				{
					cat.GetComponent<Category>().listAttachedElement.Add(element.name);
				}
			}
		}

		MainLoop.instance.StopCoroutine(attacheComptWithCat());
	}

	// Permet de lancer les différentes fonctions que l'on a besoin pour le démarrage APRES que les familles soient mise à jours
	IEnumerator startAfterFamillyOk() {
		yield return null;

		// On désactive les compétences pas encore implémenté
		desactiveToogleComp();
		// On décale les sous-catégorie et compétence selon leur place dans la hierarchie
		displayCatAndComp();

		MainLoop.instance.StopCoroutine(startAfterFamillyOk());
	}

	public void openPanelSelectComp()
	{
		try
		{
			// On charge les données pour chaque compétence
			loadParamComp();
			// Note pour chaque compétence les niveaux ou elle est présente
			readXMLinfo();
			MainLoop.instance.StartCoroutine(startAfterFamillyOk());
			// On demare la corroutine pour attacher chaque competence et sous-categorie et leur catégorie
			MainLoop.instance.StartCoroutine(attacheComptWithCat());
		}
		catch
		{
			string message = "Erreur chargement fichier de parametrage des compétences!\n";
			message += "Vérifier que le fichier csv et les informations contenues sont au bon format";
			displayMessageUser(message);
			// Permetra de fermer le panel de selection des competence lorsque le user apuie sur le bouton ok du message d'erreur
			panelSelectComp.GetComponent<ParamCompetenceSystemBridge>().closePanelParamComp = true;
		}
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
			string[] data = data_string.Split(';');

			// Si c'est une compétence
			if(data[0] == "Comp")
            {
				createCompObject(data);
			}// Sinon si c'est une catégorie
			else if(data[0] == "Cat"){
				createCatObject(data);
			}
			
		}
	}

	// Instancie et parametre la compétence à afficher
	public void createCatObject(string[] data)
    {
		// On instancie la catégorie
		GameObject category = UnityEngine.Object.Instantiate(prefabCateComp);
		// On l'attache au content
		category.transform.SetParent(ContentCompMenu.transform);
		// On signal à quel catégori la compétence appartien
		if(data[1] != "None")
        {
			category.GetComponent<MenuComp>().catParent = data[1];
		}
		// On charge les données
		category.name = data[2];
		category.transform.Find("Label").GetComponent<TMP_Text>().text = data[3];
		category.GetComponent<MenuComp>().info = data[4];

		GameObjectManager.bind(category);
	}

	// Instancie et parametre la sous-compétence à afficher
	public void createCompObject(string[] data)
	{
		// On instancie la catégorie
		GameObject competence = UnityEngine.Object.Instantiate(prefabComp);
		// On signal à quel catégori la compétence appartien
		competence.GetComponent<Competence>().catParent = data[1];
		// On l'attache au content
		competence.transform.SetParent(ContentCompMenu.transform);
		competence.name = data[2];
		// On charge le text de la compétence
		competence.transform.Find("Label").GetComponent<TMP_Text>().text = data[3];
		competence.transform.Find("Label").GetComponent<TMP_Text>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
		// On charge les info de la compétence qui sera affiché lorsque l'on survolera celle-ci avec la souris
		competence.GetComponent<Competence>().info = data[4];
		// (temporaire) On charge si la compétence peut être selectionnée (est-elle implémentée)
		competence.GetComponent<Competence>().active = Convert.ToBoolean(data[5]);
		// On charge le vecteur des compétences qui seront automatiquement selectionnées si la compétence est séléctionnée
		var data_link = data[6].Split(',');
		foreach (string value in data_link)
		{
			competence.GetComponent<Competence>().compLink.Add(value);
		}
		// On charge le vecteur des compétences qui seront automatiquement grisées si la compétence est séléctionnée
		var data_noPossible = data[7].Split(',');
		foreach (string value in data_noPossible)
		{
			competence.GetComponent<Competence>().compNoPossible.Add(value);
		}
		// On charge le vecteur des compétences qui seront automatiquement deselectionné si la compétence est déséléctionnée
		var data_unlink = data[8].Split(',');
		foreach (string value in data_unlink)
		{
			competence.GetComponent<Competence>().compLinkUnselect.Add(value);
		}

		GameObjectManager.bind(competence);
	}

	// Mise en place décaler les sous-categories et compétences
	private void displayCatAndComp()
    {
		foreach(GameObject element in menuElement_f) { 
			// Si l'élément à un parent
			if(element.GetComponent<MenuComp>().catParent != "")
            {
				int nbParent = nbParentInHierarchiComp(element);

                if (element.GetComponent<Competence>())
                {
					element.transform.Find("Background").position = new Vector3(element.transform.Find("Background").position.x + (nbParent * 15), element.transform.Find("Background").position.y, element.transform.Find("Background").position.z);
					element.transform.Find("Label").position = new Vector3(element.transform.Find("Label").position.x + (nbParent * 15), element.transform.Find("Label").position.y, element.transform.Find("Label").position.z);
				}
				else if (element.GetComponent<Category>())
                {
					element.transform.Find("Label").position = new Vector3(element.transform.Find("Label").position.x + (nbParent * 15), element.transform.Find("Label").position.y, element.transform.Find("Label").position.z);
					element.transform.Find("ButtonHide").position = new Vector3(element.transform.Find("ButtonHide").position.x + (nbParent * 15), element.transform.Find("ButtonHide").position.y, element.transform.Find("ButtonHide").position.z);
					element.transform.Find("ButtonShow").position = new Vector3(element.transform.Find("ButtonShow").position.x + (nbParent * 15), element.transform.Find("ButtonShow").position.y, element.transform.Find("ButtonShow").position.z);
				}
			}
		}
    }

	private int nbParentInHierarchiComp(GameObject element)
    {
		int nbParent = 1;

		foreach (GameObject ele in menuElement_f){ 
			if(ele.name == element.GetComponent<MenuComp>().catParent && ele.GetComponent<MenuComp>().catParent != "")
            {
				nbParent += nbParentInHierarchiComp(ele);
			}
		}
			return nbParent;
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
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.GetComponent<MenuComp>().info;
		comp.transform.Find("Label").GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

        if (comp.GetComponent<Category>())
        {
			foreach(Transform child in comp.transform){
                if (child.GetComponent<Button>())
                {
					Color col = new Color(1f, 1f, 1f);
					if (child.name == "ButtonHide")
                    {
						col = new Color(0.8313726f, 0.2862745f, 0.2235294f);
					}
					else if (child.name == "ButtonShow")
                    {
						col = new Color(0.2392157f, 0.8313726f, 0.2235294f);
					}
					child.GetComponent<Image>().color = col;
				}
            }
        }
	}

	public void resetViewInfoCompetence(GameObject comp)
    {
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = "";
		comp.transform.Find("Label").GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Normal;

		if (comp.GetComponent<Category>()){
			foreach (Transform child in comp.transform)
			{
				if (child.GetComponent<Button>())
				{
					child.GetComponent<Image>().color = new Color(1f, 1f, 1f);
				}
			}
		}
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

	// Cache ou montre les éléments associés à la catégory
	public void viewOrHideCompList(GameObject category)
    {
		category.GetComponent<Category>().hideList = !category.GetComponent<Category>().hideList;

		foreach (GameObject element in menuElement_f)
        {
            if (category.GetComponent<Category>().listAttachedElement.Contains(element.name))
            {
				element.SetActive(!category.GetComponent<Category>().hideList);
            }
        }
	}

	// Active ou désactive la bouton
	// Cette fonction est résèrvé à la gestion du bonton à afficher à coté de la catégorie si jamais le user appuie sur le text pour faire apparaitre ou disparaitre la liste associée
	public void hideOrShowButtonCategory(GameObject button)
    {
		button.SetActive(!button.activeSelf);
	}
}