using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFloatScript : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 0.25f;   // How high it moves up/down
    public float floatSpeed = 1f;        // How fast it floats

    [Header("Spinning")]
    public float spinSpeed = 45f;        // Degrees per second

    Vector3 startPos;


    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(transform.position.x, WorldGeneration2.GetHeight(transform.position.x, transform.position.z)+.75f, transform.position.z);
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPos + Vector3.up * yOffset;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}
