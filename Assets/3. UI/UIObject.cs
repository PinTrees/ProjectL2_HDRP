using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class UIObject : MonoBehaviour
{
    [SerializeField] 
    protected GameObject UI;

    public bool IsShow() { return UI.activeSelf; }
    public virtual void Show() { if (!UI.activeSelf) UI.SetActive(true); }
    public virtual void Close() { if (UI.activeSelf) UI.SetActive(false); }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UIObject), true), CanEditMultipleObjects]
public class UIObjectEditor : Editor
{
    UIObject owner;

    void OnEnable()
    {
        owner = target as UIObject;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Open"))
        {
            owner.Show();
        }
        if (GUILayout.Button("Close"))
        {
            owner.Close();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}
#endif