using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class PoolObject<T> : MonoBehaviour
{
    public string Type = typeof(T).ToString();
    public int PoolMaxCount;
    public bool StartDisable = true;

    [Space]
    [HideInInspector]
    public int __inspector_divider;

    protected void InitPoolObject()
    {
        Type = typeof(T).ToString();
        PoolManager.Instance.AddPoolObject(this);
        Destroy(this.gameObject);
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(PoolObject<object>))]
public class PoolObjectEditor<T> : Editor
{
    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();
        GUILayout.Space(10);

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif