using FYFY;
using UnityEngine;

/// <summary>
/// Manage decoration rotation
/// </summary>
public class RandomRotationSystem : FSystem {

    private Family f_rotationGOs = FamilyManager.getFamily(new AllOfComponents(typeof(RandomRotation)));

    public CurrentSettingsValues currentSettingsValues;

    public static RandomRotationSystem instance;

    public RandomRotationSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        foreach (GameObject go in f_rotationGOs)
        {
            RandomRotation rr = go.GetComponent<RandomRotation>();
            rr.speed = Random.Range(0.02f, 0.15f);
            rr.axes = new Vector3(Random.value, Random.value, Random.value);
        }
        if (f_rotationGOs.First() != null)
        {
            Transform decoration = f_rotationGOs.First().transform.parent;
            decoration.Rotate(Random.Range(0, 360), 0, 0);
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        if (currentSettingsValues.values.currentAnimation == 0 && f_rotationGOs.First().transform.parent.gameObject.activeInHierarchy)
            GameObjectManager.setGameObjectState(f_rotationGOs.First().transform.parent.gameObject, false);
        if (currentSettingsValues.values.currentAnimation == 1 && !f_rotationGOs.First().transform.parent.gameObject.activeInHierarchy)
            GameObjectManager.setGameObjectState(f_rotationGOs.First().transform.parent.gameObject, true);

        if (f_rotationGOs.First().transform.parent.gameObject.activeInHierarchy)
        {
            foreach (GameObject go in f_rotationGOs)
            {
                RandomRotation rr = go.GetComponent<RandomRotation>();
                go.transform.Rotate(rr.axes.x * rr.speed, rr.axes.y * rr.speed, rr.axes.z * rr.speed);
            }
            if (f_rotationGOs.First() != null)
            {
                Transform decoration = f_rotationGOs.First().transform.parent;
                decoration.Rotate(Time.deltaTime * 2, 0, 0);
            }
        }
    }
}