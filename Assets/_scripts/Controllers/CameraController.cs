using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 currentPosition;

    private void Start()
    {
        currentPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition += transform.right * Input.GetAxis("Horizontal");
        currentPosition += transform.up * Input.GetAxis("Vertical");

        transform.position = currentPosition;
    }
}
