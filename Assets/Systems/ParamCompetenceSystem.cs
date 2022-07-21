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
	public string pathParamComp = "/StreamingAssets/ParamCompFunc/ParamCompetence.csv"; // Chemin d'acces du fichier CSV contenant les info à charger des competences
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
		desactiveToogleComp();
		foreach (string level in listCompSelectUserSave)
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
			// Note pour chaque fonction les niveaux ou elle sont présentes
			readXMLinfo();
			Debug.Log(" ok etape 1");
			// On charge les données pour chaque compétence
			loadParamComp();
			Debug.Log(" ok etape 2");
			MainLoop.instance.StartCoroutine(startAfterFamillyOk());
			Debug.Log(" ok etape 3");
			// On demare la corroutine pour attacher chaque competence et sous-categorie et leur catégorie
			MainLoop.instance.StartCoroutine(attacheComptWithCat());
			Debug.Log(" ok etape 4");
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
		// On charge le vecteur des Fonction lié à la compétence
		var data_link = data[5].Split(',');
		foreach (string value in data_link)
		{
			competence.GetComponent<Competence>().compLinkWhitFunc.Add(value);
		}
		// On charge le vecteur des compétences qui seront automatiquement selectionnées si la compétence est séléctionnée
		data_link = data[6].Split(',');
		foreach (string value in data_link)
		{
			competence.GetComponent<Competence>().compLinkWhitComp.Add(value);
		}
		// On charge le vecteur des compétences dont au moins l'une devra être selectionné en m^me temps que celle selectionné actuellement
		data_link = data[7].Split(',');
		foreach (string value in data_link)
		{
			competence.GetComponent<Competence>().listSelectMinOneComp.Add(value);
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

	// Fonction pouvant être apellé par récusrcivité
	// Permet de renvyer à qu'elle profondeur dans la hiérarchie Categorie de la selection des compétences l'élément se trouve
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

	// Lis tous les fichiers XML des niveaux de chaque dossier afin de charger quelle fonctionalité se trouve dans quel niveau  
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
		Debug.Log("Lecture lvl : " + namelevel);
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
							case "func":
								addInfo(infoNode, namelevel);
								break;
						}
					}
					break;
			}
		}
	}

	// Associe à chaque fonctionalité renseigné sa présence dans le niveau
	private void addInfo(XmlNode node, string namelevel)
	{
		if(gameData.GetComponent<FunctionalityParam>().levelDesign[node.Attributes.GetNamedItem("name").Value])
        {
			// Si la fonctionnalité n'est pas encore connue dans le dictionnaire, on l'ajoute
			if (!gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign.ContainsKey(node.Attributes.GetNamedItem("name").Value))
			{
				gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign.Add(node.Attributes.GetNamedItem("name").Value, new List<string>());
			}
			// On récupére la liste déjà présente
			List<string> listLevelForFuncLevelDesign = gameData.GetComponent<FunctionalityInLevel>().levelByFuncLevelDesign[node.Attributes.GetNamedItem("name").Value];
			listLevelForFuncLevelDesign.Add(namelevel);
		}
        else
        {
			// Si la fonctionnalité n'est pas encore connue dans le dictionnaire, on l'ajoute
			if (!gameData.GetComponent<FunctionalityInLevel>().levelByFunc.ContainsKey(node.Attributes.GetNamedItem("name").Value))
			{
				gameData.GetComponent<FunctionalityInLevel>().levelByFunc.Add(node.Attributes.GetNamedItem("name").Value, new List<string>());
			}
			// On récupére la liste déjà présente
			List<string> listLevelForFunc = gameData.GetComponent<FunctionalityInLevel>().levelByFunc[node.Attributes.GetNamedItem("name").Value];
			listLevelForFunc.Add(namelevel);
		}
	}

	// Désactive les toogles pas encore implémenté
	private void desactiveToogleComp()
	{

		foreach(string nameFunc in gameData.GetComponent<FunctionalityParam>().active.Keys)
        {
			if (!gameData.GetComponent<FunctionalityParam>().active[nameFunc])
			{
				foreach (GameObject comp in competence_f)
				{
					if (comp.GetComponent<Competence>().compLinkWhitFunc.Contains(nameFunc) && comp.GetComponent<Toggle>().interactable)
					{
						comp.GetComponent<Toggle>().interactable = false;
						comp.GetComponent<Competence>().active = false;
					}
				}
			}
        }
	}

	// Permet de selectionné aussi les functionnalité linker avec la fonctionalité selectionné
	private void addSelectFuncLinkbyFunc(string nameFunc)
    {
		foreach(string f_name in gameData.GetComponent<FunctionalityParam>().activeFunc[nameFunc])
        {
			Debug.Log("Func link : " + f_name);
            // Si la fonction na pas encore été selectionné
			// alors on l'ajoute à la séléction et on récurcive dessus
            if (f_name != "" && !gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(f_name))
            {
				Debug.Log("Func link add");
				gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Add(f_name);
				addSelectFuncLinkbyFunc(f_name);
			}
        }
    }

	// Pour certaine compétence il est indispensable que d'autre soit aussi selectionné
	// Cette fonction vérifie que c'est bien le cas avant de lancer la selection de niveau auto
	// Sinon il signale au User qu'elle compétence pause probléme ainsi qu'une compétence minimum qu'il doit cocher parmit la liste proposé
	public void verificationSelectedComp()
    {
		saveListUser();
		bool verif = true;
		List<GameObject> listCompSelect = new List<GameObject>();
		List<string> listNameCompSelect = new List<string>();
		GameObject errorSelectComp = null;

		//On verifi
		foreach (GameObject comp in competence_f)
        {
            // Si la compétence est séléctionné on le note
            if (comp.GetComponent<Toggle>().isOn)
            {
				// Si la compétence demande à avoir une autre comp
				listCompSelect.Add(comp);
				listNameCompSelect.Add(comp.name);
			}
        }

		foreach(GameObject comp in listCompSelect)
        {
			if(comp.GetComponent<Competence>().listSelectMinOneComp[0] != "")
            {
				verif = false;
				foreach (string nameComp in comp.GetComponent<Competence>().listSelectMinOneComp)
                {
                    if (listNameCompSelect.Contains(nameComp))
                    {
						verif = true;
					}
                }
                if (!verif)
                {
					errorSelectComp = comp;
				}
            }
        }

		// Si tous va bien on lance la selection du niveau
        if (verif)
        {
			startLevel();
        }
        else // Sinon on signal au joueur l'erreur
        {
			// Message au User en lui signalant qu'elle competence il doit choisir 
			string message = "Pour la compétence " + errorSelectComp + " Il faut aussi selectionner une de ces compétences :\n";
			foreach(string comp in errorSelectComp.GetComponent<Competence>().listSelectMinOneComp)
            {
				message += comp + " ";
            }
			displayMessageUser(message);
		}
    }

	public void startLevel()
    {
		// On parcourt tous les levels disponible pour les copier dans une liste temporaire
		List<string> copyLevel = new List<string>();
		int nbCompActive = 0;
		bool conditionStartLevelOk = true;

		bool levelLD = false;
		// On regarde si des competence concernant le level design on été selectionné
		foreach (GameObject comp in competence_f)
		{
            if (comp.GetComponent<Toggle>().isOn)
            {
				nbCompActive += 1;
				// On fait ça avec le level design
				foreach (string f_key in gameData.GetComponent<FunctionalityParam>().levelDesign.Keys)
				{
					Debug.Log("Func level design : " + f_key);
                    if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(f_key) && comp.GetComponent<Competence>().compLinkWhitFunc.Contains(f_key))
                    {
						Debug.Log("Func add selection");
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

        // Si aucune compétence n'a été selectionné on ne chargera pas de niveau
        if (nbCompActive <= 0)
        {
			conditionStartLevelOk = false;
		}

        if (conditionStartLevelOk)
        {
			// On signal que la séléction de niveau se fait par les compétences (et donc que les info des fonctionnalité du niveau ne doivent pas être chargé)
			gameData.GetComponent<GameData>().executeLvlByComp = true;
			// 2 cas de figures : 
			// Demande de niveau spécial pour la compétence
			// Demande de niveau sans competence LD
			if (levelLD)
			{
				// On parcourt le dictionnaires des fonctionnalités de level design
				// Si elle fait partie des fonctionnalités selectionné, alors on enregistre les levels associé à la fonctionnalité
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
				// On garde ensuite les niveaux qui contient exclusivement toutes les fonctionalités selectionné
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
				// On supprime de la liste des niveaux possible tous les niveaux appellent des fonctionnalités de level design
				foreach (List<string> levels in gameData.levelList.Values)
				{
					// On créer une copie de la liste des niveaux disponible
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
			string message = "Erreur, pas de compétence sélectionné!";
			displayMessageUser(message);
		}

		// Si on a au moins une compétence activé et un niveau en commun
		// On lance un niveau selectionné aléatoirement parmis la liste des niveaux restant
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
		else // Sinon on signal que aucune compétence n'est selectionné ou que aucun niveau n'est disponible
        {
			string message = "Pas de niveau disponible pour l'ensemble des compétences selectionnées";
			displayMessageUser(message);
		}
	}

	public void infoCompetence(GameObject comp)
	{
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.GetComponent<MenuComp>().info;
		comp.transform.Find("Label").GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

		// Si la compétence enclanche la selection d'autre compétence, on l'afiche dans les infos
		if(comp.GetComponent<Competence>() && comp.GetComponent<Competence>().compLinkWhitComp[0] != "")
        {
			string infoMsg = panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text;
			infoMsg += "\n\nCompetence selectionné automatiquement : \n";
			foreach(string nameComp in comp.GetComponent<Competence>().compLinkWhitComp)
            {
				infoMsg += nameComp + " ";
			}
			panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = infoMsg;
		}

		// Si on survole une category, on change la couleur du bouton
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

	// Lorsque la souris sort de la zone de text de la compétence ou catégorie, on remet le text à son état initial
	public void resetViewInfoCompetence(GameObject comp)
    {
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

		// On parcourt la liste des fonctions a activé pour la compétence
		foreach (string funcNameActive in comp.GetComponent<Competence>().compLinkWhitFunc)
		{
			//Pour chaque fonction on regarde si cela empeche une compétence d'être selectionné
			foreach (string funcNameDesactive in gameData.GetComponent<FunctionalityParam>().enableFunc[funcNameActive])
			{
				// Pour chaque fonction non possible, on regarde les compétence les utilisant pour en désactivé la selection
				foreach (GameObject c in competence_f)
				{
					if (c.GetComponent<Competence>().compLinkWhitFunc.Contains(funcNameDesactive))
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

			foreach(string nameComp in comp.GetComponent<Competence>().compLinkWhitComp)
            {
				foreach(GameObject c in competence_f)
                {
					if(c.name == nameComp)
                    {
						// Les compétences non active sont les compétences dont au moins une des fonctionalités n'est pas encore implémenté
						// Pour éviter tous bug (comme être considérer comme inactive à cause d'une autre compétence séléctionné) on test si la compétence est désactivé par le biais d'un manque de fonction ou non
						if (c.GetComponent<Competence>().active)
						{
							if (c.GetComponent<Toggle>().interactable)
							{
								// Pour éviter les boucles infini, si la compétence est déjà activé, alors la récursive à déjà eu lieu
								if (!c.GetComponent<Toggle>().isOn)
								{
									selectComp(c, false);
								}
							}
							else
							{
								Debug.Log("error");
								error = true;
								break;
							}
						}
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

	//Lors de la deselection d'une compétence on désélectionne toutes les compétences reliées
	public void unselectComp(GameObject comp, bool userUnselect)
    {
        if (!userUnselect)
        {
			comp.GetComponent<Toggle>().isOn = false;
		}

		// On parcourt les compétence, et si la comp en parametre est présente en lien avec une des compétences,
		// On déselectionne aussi cette compétence par récursivité
		foreach(GameObject c in competence_f)
        {
			if(c.GetComponent<Toggle>().interactable && c.GetComponent<Toggle>().isOn && c.GetComponent<Competence>().compLinkWhitComp.Contains(comp.name))
            {
				unselectComp(c, false);
			}
        }

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
			comp.GetComponent<Toggle>().interactable = true;
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