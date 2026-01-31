using UnityEngine;

public class TriggerCollisionSound : MonoBehaviour
{
    public AudioSource audioSource;

    // See Events in death animations
    public void playShock()
    {
        audioSource.PlayOneShot(Resources.Load<AudioClip>("Sound/collision"));
    }

    // See Events in death animations
    public void playDestruction()
    {
        audioSource.PlayOneShot(Resources.Load<AudioClip>("Sound/destruction"));
    }
}
