using UnityEngine;

public class TriggerCollisionSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip collisionSound;
    public AudioClip destructionSound;

    // See Events in death animations
    public void PlayShock()
    {
        audioSource.PlayOneShot(collisionSound);
    }

    // See Events in death animations
    public void PlayDestruction()
    {
        audioSource.PlayOneShot(destructionSound);
    }
}
