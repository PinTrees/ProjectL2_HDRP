using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uIndicatorUI : UIObject
{
    public Canvas canvas;
    public List<uTargetIndicator> targetIndicators = new();

    public Camera mainCamera;

    void Start()
    {
        
    }

    void Update()
    {
        targetIndicators.ForEach(e =>
        {
            e.UpdateTargetIndicator();
        });
    }

    public void AddTargetIndicator(uTargetIndicator indicator)
    {
        indicator.transform.SetParent(canvas.transform, false);
        targetIndicators.Add(indicator);
    }
}
