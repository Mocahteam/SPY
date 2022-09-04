using UnityEngine;
using System.Collections.Generic;

public class AddSpecificContainer : MonoBehaviour {
	public string name = "";
	public AgentEdit.EditMode editState = AgentEdit.EditMode.Editable;
	public List<GameObject> script = null;
}