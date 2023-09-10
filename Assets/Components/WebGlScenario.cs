using System;
using System.Collections.Generic;

[Serializable]
public class WebGlScenario
{
	public string key; // The name of the file without extension
	public string name; // The name of the scenario
	public string description; // the description of the scenario
	public List<DataLevel> levels; // the list of levels included in this scenario
}