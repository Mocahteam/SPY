using UnityEngine;
using FYFY;
using System.Collections;

/// <summary>
/// This system is in charge to play lobby animations
/// </summary>
public class LobbyAnimation : FSystem {
    public GameObject Door;
    public GameObject Drone;
    public GameObject Kyle;
    public GameObject Destiny;
    public GameObject R102;

    private string[] anims = new string[] { "Action", "ArmStretch", "IntroNail" };

    public static LobbyAnimation instance;

    public LobbyAnimation()
    {
        instance = this;
    }

    protected override void onStart()
    {
        MainLoop.instance.StartCoroutine(AnimDoorAndR102());
        MainLoop.instance.StartCoroutine(AnimKyleAndDestiny());
        MainLoop.instance.StartCoroutine(AnimDrone());
    }

    private IEnumerator AnimDoorAndR102()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 10));
            R102.GetComponent<Animator>().SetTrigger("Action");
            ActivationSlot doorSlot = Door.GetComponent<ActivationSlot>();
            if (!doorSlot.state)
                Door.GetComponent<Animator>().SetTrigger("Open");
            else
                Door.GetComponent<Animator>().SetTrigger("Close");
            doorSlot.state = !doorSlot.state;
        }
    }

    private IEnumerator AnimKyleAndDestiny()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));
            if (Random.Range(0f, 1f) > 0.5f)
            {
                Animator kyleAnim = Kyle.GetComponent<Animator>();
                kyleAnim.SetTrigger(anims[Random.Range(0, 3)]);
            }
            else
            {
                Animator destinyAnim = Destiny.GetComponent<Animator>();
                destinyAnim.SetTrigger(anims[Random.Range(0, 3)]);
            }
        }
    }

    private IEnumerator AnimDrone()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 6));
            float step = Random.Range(0f, 50f);
            step *= Random.Range(0f, 1f) > 0.5f ? 1 : -1;
            Animation animator = Drone.GetComponentInChildren<Animation>();
            Quaternion target = animator.transform.rotation * Quaternion.AngleAxis(step, Vector3.up);
            while (animator.transform.rotation != target)
            {
                yield return null;
                animator.transform.rotation = Quaternion.RotateTowards(animator.transform.rotation, target, 0.5f);
            }
        }
    }
}