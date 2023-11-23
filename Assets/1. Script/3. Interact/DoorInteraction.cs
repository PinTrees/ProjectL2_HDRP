using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoorInteraction : MonoBehaviour
{
    [SerializeField] Collider _collider;

    private bool _isOpened = false;

    void Start()
    {
        if(null == _collider) _collider = GetComponent<Collider>();
    }

    public void OnInteraction()
    {
        if (_isOpened) OnCloseDoor();
        else OnOpenDoor();
    }

    public void OnOpenDoor()
    {
        transform.DOKill();

        _isOpened = true;
        _collider.isTrigger = true;
        transform.DOLocalRotate(new Vector3(0, 90, 0), 1.0f).OnComplete(() =>
        {
            _collider.isTrigger = false;
        });
    }

    public void OnCloseDoor()
    {
        transform.DOKill();

        _isOpened = false;
        _collider.isTrigger = true;
        transform.DOLocalRotate(new Vector3(0, 0, 0), 1.0f).OnComplete(() =>
        {
            _collider.isTrigger = false;
        });
    }

    void Update()
    {
        
    }
}
