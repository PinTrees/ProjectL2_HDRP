using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


// Editor Main
[CustomEditor(typeof(AnimallGroup))]
public class AnimallGroupEditor : Editor
{
    AnimallGroup value;

    private void OnEnable()
    {
        value = (AnimallGroup)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        BuildSpawnerButton();


        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    public void BuildSpawnerButton()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        // Remove Spawner
        if (GUILayout.Button("Remove Animall"))
        {
            value._EditorRemoveObject();
        }

        EditorGUILayout.Space();

        // Create Spawner
        EditorGUI.BeginChangeCheck(); // 이 위치에서 변경 사항 체크를 시작

        if (GUILayout.Button("Create Animall"))
        {
            value._EditorSpawnObject();
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Refresh"))
        {
        }

        EditorGUILayout.EndVertical();
    }


    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(value.transform.position, Vector3.up, value.SpawnRange);

        Handles.color = Color.red;
        Handles.DrawWireDisc(value.transform.position, Vector3.up, value.WalkRange);
    }
}
