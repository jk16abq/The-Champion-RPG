﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    [Header("Position Variables")]
    public Transform target;
    public float smoothing;
    public Vector2 maxPosition;
    public Vector2 minPosition;

    [Header("Position Reset")]
    public VectorValue camMin;
    public VectorValue camMax;

	// Use this for initialization
	void Start () {
        maxPosition = camMax.initialValue;
        minPosition = camMin.initialValue;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        //Camera smoothness
        //Find the distance between camera and a target by using linear interpolation 
		if(transform.position != target.position)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            //Prevent camera going off the map by using clamp
            targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x , maxPosition.x );
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y , maxPosition.y );

            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing);


        }
	}

}
