using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowmanScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float randomScale = Random.Range(.8f, 1.5f);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        float randomXRotation = Random.Range(-7f, 7f);
        float randomZRotation = Random.Range(-7f, 7f);
        float randomYRotation = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(randomXRotation, randomYRotation, randomZRotation);
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
