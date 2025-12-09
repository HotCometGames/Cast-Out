using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnGroundScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float height = WorldGeneration2.GetHeight(transform.position.x, transform.position.z);
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
        Destroy(this);
    }
}
