using UnityEngine;
using FYFY;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// This system enables to manage game mode: playmode vs editmode
/// </summary>
public class ModeManager : FSystem {

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	public static ModeManager instance;

    public ModeManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		f_playingMode.addEntryCallback(delegate { 
			// remove all EditMode
			foreach(GameObject editModeGO in f_editingMode)
				foreach (EditMode em in editModeGO.GetComponents<EditMode>())
					GameObjectManager.removeComponent(em);
		});

		f_editingMode.addEntryCallback(delegate {
			// remove all PlayMode
			foreach (GameObject editModeGO in f_playingMode)
				foreach (PlayMode em in editModeGO.GetComponents<PlayMode>())
					GameObjectManager.removeComponent(em);
		});
	}

	// Used in ExecuteButton in inspector
	public void setPlayingMode(){
		GameObjectManager.addComponent<PlayMode>(MainLoop.instance.gameObject);
	}
	
	// Used in StopButton and ReloadState in inspector
	public void setEditMode()
	{
		GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
		GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
		{
			verb = "stopped",
			objectType = "program"
		});
	}
}
