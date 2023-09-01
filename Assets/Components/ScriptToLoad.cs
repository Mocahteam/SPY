using UnityEngine;
using System.Collections.Generic;
using System.Xml;

public class ScriptToLoad : MonoBehaviour {
	public XmlNode scriptNode;
	public string scriptName;
	public UIRootContainer.EditMode editMode;
	public UIRootContainer.SolutionType type;
}