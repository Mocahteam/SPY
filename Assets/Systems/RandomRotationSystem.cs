using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manage decoration rotation
/// </summary>
public class RandomRotationSystem : FSystem {

    private Family f_rotationGOs = FamilyManager.getFamily(new AllOfComponents(typeof(RandomRotation)));

    protected override void onStart()
    {
        foreach (GameObject go in f_rotationGOs)
        {
            RandomRotation rr = go.GetComponent<RandomRotation>();
            rr.speed = Random.Range(0.02f, 0.15f);
            rr.axes = new Vector3(Random.value, Random.value, Random.value);
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {

        foreach (GameObject go in f_rotationGOs)
        {
            RandomRotation rr = go.GetComponent<RandomRotation>();
            go.transform.Rotate(rr.axes.x * rr.speed, rr.axes.y * rr.speed, rr.axes.z * rr.speed);
        }
    }
}