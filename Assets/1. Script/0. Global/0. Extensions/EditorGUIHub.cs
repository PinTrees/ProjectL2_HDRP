using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorGUIHub
{
    static public void BeginVerticalWindow(string title, float padding_hor=4)
    {
        GUILayout.BeginVertical(title, "window");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(padding_hor);
        EditorGUILayout.BeginVertical();
    }

    static public void EndVerticalWindow(float padding_hor = 4, float padding_vert = 4) 
    {
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(padding_hor);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(padding_vert);
        EditorGUILayout.EndVertical();
    }
}
