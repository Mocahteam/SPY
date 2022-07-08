using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    private ScrollRect scrollRect;
    private float verticalSpeed;
    private float horizontalSpeed;

    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        verticalSpeed = 0;
        horizontalSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        scrollRect.verticalScrollbar.value += verticalSpeed;
        scrollRect.horizontalScrollbar.value += horizontalSpeed;
    }

    public void setVerticalSpeed(float newSpeed)
    {
        verticalSpeed = newSpeed;
    }

    public void setHorizontalSpeed(float newSpeed)
    {
        horizontalSpeed = newSpeed;
    }
}
