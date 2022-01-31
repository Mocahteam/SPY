using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class LevelGeneratorSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void XmlToLevel(System.String fileName)
	{
		MainLoop.callAppropriateSystemMethod (null, "XmlToLevel", fileName);
	}

}
