using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform _mainCamTransform;
    
    void Start()
    {
        _mainCamTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + _mainCamTransform.rotation * Vector3.forward, 
            _mainCamTransform.rotation * Vector3.up); // wtf
    }
}
