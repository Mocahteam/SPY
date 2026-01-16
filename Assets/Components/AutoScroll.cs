using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class AutoScroll : MonoBehaviour
{
    public RectTransform autoScrollUp;
    public RectTransform autoScrollDown;
    public RectTransform autoScrollLeft;
    public RectTransform autoScrollRight;
    [HideInInspector]
    public float verticalSpeed = 0;
    [HideInInspector]
    public float horizontalSpeed = 0;
}
