using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcScript : MonoBehaviour
{
    public List<MultiTrade> possibleTrades;
    public List<Trade> trades;
    [SerializeField] GameObject glasses;
    void Start()
    {
        int index = Random.Range(0, possibleTrades.Count);
        trades = possibleTrades[index].trades;

        int glassesChance = Random.Range(0, 100);
        if (glassesChance < 20) // 20% chance to spawn with glasses
        {
            glasses.SetActive(true);
        }
        else
        {
            glasses.SetActive(false);
        }
    }

}
