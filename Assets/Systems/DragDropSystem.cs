using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// beginDragElementFromLibrary
///		Pour le début du drag and drop d'un élément venant de la librairie
/// beginDragElementFromEditableScript
///		Pour le début du drag and drop d'un élément venant de la séquence d'action en construction
/// dragElement
///		Pendant le drag d'un élément
/// endDragElement
///		A la fin d'un drag and drop si l'élément n'est pas laché dans un container pour la création d'une séquence
/// dropElementInContainer
///		A la fin d'un drag and drop si l'élément est laché dans un container pour la création d'une séquence
/// creationActionBlock
///		Création d'un block d'action lors de la selection de l'element correspondant dans la librairie
/// deleteElement
///		Destruction d'une block d'action
/// clickLibraryElementForAddInContainer
///		Ajout d'une action dans la derniére sequence d'action modifié, correspondant à l'élément ayant reçue un double click.
/// tcheckDoubleClick
///		Regarde si un double click à eu lieu sur l'élément auquel il est rataché
/// </summary>

public class DragDropSystem : FSystem
{
	// Les familles
    private Family viewportContainerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les container éditable
	private Family actionPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UIActionType), typeof(Image)));  // Les block d'actions pointer
	private Family scriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer))); // Les containers scripts
	private Family dropZone_f = FamilyManager.getFamily(new AllOfComponents(typeof(DropZoneComponent))); // Les drops zones
	private Family containerActionBloc_f = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(ContainerActionBloc))); // Les blocs qui ne sont pas de base

	// Les variables
	private GameObject itemDragged; // L'item (ici block d'action) en cours de drag
	public GameObject mainCanvas; // Le canvas principal
	public GameObject lastEditableContainer; // Le dernier container édité 
	public AudioSource audioSource; // Pour le son d'ajout de block
	public GameObject buttonPlay;
	//Pour la gestion du double click
	private float lastClickTime;
	public float catchTime;

	// L'instance
	public static DragDropSystem instance;

	public DragDropSystem()
    {
		instance = this;
		

	}


	// Besoin d'attendre l'update pour effectuer le recalcule de la taille des container
	private IEnumerator waitForResizeContainer(GameObject bloc)
	{
		yield return null;
		resizeContainerActionBloc(bloc);
	}


	// On active toutes les drop zone
	private void dropZoneActivated(bool value)
	{
		foreach (GameObject Dp in dropZone_f)
		{
			Dp.SetActive(value);
			Dp.transform.GetChild(0).gameObject.SetActive(false); // On est sur que les bar sont désactivé
		}

		// Si l'item drag est un container d'un bloc d'action
		// On désactive ses blocs
		if (itemDragged!= null && itemDragged.GetComponent<UIActionType>().container)
		{
			dropZoneContainerDragDesactived(itemDragged);
		}
	}


	// Annule l'activation des drops zones du container en cours de drag and drop
	private void dropZoneContainerDragDesactived(GameObject container)
    {
		// On désactive la drop zone du parent
		container.transform.Find("Image").Find("DropZone").gameObject.SetActive(false);
		foreach (Transform childBloc in container.transform.Find("Container").transform)
        {
			childBloc.Find("DropZone").gameObject.SetActive(false);
        }
    }


	// Lors de la selection (début d'un drag) d'un block de la librairie
	// Crée un game object action = à l'action selectionné dans librairie pour ensuite pouvoir le manipuler (durant le drag et le drop)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On verifie si c'est un up droit ou gauche
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left)
		{
			// On créer le block action associé à l'élément
			creationActionBlock(element.selectedObject);
		}

		// On active les drops zone 
		dropZoneActivated(true);
	}


	// Lors de la selection (début d'un drag) d'un block de la sequence
	// l'enélve de la hiérarchie de la sequence d'action 
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On verifie si c'est un up droit ou gauche
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left)
		{
			// On note le container utilisé
			lastEditableContainer = element.selectedObject.transform.parent.gameObject;
			// On enregistre l'objet sur lequel on va travailler le drag and drop dans le systéme
			itemDragged = element.selectedObject;
			// On active les drops zone 
			dropZoneActivated(true);
			// On l'associe (temporairement) au Canvas Main
			GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
			itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			// exclude this GameObject from the EventSystem
			itemDragged.GetComponent<Image>().raycastTarget = false;
			//if (itemDragged.GetComponent<BasicAction>())
			foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				child.raycastTarget = false;
			// Restore action and subactions to inventory
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
				GameObjectManager.addComponent<AddOne>(actChild.gameObject);
			//lastEditableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = false;

			// Rend le bouton d'execution acitf (ou non)
			UISystem.instance.startUpdatePlayButton();
		}
	}


	// Pendant le drag d'un block, permet de lui faire suivre le mouvement de la souris
	public void dragElement()
	{
		if(itemDragged != null) {
			itemDragged.transform.position = Input.mousePosition;
		}
	}


	// Determine si l'element associer à l'évenement Pointer Up se trouvé dans une zone de container ou non
	// Détruite l'objet si pas dans un container, sinon rien
	public void endDragElement()
	{
		if (itemDragged != null)
		{
			// On commence par regarder si il y a un container pointé et sinon on supprime l'objet drag
			if (viewportContainerPointed_f.Count <= 0)
			{
				// remove item and all its children
				for (int i = 0; i < itemDragged.transform.childCount; i++)
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				itemDragged.transform.DetachChildren();
				// Suppresion des famille de FYFY
				GameObjectManager.unbind(itemDragged);
				// Déstruction du block
				UnityEngine.Object.Destroy(itemDragged);

				// Rafraichissement de l'UI
				UISystem.instance.refreshUIButton();
				// Suppression de l'item stocker en donnée systéme
				itemDragged = null;
				//lastEditableContainer.transform.parent.parent.GetComponent<ScrollRect>().enabled = true;
				// On désactive les drop zone
				dropZoneActivated(false);
			}
            else // sinon on ajoute l'élément au container pointé
            {
				GameObject container = viewportContainerPointed_f.First().transform.Find("ScriptContainer").gameObject;
				// On récupére qu'elle container est pointer
				// Et on ajouter l'action à la fin du container éditable
				dropElementInContainer(container.transform.Find("EndZoneActionBloc").Find("DropZone").gameObject);
			}
		}
	}


	// Place l'element dans la place ciblé (position de l'element associer au radar) du container editable
	public void dropElementInContainer(GameObject redBar)
	{
		// On note le container utilisé
		lastEditableContainer = redBar.transform.parent.parent.gameObject;

		if (itemDragged != null)
		{
			if (lastEditableContainer.GetComponent<UITypeContainer>().notScriptContainer)
			{
				// On associe l'element au container
				itemDragged.transform.SetParent(lastEditableContainer.transform.parent.transform);
				// On met l'élément à la position voulue
				itemDragged.transform.SetSiblingIndex(redBar.transform.parent.parent.transform.GetSiblingIndex());
				GameObjectManager.refresh(itemDragged);
			}
            else
            {
				// On associe l'element au container
				itemDragged.transform.SetParent(lastEditableContainer.transform);
				// On met l'élément à la position voulue
				itemDragged.transform.SetSiblingIndex(redBar.transform.parent.transform.GetSiblingIndex()); 
				GameObjectManager.refresh(itemDragged);

			}
			// On le met à la taille voulue
			itemDragged.transform.localScale = new Vector3(1, 1, 1);
			// Pour réactivé la selection posible
			itemDragged.GetComponent<Image>().raycastTarget = true;
			//if (itemDragged.GetComponent<BasicAction>())
			//{
			foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				child.raycastTarget = true;
			//}

			// update limit bloc
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
			{
				GameObjectManager.addComponent<Dropped>(actChild.gameObject);
			}

			if (itemDragged.GetComponent<UITypeContainer>())
            {
				itemDragged.GetComponent<Image>().raycastTarget = true;
			}

			// Lance le son de dépôt du block d'action
			audioSource.Play();

			UISystem.instance.startUpdatePlayButton();
			itemDragged = null;
			mainCanvas.transform.Find("EditableCanvas").GetComponent<ScrollRect>().enabled = true;
			UISystem.instance.refreshUIButton();

			// On désactive les drop zone
			dropZoneActivated(false);

			// Si le container est un container d'un bloc d'action
			if (lastEditableContainer.GetComponent<UITypeContainer>().actionContainer)
            {
				resizeContainerActionBloc(lastEditableContainer.transform.parent.gameObject);
            }
		}
	}


	// Recréer la taille du contenaire (action) selon le nombre d'élément qu'il contient
	private void resizeContainerActionBloc(GameObject bloc)
    {
		GameObject parentBloc = bloc.transform.parent.gameObject;
		GameObject copy = UnityEngine.Object.Instantiate<GameObject>(bloc);
		GameObjectManager.bind(copy);
		// On associe l'element au container
		copy.transform.SetParent(parentBloc.transform);
		// On met l'élément à la position voulue
		copy.transform.SetSiblingIndex(bloc.transform.GetSiblingIndex());
		GameObjectManager.unbind(bloc);
		// Déstruction du block
		UnityEngine.Object.Destroy(bloc);
		// On oublie pas d'associer le nouveau container créer en tant que dernier container
		lastEditableContainer = copy.transform.Find("Container").gameObject;
	}


	// On créer l'action block en fonction de l'element reçu
	private void creationActionBlock(GameObject element)
    {
		// On récupére le pref fab associé à l'action de la libriaire
		GameObject prefab = element.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		itemDragged = UnityEngine.Object.Instantiate<GameObject>(prefab, element.transform);
		BaseElement action = itemDragged.GetComponent<BaseElement>();
		itemDragged.GetComponent<UIActionType>().linkedTo = element;
		// On l'ajoute au famille de FYFY
		GameObjectManager.bind(itemDragged);
		// exclude this GameObject from the EventSystem
		itemDragged.GetComponent<Image>().raycastTarget = false;
		//if (itemDragged.GetComponent<BasicAction>())
		foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
			child.raycastTarget = false;
	}


	// Supprime l'elementd
	public void deleteElement(GameObject element)
	{
		// On note l'action pointé
		GameObject actionPointed = null;
		if(actionPointed_f.Count > 0)
        {
			actionPointed = actionPointed_f.getAt(actionPointed_f.Count - 1); actionPointed_f.getAt(actionPointed_f.Count - 1);
			Debug.Log(actionPointed.name);
		}

		// On vérifie qu'il y à bien un objet pointé pour la suppression
		if(actionPointed != null)
        {
			GameObjectManager.addComponent<ResetBlocLimit>(actionPointed);
			UISystem.instance.startUpdatePlayButton();

			if (!actionPointed.GetComponent<UIActionType>().container)
			{
				foreach (GameObject bloc in containerActionBloc_f)
				{
					MainLoop.instance.StartCoroutine(waitForResizeContainer(bloc));
				}
			}
		}
	}


	// Si double click sur l'élément, ajoute le block d'action au dernier container utilisé
	public void clickLibraryElementForAddInContainer(BaseEventData element)
    {
		if (tcheckDoubleClick())
		{
			// On créer le block action
			creationActionBlock(element.selectedObject);
			// On l'envoie vers l'editable container
			dropElementInContainer(lastEditableContainer.transform.Find("EndZoneActionBloc").Find("DropZone").gameObject);
		}
	}


	// Vérifie si le double click à eu lieu
	private bool tcheckDoubleClick()
	{
		//check double click
		// On met à jours le timer du dernier clique
		// et on retourne la réponse
		if (Time.time - lastClickTime < catchTime)
        {
			lastClickTime = Time.time;
			return true;
		}
        else
        {
			lastClickTime = Time.time;
			return false;
		}

	}

}