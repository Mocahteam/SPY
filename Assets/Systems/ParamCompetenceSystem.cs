using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Familles
	private Family f_competence = FamilyManager.getFamily(new AllOfComponents(typeof(Competence))); // Les Toogles compétences
	private Family f_menuElement = FamilyManager.getFamily(new AnyOfComponents(typeof(Competence), typeof(Category))); // Les Toogles compétences et les Catégories qui les réunnissent en groupes
	private Family f_category = FamilyManager.getFamily(new AllOfComponents(typeof(Category))); // Les categories qui contiendront des sous categories ou des compétences

	// Variables
	public GameObject panelSelectComp; // Panneau de selection des compétences
	public GameObject panelInfoComp; // Panneau d'information des compétences
	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, conflit dans la selection des compétences etc...)
	public GameObject scrollViewComp; // Le contrôleur du scroll pour les compétences
	public string pathParamComp = "/StreamingAssets/ParamCompFunc/ParamCompetence.csv"; // Chemin d'acces du fichier CSV contenant les info des competences à charger 
	public GameObject prefabCateComp; // Prefab de l'affichage d'une catégorie de compétence
	public GameObject prefabComp; // Prefab de l'affichage d'une compétence
	public GameObject ContentCompMenu; // Panneau qui contient la liste des catégories et compétences
	public TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressés à l'utilisateur

	private GameData gameData;
	private List<string> listCompSelectUser = new List<string>(); // Enregistre temporairement les compétences séléctionnées par le user
	private List<string> listCompSelectUserSave = new List<string>(); // Contient les compétences selectionnées par le user

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}

	// used on TitleScreen scene
	public void openPanelSelectComp()
	{
		try
		{
			// Note pour chaque fonction les niveaux ou elles sont présentes
			readXMLinfo();
		}
		catch
		{
			string message = "Erreur chargement fichiers de niveaux!\n";
			message += "Vérifier que les fichiers existent ou sont bien au format XML";
			displayMessageUser(message);
			// Permetra de fermer le panel de selection des competences lorsque le user appuie sur le bouton ok du message d'erreur
			panelSelectComp.GetComponent<ParamCompetenceSystemBridge>().closePanelParamComp = true;
		}

		try
		{
			// On charge les données pour chaque compétence
			loadParamComp();
			MainLoop.instance.StartCoroutine(startAfterFamillyOk());
			// On démarre la coroutine pour attacher chaque compétence et sous-categorie et leur catégorie
			MainLoop.instance.StartCoroutine(attacheComptWithCat());
		}
		catch
		{
			string message = "Erreur chargement fichier de parametrage des compétences!\n";
			message += "Vérifié que le fichier csv et les informations contenues sont au bon format";
			displayMessageUser(message);
			// Permettra de fermer le panel de selection des compétences lorsque le user appuie sur le bouton ok du message d'erreur
			panelSelectComp.GetComponent<ParamCompetenceSystemBridge>().closePanelParamComp = true;
		}
	}

	private IEnumerator noSelect(GameObject comp)
    {
		yield return null;

		listCompSelectUser = new List<string>(listCompSelectUserSave);
		resetSelectComp();
		desactiveToogleComp();
		foreach (string level in listCompSelectUserSave)
        {
			foreach(GameObject c in f_competence)
            {
				if(c.name == level)
                {
					selectComp(c, false);
				}
			}
		}
		MainLoop.instance.StopCoroutine(noSelect(comp));
	}

	// Permet d'attacher à chaque catégorie les sous-categories et compétences qui la compose
	private IEnumerator attacheComptWithCat()
    {
		yield return null;

		foreach (GameObject cat in f_category)
		{
			foreach(GameObject element in f_menuElement)
            {
				if (element.GetComponent<MenuComp>().catParent == cat.name)
				{
					cat.GetComponent<Category>().listAttachedElement.Add(element.name);
				}
			}
		}

		MainLoop.instance.StopCoroutine(attacheComptWithCat());
	}

	// Permet de lancer les différentes fonctions que l'on a besoin pour le démarrage APRES que les familles soient mises à jour
	private IEnumerator startAfterFamillyOk() {
		yield return null;

		// On désactive les compétences pas encore implémentées
		desactiveToogleComp();
		// On décale les sous-catégories et compétences selon leur place dans la hierarchie
		displayCatAndComp();

		MainLoop.instance.StopCoroutine(startAfterFamillyOk());
	}

	// Chargement des parametres des compétences
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

	// Instancie et paramètre la compétence à afficher
	private void createCatObject(string[] data)
    {
		// On instancie la catégorie
		GameObject category = UnityEngine.Object.Instantiate(prefabCateComp);
		// On l'attache au content
		category.transform.SetParent(ContentCompMenu.transform);
		// On signale à quelle catégorie la compétence appartient
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

	// Instancie et paramètre la sous-compétence à afficher
	private void createCompObject(string[] data)
	{
		// On instancie la catégorie
		GameObject competence = UnityEngine.Object.Instantiate(prefabComp);
		// On signale à quel catégorie la compétence appartient
		competence.GetComponent<Competence>().catParent = data[1];
		// On l'attache au content
		competence.transform.SetParent(ContentCompMenu.transform);
		competence.name = data[2];
		// On charge le text de la compétence
		competence.transform.Find("Label").GetComponent<TMP_Text>().text = data[3];
		competence.transform.Find("Label").GetComponent<TMP_Text>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
		// On charge les info de la compétence qui sera affichée lorsque l'on survolera celle-ci avec la souris
		competence.GetComponent<Competence>().info = data[4];
		// On charge le vecteur des Fonctions liées à la compétence
		if (data.Length >= 6)
        {
			var data_link = data[5].Split(',');
			foreach (string value in data_link)
			{
				competence.GetComponent<Competence>().compLinkWhitFunc.Add(value);
			}
			if (data.Length >= 7)
			{
				// On charge le vecteur des compétences qui seront automatiquement selectionnées si la compétence est séléctionnée
				data_link = data[6].Split(',');
				foreach (string value in data_link)
				{
					competence.GetComponent<Competence>().compLinkWhitComp.Add(value);
				}
				if (data.Length >= 8)
				{
					// On charge le vecteur des compétences dont au moins l'une devra être selectionnée en même temps que celle selectionnée actuellement
					data_link = data[7].Split(',');
					foreach (string value in data_link)
					{
						competence.GetComponent<Competence>().listSelectMinOneComp.Add(value);
					}
				}
			}
		}

		GameObjectManager.bind(competence);
	}

	// Mise en place: décaller les sous-categories et compétences
	private void displayCatAndComp()
    {
		foreach(GameObject element in f_menuElement) { 
			// Si l'élément a un parent
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

	// Fonction pouvant être appellée par récursivité
	// Permet de renvoyer à quelle profondeur dans la hiérarchie Categorie de la selection des compétences l'élément se trouve
	private int nbParentInHierarchiComp(GameObject element)
    {
		int nbParent = 1;

		foreach (GameObject ele in f_menuElement){ 
			if(ele.name == element.GetComponent<MenuComp>().catParent && ele.GetComponent<MenuComp>().catParent != "")
            {
				nbParent += nbParentInHierarchiComp(ele);
			}
		}
			return nbParent;
    }

	// Lit tous les fichiers XML des niveaux de chaque dossier afin de charger quelle fonctionalité se trouve dans quel niveau  
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

	// Parcourt le noeud d'information et appelle les bonnes fonctions pour traiter l'information du niveau
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
							case "func":
								addInfo(infoNode, namelevel);
								break;
						}
					}
					break;
			}
		}
	}

	// Associe à chaque fonctionalité renseignée sa présence dans le niveau
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

	// Désactive les toggles pas encore implémentés
	private void desactiveToogleComp()
	{

		foreach(string nameFunc in gameData.GetComponent<FunctionalityParam>().active.Keys)
        {
			if (!gameData.GetComponent<FunctionalityParam>().active[nameFunc])
			{
				foreach (GameObject comp in f_competence)
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

	// Permet de selectionné aussi les functionnalités linker avec la fonctionalité selectionnée
	private void addSelectFuncLinkbyFunc(string nameFunc)
    {
		foreach(string f_name in gameData.GetComponent<FunctionalityParam>().activeFunc[nameFunc])
        {
            // Si la fonction na pas encore été selectionnée
			// alors on l'ajoute à la séléction et on fait un appel récursif dessus
            if (f_name != "" && !gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(f_name))
            {
				gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Add(f_name);
				addSelectFuncLinkbyFunc(f_name);
			}
        }
    }

	// Pour certaines compétences il est indispensable que d'autres soient aussi selectionnées
	// Cette fonction vérifie que c'est bien le cas avant de lancer la selection de niveau auto
	// Sinon il signale au User quelle compétence pose problème ainsi qu'une compétence minimum qu'il doit cocher parmis la liste proposée
	public void verificationSelectedComp()
    {
		saveListUser();
		bool verif = true;
		List<GameObject> listCompSelect = new List<GameObject>();
		List<string> listNameCompSelect = new List<string>();
		GameObject errorSelectComp = null;

		//On verifie
		foreach (GameObject comp in f_competence)
        {
            // Si la compétence est séléctionnée on le note
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

		// Si tout va bien on lance la sélection du niveau
        if (verif)
        {
			startLevel();
        }
        else // Sinon on signale au joueur l'erreur
        {
			// Message au User en lui signalant quelle competence il doit choisir 
			string message = "Pour la compétence " + errorSelectComp + " Il faut aussi selectionner une de ces compétences :\n";
			foreach(string comp in errorSelectComp.GetComponent<Competence>().listSelectMinOneComp)
            {
				message += comp + " ";
            }
			displayMessageUser(message);
		}
    }

	// Use in ButtonStartLevel in ParamCompPanel prefab
	public void startLevel()
    {
		// On parcourt tous les levels disponibles pour les copier dans une liste temporaire
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
		}
	}

	// Used when PointerOver CategorizeCompetence prefab (see in editor)
	public void infoCompetence(GameObject comp)
	{
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.GetComponent<MenuComp>().info;
		comp.transform.Find("Label").GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

		// Si la compétence enclanche la sélection d'autre compétence, on l'affiche dans les infos
		if(comp.GetComponent<Competence>() && comp.GetComponent<Competence>().compLinkWhitComp[0] != "")
        {
			string infoMsg = panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text;
			infoMsg += "\n\nCompetence selectionnée automatiquement : \n";
			foreach(string nameComp in comp.GetComponent<Competence>().compLinkWhitComp)
            {
				infoMsg += nameComp + " ";
			}
			panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = infoMsg;
		}

		// Si on survole une categorie, on change la couleur du bouton
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
	// On desactive toutes les compétences non implémentées et les compétences ne pouvant plus être selectionnées
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

		// On parcourt la liste des fonctions à activer pour la compétence
		foreach (string funcNameActive in comp.GetComponent<Competence>().compLinkWhitFunc)
		{
			//Pour chaque fonction on regarde si cela empêche une compétence d'être selectionnée
			foreach (string funcNameDesactive in gameData.GetComponent<FunctionalityParam>().enableFunc[funcNameActive])
			{
				// Pour chaque fonction non possible, on regarde les compétences les utilisant pour en désactiver la selection
				foreach (GameObject c in f_competence)
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
				foreach(GameObject c in f_competence)
                {
					if(c.name == nameComp)
                    {
						// Les compétences non active sont les compétences dont au moins une des fonctionalités n'est pas encore implémentée
						// Pour éviter tout bug (comme être considéré comme inactive à cause d'une autre compétence séléctionnée) on teste si la compétence est désactivée par le biais d'un manque de fonction ou non
						if (c.GetComponent<Competence>().active)
						{
							if (c.GetComponent<Toggle>().interactable)
							{
								// Pour éviter les boucles infinies, si la compétence est déjà activée, alors la récursive a déjà eu lieu
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

	//Lors de la desélection d'une compétence on desélectionne toutes les compétences reliées
	public void unselectComp(GameObject comp, bool userUnselect)
    {
		// On retire la compétence de la liste des compétences sélectionnées
		addOrRemoveCompSelect(comp, false);

		// On reset l'affichage de toutes les compétences.
		resetSelectComp();

		// Le toggle va être désactivé automatiquement par le programme aprés le traitement de la fonction 
		if (userUnselect)
		{
			comp.GetComponent<Toggle>().isOn = true;
		}

		//On désactive tous les toggle des comp pas implémentés
		desactiveToogleComp();

		// On resélectionne toutes les compétences
		foreach (string compName in listCompSelectUser)
		{
			foreach (GameObject c in f_competence)
			{
				if (c.name == compName)
				{
					selectComp(c, false);
				}
			}
		}

	}

	// Ajoute ou retire la compétence de la liste des compétences selectionnées manuellement par l'utilisateur
	public void addOrRemoveCompSelect(GameObject comp, bool value)
	{
        if (value)
        {
			// Si la compétence n'est pas encore notée comme avoir été selectionnée par le user
            if (!listCompSelectUser.Contains(comp.name))
            {
				listCompSelectUser.Add(comp.name);
			}
		}
        else
        {
			// Si la compétence avait été séléctionnée par le user
			if(listCompSelectUser.Contains(comp.name)){
				listCompSelectUser.Remove(comp.name);
			}
		}
	}

	// Reset toutes les compétences en "non selectionnée"
	private void resetSelectComp()
    {
		foreach (GameObject comp in f_competence)
		{
			comp.GetComponent<Toggle>().isOn = false;
			comp.GetComponent<Toggle>().interactable = true;
		}
	}

	// Enregistre la liste des compétences sélectionnées par le user
	public void saveListUser()
    {
		listCompSelectUserSave = new List<string>(listCompSelectUser);
	}

	// Ferme le panel de sélection des compétences
	// Décoche toutes les compétences cochées
	// vide les listes de suivis des compétences selectionnées
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

	// Cache ou montre les éléments associés à la catégorie
	public void viewOrHideCompList(GameObject category)
    {
		category.GetComponent<Category>().hideList = !category.GetComponent<Category>().hideList;

		foreach (GameObject element in f_menuElement)
        {
            if (category.GetComponent<Category>().listAttachedElement.Contains(element.name))
            {
				element.SetActive(!category.GetComponent<Category>().hideList);
            }
        }
	}

	// Active ou désactive la bouton
	// Cette fonction est réservée à la gestion du bouton à afficher à coté de la catégorie si jamais le user appuie sur le text pour faire apparaitre ou disparaitre la liste associée
	public void hideOrShowButtonCategory(GameObject button)
    {
		button.SetActive(!button.activeSelf);
	}
}