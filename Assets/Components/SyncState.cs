using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncState : MonoBehaviour
{
    public void inverseState(bool newState){
        this.gameObject.SetActive(!newState);
    }
}
