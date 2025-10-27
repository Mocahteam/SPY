using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using UnityEngine;

public static class UtilityLobby
{
	public static string testFromScenarioEditor = "testFromScenarioEditor";
	public static string testFromLevelEditor = "testFromLevelEditor";
	public static string testFromUrl = "testFromUrl";
	public static string editingScenario = "editingScenario";

	public static void LoadLevelOrScenario(GameData gameData, string uri, string xmlContent)
	{
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(xmlContent);
		Utility.removeComments(doc);
		// a valid level must have only one tag "level" and no tag "scenario"
		if (doc.GetElementsByTagName("level").Count == 1 && doc.GetElementsByTagName("scenario").Count == 0)
			gameData.levels[new Uri(uri).AbsoluteUri] = doc.GetElementsByTagName("level")[0];
		// a valid scenario must have only one tag "scenario"
		else if (doc.GetElementsByTagName("scenario").Count == 1)
			updateScenarioContent(gameData, uri, doc);
		else
			throw new Exception("\"" + uri + "\"" + gameData.GetComponent<Localization>().localization[21]);
	}

	public static void updateScenarioContent(GameData gameData, string uri, XmlDocument doc)
	{
		WebGlScenario scenario = new WebGlScenario();
		scenario.key = Path.GetFileNameWithoutExtension(uri);
		XmlNode xmlScenario = doc.GetElementsByTagName("scenario")[0];
		if (xmlScenario.Attributes.GetNamedItem("name") != null)
			scenario.name = xmlScenario.Attributes.GetNamedItem("name").Value;
		else
			scenario.name = scenario.key;
		if (xmlScenario.Attributes.GetNamedItem("desc") != null)
			scenario.description = xmlScenario.Attributes.GetNamedItem("desc").Value;
		else
			scenario.description = "";
		scenario.levels = new List<DataLevel>();
		foreach (XmlNode child in doc.GetElementsByTagName("scenario")[0])
			if (child.Name.Equals("level"))
			{
				DataLevel dl = new DataLevel();
				// get src
				dl.src = new Uri(Application.streamingAssetsPath + "/" + (child.Attributes.GetNamedItem("src").Value)).AbsoluteUri;

				// get name
				if (child.Attributes.GetNamedItem("name") != null)
					dl.name = child.Attributes.GetNamedItem("name").Value;
				else
					// if not def, use file name
					dl.name = Path.GetFileNameWithoutExtension(dl.src);

				// load overrided dialogs
				foreach (XmlNode subChild in child.ChildNodes)
					if (subChild.Name.Equals("dialogs"))
					{
						dl.overridedDialogs = new List<Dialog>();
						Utility.readXMLDialogs(subChild, dl.overridedDialogs);
						break;
					}

				scenario.levels.Add(dl);
			}
		gameData.scenarios[scenario.key] = scenario;
	}

	public static bool isCompetencyMatchWithLevel(Competency competency, XmlDocument level)
	{
		// check all filters of the competency
		Dictionary<string, List<XmlNode>> filtersState = new Dictionary<string, List<XmlNode>>();
		foreach (RawFilter filter in competency.filters)
		{

			if (filtersState.ContainsKey(filter.label))
			{
				// if a filter with this label is defined and no XmlNode identified, useless to check this new one
				if (filtersState[filter.label].Count == 0)
					continue;
			}
			else
			{
				// init this filter with all XmlNode of required tag
				List<XmlNode> tagList = new List<XmlNode>();
				foreach (XmlNode tag in level.GetElementsByTagName(filter.tag))
					tagList.Add(tag);
				filtersState.Add(filter.label, tagList);
			}

			// check if this filter is true
			List<XmlNode> tags = filtersState[filter.label];
			foreach (RawConstraint constraint in filter.constraints)
			{
				int levelAttrValue;
				switch (constraint.constraint)
				{
					// Check if the value of an attribute of the tag is equal to a given value
					case "=":
						for (int t = tags.Count - 1; t >= 0; t--)
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null || tags[t].Attributes.GetNamedItem(constraint.attribute).Value != constraint.value)
								tags.RemoveAt(t);
						break;
					// Check if the value of an attribute of the tag is not equal to a given value
					case "<>":
						for (int t = tags.Count - 1; t >= 0; t--)
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null || tags[t].Attributes.GetNamedItem(constraint.attribute).Value == constraint.value)
								tags.RemoveAt(t);
						break;
					// Check if the value of an attribute of the tag is greater than a given value (for limit attribute consider -1 as infinite value)
					case ">":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue <= int.Parse(constraint.value) && (constraint.attribute != "limit" || levelAttrValue != -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is smaller than a given value (for limit attribute consider -1 as infinite value)
					case "<":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue >= int.Parse(constraint.value) || (constraint.attribute == "limit" && levelAttrValue == -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is greater than or equal a given value (for limit attribute consider -1 as infinite value)
					case ">=":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue < int.Parse(constraint.value) && (constraint.attribute != "limit" || levelAttrValue != -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is smaller than or equal a given value (for limit attribute consider -1 as infinite value)
					case "<=":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue > int.Parse(constraint.value) || (constraint.attribute == "limit" && levelAttrValue == -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the attribute of the tag is included inside a given value
					case "isIncludedIn":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null || !constraint.value.Contains(tags[t].Attributes.GetNamedItem(constraint.attribute).Value))
								tags.RemoveAt(t);
						}
						break;
					// Check if the value of an attribute of a tag is equal to the value of an attribute of another tag
					case "sameValue":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								bool found = false;
								foreach (XmlNode node in tags[t].OwnerDocument.GetElementsByTagName(constraint.tag2))
								{
									if (node != tags[t] && node.Attributes.GetNamedItem(constraint.attribute2) != null && node.Attributes.GetNamedItem(constraint.attribute2).Value == tags[t].Attributes.GetNamedItem(constraint.attribute).Value)
									{
										found = true;
										break;
									}
								}
								if (!found)
									tags.RemoveAt(t);
							}
						}
						break;
					// Check if a tag contains at least one child
					case "hasChild":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (!tags[t].HasChildNodes)
								tags.RemoveAt(t);
						}
						break;
				}
			}
		}
		// check the rule (combination of filters)
		string rule = competency.rule;
		foreach (string key in filtersState.Keys)
		{
			rule = rule.Replace(key, "" + filtersState[key].Count);
		}
		DataTable dt = new DataTable();
		if (rule != "")
			return (bool)dt.Compute(rule, "");
		else
			return false;
	}
}