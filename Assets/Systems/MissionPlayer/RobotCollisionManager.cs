using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class RobotCollisionManager : FSystem
{
    private Family f_agentCollision = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player", "Drone"));
    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    private bool activeCollision;

    protected override void onStart()
    {
        activeCollision = false;

        f_playingMode.addEntryCallback(delegate {
            activeCollision = true;
        });
        f_editingMode.addEntryCallback(delegate {
            activeCollision = false;
        });

        f_agentCollision.addEntryCallback(onNewCollision);
    }

	private void onNewCollision(GameObject agent)
	{
        if (activeCollision)
        {
            Triggered3D trigger = agent.GetComponent<Triggered3D>();
            foreach (GameObject target in trigger.Targets)
            {
                //Check if the agent collide with a detection cell
                if (target.GetComponent<Detector>() != null)
                {
                    //end level
                    GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Detected });
                    agent.GetComponent<Animator>().SetTrigger("Death");
                    agent.GetComponent<Collider>().enabled = false;
                    agent.GetComponent<ScriptRef>().isBroken = true;
                    break;
                }
                if (!target.CompareTag("Coin"))
                {
                    if (agent.CompareTag("Player"))
                        GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Collision });
                    agent.GetComponent<Animator>().SetTrigger("Death");
                    agent.GetComponent<Collider>().enabled = false;
                    agent.GetComponent<ScriptRef>().isBroken = true;
                    if (agent.CompareTag("Drone"))
                        DetectorManager.instance.updateDetectors();
                }
            }
        }
	}
}
