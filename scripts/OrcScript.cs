using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcScript : MonoBehaviour
{
    public string traderName;
    public List<MultiTrade> possibleTrades;
    public List<Trade> trades;
    public List<Trade> AsaiahTrades;
    [SerializeField] GameObject glasses;
    void Start()
    {
        int index = Random.Range(0, possibleTrades.Count);
        trades = possibleTrades[index].trades;

        int glassesChance = Random.Range(0, 100);
        if (glassesChance < 20) // 20% chance to spawn with glasses
        {
            glasses.SetActive(true);
            traderName = "Asaiah the coolest";
            trades = AsaiahTrades;
        }
        else
        {
            glasses.SetActive(false);
        }
    }

}
