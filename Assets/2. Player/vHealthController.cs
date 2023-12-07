using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class vHealthController : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    private Action onDeathEvent;

    public float GetFillAmount() { return (float)currentHealth / (float)maxHealth; }

    public void TakeDamage(vDamage damage)
    {
        currentHealth -= damage.damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            if (onDeathEvent != null)
                onDeathEvent();
        }
    }

}


public class vDamage
{
    public int damage;
    public vDamage(int damage)
    {
        this.damage = damage;
    }
}