using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuckEffect : MonoBehaviour
{
    EntityStatHandler entityStats;
    float duration = 2f;
    float timer = 0f;
    int jumps = 0;

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

        if (Input.GetButtonDown("Jump") && gameObject.CompareTag("Player"))
        {
            jumps++;
        }
        if (jumps >= 4)
        {
            RemoveEffect();
        }
    }

    public void ApplyEffect()
    {
        entityStats = GetComponent<EntityStatHandler>();
        if (entityStats != null)
        {
            entityStats.speedMultiplier.Add(0);
            entityStats.statusEffects.Add("Stuck");
        } else {
            Destroy(this);
        }
    }
    public void RemoveEffect()
    {
        if (entityStats != null)
        {
            entityStats.speedMultiplier.Remove(0);
            entityStats.statusEffects.Remove("Stuck");
        }
        Destroy(this);
    }
}
