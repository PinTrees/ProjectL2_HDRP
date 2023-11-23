using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TransformExtentions
{
    static public void LookAtTarget_Humanoid(Transform ownerTransform, Transform targetTransform, float speedNormal)
    {
        Vector3 targetDirection = targetTransform.position - ownerTransform.position;
        targetDirection.y = 0;

        if (targetDirection != Vector3.zero)
        {
            ownerTransform.rotation = Quaternion.Slerp(ownerTransform.rotation 
                                                     , Quaternion.LookRotation(targetDirection)
                                                     , speedNormal * Time.deltaTime);
        }
    }

    static public void LookAtTarget_Humanoid_Dir(Transform ownerTransform, Vector3 direction, float speedNormal)
    {
        if (direction != Vector3.zero)
        {
            ownerTransform.rotation = Quaternion.Slerp(ownerTransform.rotation
                                                     , Quaternion.LookRotation(direction)
                                                     , speedNormal * Time.deltaTime);
        }
    }

    static public void LookAtTarget_Humanoid_FixedTime(Transform ownerTransform, Transform targetTransform, float fixed_duration)
    {
        var targetPostion = targetTransform.position;
        targetPostion.y = ownerTransform.position.y;

        var rotate = Quaternion.LookRotation(targetPostion - ownerTransform.position);
        ownerTransform.DORotate(rotate.eulerAngles, fixed_duration);
    }



    static public void FreezeRotationXZ(Transform transform) { transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0); }

    static public void Overwrite(GameObject target_move, Transform target)
    {
        target_move.transform.position = target.position;
        target_move.transform.rotation = target.rotation;
        target_move.transform.localScale = target.localScale;
    }

    /// <summary>
    /// Y 축 값은 무시됩니다.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    static public Vector3 TerrainSurffacePositon(Vector3 targetPosition)
    {
        var trY = Terrain.activeTerrain.SampleHeight(new Vector3(targetPosition.x, 0, targetPosition.z)) + Terrain.activeTerrain.transform.position.y;
        return new Vector3(targetPosition.x, trY, targetPosition.z);
    }
}
