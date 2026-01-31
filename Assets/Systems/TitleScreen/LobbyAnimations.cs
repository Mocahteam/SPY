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

    private string[] anims = new string[] { "Action", "IntroNail", "ArmStretch" };

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
        Animator R102Anim = R102.GetComponent<Animator>();
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));
            int animChoice = Random.Range(0, 2);
            R102Anim.SetTrigger(anims[animChoice]);
            if (animChoice == 0)
            {
                ActivationSlot doorSlot = Door.GetComponent<ActivationSlot>();
                if (!doorSlot.state)
                    Door.GetComponent<Animator>().SetTrigger("Open");
                else
                    Door.GetComponent<Animator>().SetTrigger("Close");
                doorSlot.state = !doorSlot.state;
            }
        }
    }

    private IEnumerator AnimKyleAndDestiny()
    {
        Animator kyleAnim = Kyle.GetComponent<Animator>();
        Animator destinyAnim = Destiny.GetComponent<Animator>();
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));
            if (Random.Range(0f, 1f) > 0.5f)
            {
                kyleAnim.SetTrigger(anims[Random.Range(0, 3)]);
            }
            else
            {
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
            Quaternion target = Drone.transform.rotation * Quaternion.AngleAxis(step, Vector3.up);
            while (Drone.transform.rotation != target)
            {
                yield return null;
                Drone.transform.rotation = Quaternion.RotateTowards(Drone.transform.rotation, target, 0.5f);
            }
        }
    }
}