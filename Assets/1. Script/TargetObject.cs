using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public GameObject indicator;

    public float disable_indicator_distance_near;
    public float disable_indicator_distance_far;

    private void Start()
    {
        indicator.GetComponent<uTargetIndicator>().Enter(transform.parent.gameObject, Camera.main, UIManager.Instance.IndicatorUI.canvas);
    }

    private void Update()
    {

    }
}
