using DG.Tweening;
using KWS;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    const float RAY_MAX_DISTANCE = 1000;
    const float RAY_WATER_DISTANCE = 0.3f;

    [SerializeField] float mOffsetY;
    [SerializeField] Transform mTargetTransform;

    private void Start()
    {
        if(null == mTargetTransform)
        {
            mTargetTransform = transform;
        }
    } 



    private void OnTriggerStay(Collider other)
    {
    }


    public void Update()
    {
        //Used for water physics. It check all instances. 
        var waterSurfaceData = KWS.WaterSystem.GetWaterSurfaceData(mTargetTransform.position);

        //checking if the surface data is ready. Since I use asynchronous updating, the data may be available with a delay, so the first frame can be null. 
        if (waterSurfaceData.IsActualDataReady) 
        {
            var waterPosition = waterSurfaceData.Position;
            mTargetTransform.DOMoveY(waterPosition.y, 0.2f);
        }
            
        //KWS.WaterInstance.IsWaterRenderingActive = true;   //You can manually control the rendering of water (software occlusion culling)
        //KWS.WaterInstance.WorldSpaceBounds;   //world space bounds of the current quadtree mesh/custom mesh/river 
        //KWS.WaterInstance.IsCameraUnderwater(); //check if the current rendered camera intersect underwater volume
        //KWS.WaterInstance.IsSphereUnderWater(mTargetTransform.position, 2.5f); //Check if the current world space position/sphere is under water. 
    }
}
