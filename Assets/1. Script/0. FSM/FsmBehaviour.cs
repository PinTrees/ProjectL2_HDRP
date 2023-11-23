using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FsmBehaviour : MonoBehaviour
{
    [SerializeField]
    Transform stateParentTransform;

    Dictionary<string, StateBehaviour> stateMap = new();

    StateBehaviour currentState;

    void Start()
    {
        var states = stateParentTransform.GetComponentsInChildren<StateBehaviour>().ToList();
        states.ForEach(e =>
        {
            stateMap[e.Name] = e;
        });
    }

    void Update()
    {
        currentState?.ManualUpdate();
    }
}
