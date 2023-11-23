using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class NavigationManager : Singleton<NavigationManager>   
{
    public Vector3 NavigationBuildScale;
    public Vector3 NavigationCellScale;
    public Transform target;

#if UNITY_EDITOR
    public void _Editor_BuildNavigation()
    {
        target = GameObject.FindAnyObjectByType<Player>().transform;
        var terrain = TerrainExtentionsX.GetNearTerrainCollider(transform);

        var x = NavigationBuildScale.x / NavigationCellScale.x;
        var y = NavigationBuildScale.y / NavigationCellScale.y;

        for(int i = 0; i < x; ++i)
        {
            for(int j = 0; j < y; ++j)
            {
                Ray ray;
            }
        }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(NavigationManager))]
public class NavigationManagerEditor : Editor
{
    NavigationManager owner;
    private void OnEnable()
    {
        owner = (NavigationManager)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();



        if(GUI.changed || EditorGUI.EndChangeCheck())
        {
            Debug.Log("[Navigation] Save");
        }
    }
}
#endif
