using System;
using System.Collections.Generic;

[Serializable]
public class Dialog
{
	public string text = null;
	public string img = null;
	public float imgHeight = -1;
	public int camX = -1;
	public int camY = -1;
	public string sound = null;
	public string video = null;
	public bool enableInteraction = false;
}

[Serializable]
public class DataLevel
{
	public string src;
	public string name;
	public List<Dialog> overridedDialogs = null;
}