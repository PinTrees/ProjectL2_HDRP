using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Npc Combat Data", menuName = "Scriptable Object/Npc Combat Data", order = int.MaxValue)]
public class NpcCombatData : ScriptableObject
{
    public bool IsFirstStrike = false;

    public float TargetFindAngle = 120;
    public float TargetFindRange = 7.5f;

    public float TargetBackAngle = 100;
    public float TargetBackRange = 2.5f;

    public float BelligerenceFactor;



    public bool HasFindTarget(Transform owner, Transform target)
    {
        /// ��ǥ Ÿ�� ��ġ
        var target_position = target.position;

        /// ����
        Vector3 vDir_target = target_position - owner.position;
        Vector3 vDir_forward = owner.forward;

        /// Ÿ�ٰ� �ڽ��� ���� Ȯ��
        float dotProduct = Vector3.Dot(vDir_target.normalized, vDir_forward.normalized);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        /// �������� �� ���� ���� ������ Ȯ��
        if (angle <= TargetFindAngle / 2f && Vector3.Distance(owner.position, target_position) < TargetFindRange)
        {
            return true;
        }
        else return false;
    }
}
