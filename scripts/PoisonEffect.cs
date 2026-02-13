using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    EntityStatHandler entityStats;
    float duration = 5f;
    float timer = 0f;
    float damagePerSecond = 5f;
    float lastDamageTime;

    // Start is called before the first frame update
    void Start()
    {
        lastDamageTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
        } else
        {
            RemoveEffect();
        }

        if(lastDamageTime + 1f < Time.time)
        {
            if (entityStats != null)
            {
                entityStats.currentHealth -= damagePerSecond;
            }
            lastDamageTime = Time.time;
        }
    }

    public void ApplyEffect()
    {
        entityStats = GetComponent<EntityStatHandler>();
        if (entityStats != null)
        {
            entityStats.statusEffects.Add("Poison");
        } else {
            Destroy(this);
        }
    }
    public void RemoveEffect()
    {
        if (entityStats != null)
        {
            entityStats.statusEffects.Remove("Poison");
        }
        Destroy(this);
    }
}
