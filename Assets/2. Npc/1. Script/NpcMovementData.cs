using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Npc Movement Data", menuName = "Scriptable Object/Npc Movement Data", order = int.MaxValue)]
public class NpcMovementData : ScriptableObject
{
    public bool FixeblePosition = false;

    public float nextRandomDirectionTime;       // 다음 랜덤 이동 방향을 설정할 시간
    public float MinRandomDirectionTime = 1.0f; // 랜덤 이동 방향 갱신 최소 간격
    public float MaxRandomDirectionTime = 5.0f; // 랜덤 이동 방향 갱신 최대 간격

    public bool IsChangeDirection() { return Time.time >= nextRandomDirectionTime; }

    public void ChangeMoveDirection_RootMotion<T>(FSM<T> AI)
    {
        if (AI.RandomPercent(0.5f))
        {
            var randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0;

            AI.vMoveDir = randomDirection.normalized;
        }
        else
        {
            AI.vMoveDir = Vector3.zero;
        }

        nextRandomDirectionTime = Time.time + Random.Range(MinRandomDirectionTime, MaxRandomDirectionTime);
    }
}
