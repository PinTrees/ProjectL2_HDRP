using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


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
        // �׵θ� �׸���
        Handles.color = Color.blue;
        Handles.DrawWireDisc(owner.transform.position, Vector3.up, owner.InteractionRange, 1.5f);
    }
}
