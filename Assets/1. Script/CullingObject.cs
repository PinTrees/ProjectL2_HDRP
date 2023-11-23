using NGS.AdvancedCullingSystem.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullingObject : MonoBehaviour
{
    public bool IsLODGroup;
    List<DC_SourceSettings> dynamicCullingSources = new();

    void Start()
    {
        Debug.Log("[Culling] Add Renderer");
       
        if (IsLODGroup)
        {
            dynamicCullingSources.Add(DC_Controller.GetById(0).AddObjectForCulling(GetComponent<LODGroup>()));
        }
        else
        {
            foreach (var renderer in transform.GetComponentsInChildren<MeshRenderer>())
            {
                dynamicCullingSources.Add(DC_Controller.GetById(0).AddObjectForCulling(renderer));
            }
        }
        return;
    }

    void OnDisable()
    {
        Debug.Log("[Culling] Remove Renderer");

        foreach (var source in dynamicCullingSources)
        {
            DC_Controller.GetById(0).RemoveObjectForCulling(source);
        }
    }
}
