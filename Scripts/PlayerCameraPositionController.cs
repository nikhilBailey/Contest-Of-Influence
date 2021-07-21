using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraPositionController : MonoBehaviour
{
    //Camera Zoom
    public static readonly float zoomSpeed = 1;
    public float targetOrtho;
    public static readonly float smoothSpeed = 2.0f;
    public static readonly float minOrtho = 0.2f;
    public static readonly float maxOrtho = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.localPosition = new Vector3(0,0,10);
        Camera.main.transform.Rotate(0, 180, 0);

        //Inititalizes zoom
        targetOrtho = Camera.main.orthographicSize;
    }

    private static float scaleFactor = 2.5F;

    private void moveKeyPressed(float x, float y, float z) {
        if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) {
            Camera.main.transform.Translate(scaleFactor * x, scaleFactor * y, scaleFactor * z);
        }
        else {
            Camera.main.transform.Translate(x, y, z);
        }
    }

    void Update() {
        var transform = Camera.main.transform;

        if (Input.GetKey(KeyCode.W)) {
            moveKeyPressed(0, 0.025f, 0);
        }
        if (Input.GetKey(KeyCode.S)) {
            moveKeyPressed(0, -.025f, 0);
        }
        if (Input.GetKey(KeyCode.A)) {
            moveKeyPressed(-.025f, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            moveKeyPressed(.025f, 0, 0);
        }

        //handles scrolling zoom
        float scroll = Input.GetAxis ("Mouse ScrollWheel");
        if (scroll != 0.0f) {
            targetOrtho -= scroll * zoomSpeed * (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) ? scaleFactor : 1);
            targetOrtho = Mathf.Clamp (targetOrtho, minOrtho, maxOrtho);
        }
        Camera.main.orthographicSize = Mathf.MoveTowards (Camera.main.orthographicSize,
            targetOrtho, (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) ? scaleFactor : 1f) * 1.5f * smoothSpeed * Time.deltaTime);
    }
}