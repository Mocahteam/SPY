using UnityEngine;
using FYFY;

/// <summary>
/// This system enables to manage game mode: playmode vs editmode
/// </summary>
public class ModeManager : FSystem {

	private Family playingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family editingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	public static ModeManager instance;

    public ModeManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		playingMode_f.addEntryCallback(delegate { 
			// remove all EditMode
			foreach(GameObject editModeGO in editingMode_f)
				foreach (EditMode em in editModeGO.GetComponents<EditMode>())
					GameObjectManager.removeComponent(em);
		});

		editingMode_f.addEntryCallback(delegate {
			// remove all PlayMode
			foreach (GameObject editModeGO in playingMode_f)
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
	}
}
