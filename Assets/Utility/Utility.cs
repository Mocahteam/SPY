using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;

public static class Utility
{
	public static void removeComments(XmlNode node)
	{
		for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
		{
			XmlNode child = node.ChildNodes[i];
			if (child.NodeType == XmlNodeType.Comment)
				node.RemoveChild(child);
			else
				removeComments(child);
		}
	}

	public static void readXMLDialogs(XmlNode dialogs, List<Dialog> target)
	{
		foreach (XmlNode dialogXML in dialogs.ChildNodes)
		{
			Dialog dialog = new Dialog();
			if (dialogXML.Attributes.GetNamedItem("text") != null)
				dialog.text = dialogXML.Attributes.GetNamedItem("text").Value;
			if (dialogXML.Attributes.GetNamedItem("img") != null)
				dialog.img = dialogXML.Attributes.GetNamedItem("img").Value;
			if (dialogXML.Attributes.GetNamedItem("imgDesc") != null)
				dialog.imgDesc = dialogXML.Attributes.GetNamedItem("imgDesc").Value;
			if (dialogXML.Attributes.GetNamedItem("imgHeight") != null)
				dialog.imgHeight = float.Parse(dialogXML.Attributes.GetNamedItem("imgHeight").Value);
			if (dialogXML.Attributes.GetNamedItem("camX") != null)
				dialog.camX = int.Parse(dialogXML.Attributes.GetNamedItem("camX").Value);
			if (dialogXML.Attributes.GetNamedItem("camY") != null)
				dialog.camY = int.Parse(dialogXML.Attributes.GetNamedItem("camY").Value);
			if (dialogXML.Attributes.GetNamedItem("sound") != null)
				dialog.sound = dialogXML.Attributes.GetNamedItem("sound").Value;
			if (dialogXML.Attributes.GetNamedItem("video") != null)
				dialog.video = dialogXML.Attributes.GetNamedItem("video").Value;
			if (dialogXML.Attributes.GetNamedItem("enableInteraction") != null)
				dialog.enableInteraction = int.Parse(dialogXML.Attributes.GetNamedItem("enableInteraction").Value) == 1;
			if (dialogXML.Attributes.GetNamedItem("briefingType") != null)
				dialog.briefingType = int.Parse(dialogXML.Attributes.GetNamedItem("briefingType").Value);
			target.Add(dialog);
		}
	}

	// used for localization process to integrate inside expression some data
	public static string getFormatedText(string expression, params object[] data)
	{
		for (int i = 0; i < data.Length; i++)
			expression = expression.Replace("#" + i + "#", data[i].ToString());
		return expression;
	}

	public static string extractLocale(string content)
	{
		if (content == null) return "";
		string localKey = LocalizationSettings.Instance.GetSelectedLocale().Identifier.Code;
		if (content.Contains("[" + localKey + "]") && content.Contains("[/" + localKey + "]"))
		{
			int start = content.IndexOf("[" + localKey + "]") + localKey.Length + 2;
			int length = content.IndexOf("[/" + localKey + "]") - start;
			return content.Substring(start, length);
		}
		else
			return content;
	}

	public static string extractFileName(string uri)
	{
		if (uri.Contains(new Uri(Application.persistentDataPath + "/").AbsoluteUri))
			return uri.Replace(new Uri(Application.persistentDataPath + "/").AbsoluteUri, "");
		else if (uri.Contains(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri))
			return uri.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, "");
		else
			return uri;
	}

	public static IEnumerator delayGOSelection(GameObject go, int nbYield = 1)
	{
		for (int i = 0; i < nbYield; i++)
			yield return null;
		EventSystem.current.SetSelectedGameObject(go);
	}
}