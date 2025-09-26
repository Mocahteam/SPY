using System;
using System.Collections.Generic;

[Serializable]
public class RawConstraint
{
	public string attribute;
	public string constraint;
	public string value;
	public string tag2;
	public string attribute2;
}

[Serializable]
public class RawFilter
{
	public string label;
	public string tag;
	public RawConstraint[] constraints;
}

[Serializable]
public class RawComp
{
	public string key;
	public string parentKey;
	public string name;
	public string description;
	public RawFilter[] filters;
	public string rule;
}

[Serializable]
public class RawListComp
{
	public string name;
	public List<RawComp> list = new List<RawComp>();
}

[Serializable]
public class RawListReferential
{
	public List<RawListComp> referentials = new List<RawListComp>();
}