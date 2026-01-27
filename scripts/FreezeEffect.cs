using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeEffect : MonoBehaviour
{
    EntityStatHandler entityStats;
    float duration = 2f;
    float timer = 0f;
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
    }
    public void ApplyEffect()
    {
        entityStats = GetComponent<EntityStatHandler>();
        if (entityStats != null)
        {
            if (entityStats.statusEffects.Contains("Freeze"))
            {
                Destroy(this);
                return;
            }
            entityStats.speedMultiplier.Add(.6f);
            entityStats.statusEffects.Add("Freeze");
        } else {
            Destroy(this);
        }
    }
    public void RemoveEffect()
    {
        if (entityStats != null)
        {
            entityStats.speedMultiplier.Remove(.6f);
            entityStats.statusEffects.Remove("Freeze");
        }
        Destroy(this);
    }
}
