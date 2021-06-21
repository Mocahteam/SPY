using UnityEngine;
using UnityEngine.EventSystems;

public class ForAction : BaseElement, IPointerExitHandler, IPointerEnterHandler {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public int currentFor;
    public int nbFor;
    public GameObject firstChild;

    public void OnPointerExit(PointerEventData e){
        Debug.Log("OnPointerExit");
    }

    public void OnPointerEnter(PointerEventData e){
        Debug.Log("OnPointerEnter");
    }

    private void OnMouseEnter(){
        Debug.Log("OnMouseEnter");
    }
    
    private void OnMouseExit(){
        Debug.Log("OnMouseExit");
    }
}