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
	public int briefingType = 0;

	public Dialog clone()
    {
		Dialog copy = new Dialog();
		copy.text = text;
		copy.img = img;
		copy.imgHeight = imgHeight;
		copy.camX = camX;
		copy.camY = camY;
		copy.sound = sound;
		copy.video = video;
		copy.enableInteraction = enableInteraction;
		copy.briefingType = briefingType;
		return copy;
    }

	public bool isEqualTo(Dialog dialog)
    {
		return dialog.text == text && dialog.img == img && dialog.imgHeight == imgHeight && dialog.camX == camX && dialog.camY == camY && dialog.sound == sound && dialog.video == video && dialog.enableInteraction == enableInteraction && dialog.briefingType == briefingType;

	}
}

[Serializable]
public class DataLevel
{
	public string src; // contains full uri including persistentDataPath or streamingAssetsPath OR special tokens to come back to editors (scenario and level)
	public string name; // The name of the level without extension
	public List<Dialog> overridedDialogs = null;

	public DataLevel clone()
	{
		DataLevel copy = new DataLevel();
		copy.src = src;
		copy.name = name;
		if (overridedDialogs != null)
		{
			copy.overridedDialogs = new List<Dialog>();
			foreach (Dialog dialog in overridedDialogs)
				copy.overridedDialogs.Add(dialog.clone());
		}
		return copy;
	}

	public bool dialogsEqualsTo (List<Dialog> checkDialogs)
    {
		if (checkDialogs.Count != overridedDialogs.Count)
			return false;
        else
        {
			for (int i = 0; i < overridedDialogs.Count; i++)
				if (!overridedDialogs[i].isEqualTo(checkDialogs[i]))
					return false;
			return true;
        }
    }
}