using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject focus; // objetive to follow
    public float distance; // distance between camera and focus
    public float height; // height between camera and focus

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, focus.transform.position + focus.transform.TransformDirection(new Vector3(0f,height,-distance)), Time.deltaTime);
        transform.LookAt(focus.transform);
    }
}
