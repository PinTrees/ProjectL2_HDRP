using KWS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum VEHICLE_TYPE
{
    BOAT_SMALL,
    END,
}


public class Vehicle : MonoBehaviour
{
    public VEHICLE_TYPE Type;
    public Transform RideTransform;
    public Vector3 Forward;
    public Vector3 ForwardAngleOffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
