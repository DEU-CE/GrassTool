using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Mathf.Sin(Time.deltaTime*rotationSpeed));
    }
}
