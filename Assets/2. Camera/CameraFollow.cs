using System.Collections;
using System.Collections.Generic;

using UnityEngine;


// 
public class CameraFollow : MonoBehaviour
{
    Transform target;

    [SerializeField] Vector3 offset;
    [SerializeField] Vector2 clampAxis = new Vector2(-60, 60);

    [SerializeField] float follow_smoothing = 5;
    [SerializeField] float rotate_smoothing = 5;
    [SerializeField] float senstivity = 5;

    public bool IsLockTarget = false;

    float rotX, rotY;
    bool cursorLocked = false;
    Transform cam;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main.transform;

        target = GameObject.FindObjectOfType<Player>().transform;
    }
    
    void Start()
    {
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, follow_smoothing * Time.fixedDeltaTime);

        if (IsLockTarget) LookAtTarget();
        else CameraTargetRotation();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(cursorLocked)
            {
                cursorLocked = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                cursorLocked = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void LookAtTarget()
    {
        transform.rotation = cam.rotation;
        Vector3 r = cam.eulerAngles;
        rotX = r.y;
        rotY = 1.8f;
    }

    public void CameraTargetRotation()
    {
        Vector2 mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        rotX += (mouseAxis.x * senstivity) * Time.fixedDeltaTime;
        rotY += (mouseAxis.y * senstivity) * Time.fixedDeltaTime;

        rotY = Mathf.Clamp(rotY, clampAxis.x, clampAxis.y);

        Quaternion localRotation = Quaternion.Euler(-rotY, rotX, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, localRotation, Time.fixedDeltaTime * rotate_smoothing);
    }
}
