using UnityEngine;
using FYFY;
using TMPro;

/// <summary>
/// This system enables to manage game mode: playmode vs editmode
/// </summary>
public class ModeManager : FSystem {

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	public GameObject playButtonAmount;

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
		// If amount enabled, reduce by 1
		if (playButtonAmount.activeSelf) {
			TMP_Text amountText = playButtonAmount.GetComponentInChildren<TMP_Text>();
			amountText.text = ""+(int.Parse(amountText.text) - 1);
		}
	}
	
	// Used in StopButton and ReloadState in inspector
	public void setEditMode()
	{
		GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
	}
}
