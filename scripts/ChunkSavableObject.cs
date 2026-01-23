using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkSavableObject : MonoBehaviour
{
    Vector2Int lastChunkCoord;
    public GameObject ObjectPrefab;
    void Start()
    {
        JoinChunk();
        lastChunkCoord = currentChunkCoord();
    }
    void Update()
    {
        Vector2Int chunkCoord = currentChunkCoord();
        if (chunkCoord != lastChunkCoord)
        {
            JoinChunk();
            lastChunkCoord = chunkCoord;
        }
    }
    Vector2Int currentChunkCoord()
    {
        float chunkWorldSize = WorldGeneration2.chunkSize * 2f; // 2f is the vertexSpacing default
        int chunkX = Mathf.FloorToInt(transform.position.x / chunkWorldSize);
        int chunkY = Mathf.FloorToInt(transform.position.z / chunkWorldSize);
        return new Vector2Int(chunkX, chunkY);
    }
    void JoinChunk()
    {
        Vector2Int chunkCoord = currentChunkCoord();
        GameObject chunkObj = WorldGeneration2.GetChunkObj(chunkCoord);
        if (chunkObj != null)
        {
            transform.parent = chunkObj.transform;
        } else
        {
            Debug.LogWarning("ChunkSavableObject could not find chunk at " + chunkCoord);
        }
    }
    public void OnChunkUnloaded()
    {
        if (this.gameObject.GetComponent<EnemyScript>() != null)
        {
            EnemyScript enemy = this.gameObject.GetComponent<EnemyScript>();
            enemy.OnChunkUnloaded();
        }
    }

    public void OnChunkLoaded()
    {
        if (this.gameObject.GetComponent<EnemyScript>() != null)
        {
            EnemyScript enemy = this.gameObject.GetComponent<EnemyScript>();
            enemy.OnChunkLoaded();
        }
    }
}
