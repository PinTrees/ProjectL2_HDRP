using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class Pool
{
    private string code;

    private int poolCount = 0;
    private GameObject container;
    private GameObject poolObject;

    private int index;

    private List<GameObject> pool = new();
    private List<Component> components = new();

    private Action<GameObject> createEvent;
    private Action<GameObject> getEvent;

    public Pool(string code, GameObject _obj, int count)
    {
        index = 0;
        this.code = code;
        this.poolObject = _obj;
        poolCount = count;
    }

    public void SetCreateEvent(Action<GameObject> action) { createEvent = action; }
    public void SetGetPoolEvent(Action<GameObject> action) { getEvent = action; }

    public void Create()
    {
        container = new GameObject(code);
        for (int i = 0; i < poolCount; ++i)
        {
            var spawn = GameObject.Instantiate(poolObject);
            spawn.transform.parent = container.transform;
            
            pool.Add(spawn);
            components.Add(spawn.GetComponent(code));

            if (createEvent != null)
                createEvent(spawn);
        }
    }

    public GameObject Get()
    {
        index++;
        if (index >= pool.Count)
            index = 0;

        return pool[index];
    }

    public T GetComponent<T>() where T : MonoBehaviour
    {
        index++;
        if (index >= pool.Count)
            index = 0;

        return components[index] as T;
    }
}
