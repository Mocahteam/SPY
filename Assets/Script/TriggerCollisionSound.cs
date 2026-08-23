using UnityEngine;

public class TriggerCollisionSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip collisionSound;
    public AudioClip destructionSound;

    // See Events in death animations
    public void playShock()
    {
        audioSource.PlayOneShot(collisionSound);
    }

    // See Events in death animations
    public void playDestruction()
    {
        audioSource.PlayOneShot(destructionSound);
    }
}
