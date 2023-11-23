using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public enum ITEM_TYPE
{
    WEAPON,
    ARMOR,
    OTHER,
    END
}


[System.Serializable]
[CreateAssetMenu(fileName = "Item Data", menuName = "Scriptable Object/Item Data", order = int.MaxValue)]
public class ItemData : ScriptableObject
{
    [SerializeField]
    [HideInInspector]
    public string UID;
    
    [SerializeField] public ITEM_TYPE Type;
    [SerializeField] public Sprite Icon;

    [SerializeField] public GameObject Prefab;

    [SerializeField] public string Name;
    [SerializeField] public string Info;
    [SerializeField] public int Price;
}


[System.Serializable]
public class Item
{
    public ItemData data;
    public int count;
}

 
[System.Serializable]
public class ItemDrop
{
    [SerializeField] public ItemData data;

    public float rate;
    public int count;

    public int minCount;
    public int maxCount;
}


#if UNITY_EDITOR
[CustomEditor(typeof(ItemData)), CanEditMultipleObjects]
public class ItemDataEditor : Editor
{
    private ItemData owner;

    private void OnEnable()
    {
        owner = (ItemData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"UID: {owner.UID}");
        if(owner.UID.Length < 5)
        {
            if(GUILayout.Button("Create UID"))
            {
                owner.UID = System.Guid.NewGuid().ToString();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        DrawDefaultInspector();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[Item Data] Save ScriptableObject");
        }
    }
}
#endif