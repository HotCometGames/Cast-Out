using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatHandler : MonoBehaviour
{
    public float maxHealth = 100;
    public List<float> healthMultiplier = new List<float>() { 1 };
    public float currentHealth = 0;
    public float maxDamage = 10;
    public List<float> damageMultiplier = new List<float>() { 1 };
    public float currentDamage = 10;
    public float maxSpeed = 10;
    public List<float> speedMultiplier = new List<float>() { 1 };
    public float currentSpeed = 10;
    public List<string> statusEffects = new List<string>();
    void Start()
    {
    }

    void Update()
    {
        UpdateStats();
    }


    void UpdateStats()
    {


        currentDamage = maxDamage;
        foreach (float multiplier in damageMultiplier)
        {
            currentDamage *= multiplier;
        }

        currentSpeed = maxSpeed;
        foreach (float multiplier in speedMultiplier)
        {
            currentSpeed *= multiplier;
        }
    }
}

