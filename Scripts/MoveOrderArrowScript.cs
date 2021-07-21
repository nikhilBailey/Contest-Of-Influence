using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOrderArrowScript : MonoBehaviour
{
    private float zRotation;

    public GameObject _handler;
    private RenderingEngineAndGameClock handler;

    //Runs when this Script is loaded
    void awake() {
        // handler = GameObject.Find("Grid").GetComponent<RenderingEngineAndGameClock>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - transform.position);
        transform.Rotate(0, 0, 90);

        zRotation = transform.eulerAngles.z;
    }

    void OnMouseUp() {
        Debug.Log("zRotation: " + zRotation);
        
        //Convert to direction between 1 and 8
        zRotation -= 22.5f;
        if (zRotation < 0) zRotation += 360f;
        zRotation /= 45f;
        zRotation = (float) Math.Ceiling(zRotation);
        
        Debug.Log("Direction Input: " + zRotation);

        handler = _handler.GetComponent<RenderingEngineAndGameClock>();
        if (handler == null) {
            throw new Exception("No handler set on MoveArrow");
        }
        handler.moveOrder((int) zRotation);
    }
}
