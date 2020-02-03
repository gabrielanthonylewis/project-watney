using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycleOLD : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeedMultiplier = 10.0f;

    private void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.right, rotationSpeedMultiplier * Time.deltaTime);
    }
}
