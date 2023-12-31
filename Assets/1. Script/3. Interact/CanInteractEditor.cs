using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(CanInteract))]
public class CanInteractEditor : Editor
{
    CanInteract owner;

    private void OnEnable()
    {
        owner = target as CanInteract;
    }

    private void OnSceneGUI()
    {
        // 테두리 그리기
        Handles.color = Color.blue;
        Handles.DrawWireDisc(owner.transform.position, Vector3.up, owner.InteractionRange, 1.5f);
    }
}
#endif