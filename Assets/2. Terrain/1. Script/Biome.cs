using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Biome : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(Biome)), CanEditMultipleObjects]
public class BiomeEditor : Editor
{
    Biome owner;

    public void OnEnable()
    {
        owner = (Biome)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        GUILayout.BeginVertical();
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif