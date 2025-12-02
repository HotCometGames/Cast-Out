using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinSpawnScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float randomScale = Random.Range(500f, 1200f);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        float randomXRotation = Random.Range(-120f, -60f);
        float randomYRotation = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(randomXRotation, randomYRotation, 0f);
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
