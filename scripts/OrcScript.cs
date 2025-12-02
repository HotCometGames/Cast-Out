using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcScript : MonoBehaviour
{
    public List<MultiTrade> possibleTrades;
    public List<Trade> trades;

    void Start()
    {
        int index = Random.Range(0, possibleTrades.Count);
        trades = possibleTrades[index].trades;
    }

}
