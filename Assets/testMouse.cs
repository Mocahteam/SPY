using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class testMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData e){
        Debug.Log("OnPointerEnter "+this.gameObject.name);
    }

    public void OnPointerExit(PointerEventData e){
        Debug.Log("OnPointerExit"+this.gameObject.name);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
