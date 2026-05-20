using UnityEngine;
using FYFY;

public class CompetenciesChecker_wrapper : BaseWrapper
{
	public UnityEngine.Transform skillsInvolvedContent;
	public UnityEngine.Transform skillsIgnoredContent;
	public UnityEngine.GameObject SkillPrefab;
	public CurrentSettingsValues cs;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "skillsInvolvedContent", skillsInvolvedContent);
		MainLoop.initAppropriateSystemField (system, "skillsIgnoredContent", skillsIgnoredContent);
		MainLoop.initAppropriateSystemField (system, "SkillPrefab", SkillPrefab);
		MainLoop.initAppropriateSystemField (system, "cs", cs);
	}

	public void CheckSkills()
	{
		MainLoop.callAppropriateSystemMethod (system, "CheckSkills", null);
	}

}
