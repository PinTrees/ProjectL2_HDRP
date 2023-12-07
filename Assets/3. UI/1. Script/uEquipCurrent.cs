using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class uEquipCurrent : MonoBehaviour
{
    WeaponController weaponController;

    [SerializeField] uEquipSlot righthand_slot;
    [SerializeField] uEquipSlot lefthand_slot;

    [SerializeField] Transform leftslot_parent;
    [SerializeField] Transform rightslot_parent;

    List<uEquipSlot> left_slots = new();
    List<uEquipSlot> right_slots = new();

    private void Start()
    {
        left_slots = leftslot_parent.GetComponentsInChildren<uEquipSlot>().ToList();
        right_slots = rightslot_parent.GetComponentsInChildren<uEquipSlot>().ToList();
    }

    private void Update()
    {
        /// ������Ʈ�ѷ� ���� Ȯ��
        if (weaponController)
        {
            /// ������ �������� Ȯ��
            if (weaponController.CurrentEquipWeapon_Right)
            {
                /// ���Ⱑ ����Ǿ��� ��� UI ����
                if (righthand_slot.weapon != weaponController.CurrentEquipWeapon_Right)
                {
                    righthand_slot.Show(weaponController.CurrentEquipWeapon_Right);
                }
            }
            else
            {
                righthand_slot.Close();
            }

            /// �޼� �������� Ȯ��
            if (weaponController.CurrentEquipWeapon_Left)
            {
                /// ���Ⱑ ����Ǿ��� ��� UI ����
                if (lefthand_slot.weapon != weaponController.CurrentEquipWeapon_Left)
                {
                    lefthand_slot.Show(weaponController.CurrentEquipWeapon_Left);
                }
            }
            else
            {
                lefthand_slot.Close();
            }

            /// ������ ������ Ȯ��
            for(int i = 0; i < 5; ++i)
            {
                if (weaponController.Righthand[i] != null)
                {
                    if(weaponController.Righthand[i] != right_slots[i].itemdata)
                        right_slots[i].Show(weaponController.Righthand[i]);
                }
                else right_slots[i].Close();
            }

            /// �޼� ������ Ȯ��
            for(int i = 0; i < 5; ++i)
            {
                if (weaponController.Lefthand[i] != null)
                {
                    if(weaponController.Lefthand[i] != left_slots[i].itemdata)
                        left_slots[i].Show(weaponController.Lefthand[i]);
                }
                else left_slots[i].Close();
            }
        }
        else
        {
            weaponController = PlayerManager.Instance.Player.AI.weaponController;
        }
    }
}
