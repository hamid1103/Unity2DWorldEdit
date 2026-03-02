using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public float MouseX;
    public float MouseY;
    public Vector3? startPoint;
    public Vector3 targetPoint;
    public Vector3 camDirection;
    public bool locked;

    public Camera worldCaptureCamera;
    
    private Camera cam;
    private Transform transform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = this.GetComponent<Camera>();
        transform = this.GetComponent<Transform>();
        locked = true;
    }

    // Update is called once per frame
    void Update()
    {
        MouseX = cam.ScreenToViewportPoint(Input.mousePosition).x;
        MouseY = cam.ScreenToViewportPoint(Input.mousePosition).y;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
        {
            transform.position = new Vector3(0, 0, transform.position.z);
            cam.orthographicSize = 5;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            cam.orthographicSize = Math.Clamp(cam.orthographicSize - (Input.GetAxis("Mouse ScrollWheel") * 10), 1, 12);
            worldCaptureCamera.orthographicSize = cam.orthographicSize;
        }
    }

    public void ToCenter()
    { 
        transform.position = new Vector3(0, 0, transform.position.z);
    }

    private void FixedUpdate()
    {
        if (!locked)
        {
            //New cam movement
            if (Input.GetMouseButton(2) || Input.GetKey(KeyCode.LeftShift))
            {
                Vector3 camPointerPos = cam.ScreenToWorldPoint(Input.mousePosition);
                //Get Starting point
                if (startPoint == null)
                {
                    startPoint = this.transform.position;
                }
            
                targetPoint = new Vector3(camPointerPos.x, camPointerPos.y, 0);
                camDirection = new Vector3(targetPoint.x - transform.position.x, targetPoint.y - transform.position.y, 0);
            
                transform.position += camDirection * 2 * Time.fixedDeltaTime;
            }
            else
            {
                startPoint = null;
            }
        }
    }
}
