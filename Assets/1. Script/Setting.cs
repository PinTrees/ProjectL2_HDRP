using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Setting : MonoBehaviour
{
    void Start()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
    }

    void Update()
    {
        
    }
}
