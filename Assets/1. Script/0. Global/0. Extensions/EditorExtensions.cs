using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class EditorExtensions 
{
#if UNITY_EDITOR
    public static Camera _Editor_GetSceneCamera()
    {
        var cameras = SceneView.GetAllSceneCameras();
        if (cameras == null) return null;
        if (cameras.Length < 1) return null;

        return cameras.First();
    }

    public static Vector3? _Editor_GetSceneCameraPosition()
    {
        var cameras = SceneView.GetAllSceneCameras();
        if (cameras == null) return null;
        if (cameras.Length < 1) return null;

        return cameras.First().transform.position;
    }
#endif
}
