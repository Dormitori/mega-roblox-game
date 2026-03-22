using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health;
    public event Action Death;
    
    public void SetHealth(float health)
    {
        this.health = health;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= Mathf.Epsilon)
            Death?.Invoke();
    }
    
}