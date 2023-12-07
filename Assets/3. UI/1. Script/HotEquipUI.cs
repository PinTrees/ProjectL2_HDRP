using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotEquipUI : MonoBehaviour
{
    private const int MAX_SLOT_COUNT = 8;

    [SerializeField] uItemSlot _slot;
    [SerializeField] Transform _slotParent;

    private List<uItemSlot> _slotList = new List<uItemSlot>();


    void Start()
    {
        for(int i = 0; i < MAX_SLOT_COUNT; ++i)
        {
            var go = Instantiate(_slot);
            go.transform.SetParent(_slotParent, false);

            var slot = go.GetComponent<uItemSlot>();
            _slotList.Add(slot);
        }

        Destroy(_slot);
    }


    public void Init()
    {
        Close();
    }

    public void Show()
    {
        _slotList.ForEach(e => e.Close());
        gameObject.SetActive(true);
    }


    public void Close()
    {
        gameObject.SetActive(false);
    }
}
