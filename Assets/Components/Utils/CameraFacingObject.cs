﻿using UnityEngine;

[ExecuteInEditMode]
public class CameraFacingObject : MonoBehaviour
{
    public bool matchRotation;

    private Camera mainCamera;

    private void Update()
    {
        if (!mainCamera) mainCamera = Camera.main;

        transform.forward = mainCamera.transform.forward;

        if (matchRotation) transform.rotation = mainCamera.transform.rotation;
    }
}
