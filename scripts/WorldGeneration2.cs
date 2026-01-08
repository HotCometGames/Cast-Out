using System;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

public enum BiomeType
{
    WaterRocks,
    Ocean,
    Plains,
    Taiga,
    Mountain,
    LavaCast,
    Tundra,
    Desert,

}
[System.Serializable]
public class StructureData
{
    public GameObject prefab;
    public float spawnThreshold;
}

[System.Serializable]
public class NatureData
{
    public GameObject treePrefab;
    public float spawnThreshold;
    public bool isTree;
    public bool isItem;
}

[System.Serializable]
public class MobData
{
    public GameObject mobPrefab;
    public float spawnThreshold;
}

[System.Serializable]
public class BiomeData
{
    public BiomeType biomeType;
    public Texture2D texture;
    public NatureData[] natureDatas;
    public StructureData[] structureDatas;
    public MobData[] mobDatas;
    public int treeDominanceNeighbors = 5;
    public float biomeThreshold = 0.5f; // Used for biome selection
    public int textureIndex;
}

public class WorldGeneration2 : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] int setChunkSize = 64;
    static int chunkSize = 64;
    [SerializeField] float setVertexSpacing = 2f;
    static float vertexSpacing = 2f;
    [SerializeField] int setBlendEdge = 3;
    static int blendEdge = 3;
    [SerializeField] int setRenderDistance = 3;
    static int renderDistance = 3;

    [Header("Noise Settings")]
    [SerializeField] float setBaseNoiseScale = 0.005f;
    static float baseNoiseScale = 0.005f;
    [SerializeField] float setMediumNoiseScale = 0.02f;
    static float mediumNoiseScale = 0.02f;
    [SerializeField] float setSmallNoiseScale = 0.1f;
    static float smallNoiseScale = 0.1f;
    [SerializeField] float setBiomeTempNoiseScale = 0.002f;
    static float biomeTempNoiseScale = 0.002f;
    [SerializeField] float setBiomeDeciderNoiseScale = 0.0015f;
    static float biomeDeciderNoiseScale = 0.0015f;
    [SerializeField] float setTreeNoiseScale = 0.02f;
    static float treeNoiseScale = 0.02f;
    [SerializeField] float setStructureNoiseScale = 0.1f;
    static float structureNoiseScale = 0.1f;
    [SerializeField] float setMobNoiseScale = 0.05f;
    static float mobNoiseScale = 0.05f;
    [SerializeField] int setDistanceBetweenTrees = 3;
    static int distanceBetweenTrees = 3;

    [Header("Height Settings")]
    [SerializeField] float setBaseHeight = 20f;
    static float baseHeight = 20f;
    [SerializeField] float setMediumHeight = 8f;
    static float mediumHeight = 8f;
    [SerializeField] float setSmallHeight = 2f;
    static float smallHeight = 2f;

    [Header("Biome Settings")]
    [SerializeField] Material blendMaterial;
    [SerializeField] Material waterMaterial;
    [SerializeField] Material lavaMaterial;

    [Header("Biome Data")]
    [SerializeField] List<BiomeData> biomes; // Add all biome data here
    [SerializeField] List<float> setBiomeTempThresholds;
    static List<float> biomeTempThresholds; // Corresponding thresholds for biomes. hot - neutral - cold
    [SerializeField] List<BiomeData> setHotBiomes;
    static List<BiomeData> hotBiomes;
    [SerializeField] List<BiomeData> setNeutralBiomes;
    static List<BiomeData> neutralBiomes;
    [SerializeField] List<BiomeData> setColdBiomes;
    static List<BiomeData> coldBiomes;

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] GameObject spawnLight;

    [Header("Seed Settings")]
    public int seed = 0;

    private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();

    static float baseOffsetX, baseOffsetZ;
    static float mediumOffsetX, mediumOffsetZ;
    static float smallOffsetX, smallOffsetZ;
    static float biomeTempOffsetX, biomeTempOffsetZ;
    static float biomeDeciderOffsetX, biomeDeciderOffsetZ;
    static float treeOffsetX, treeOffsetZ;
    static float treeOffsetX2, treeOffsetZ2;
    static float structureOffsetX, structureOffsetZ;
    static float mobOffsetX, mobOffsetZ;
    private bool setOffsets = false;

    void Awake()
    {
        chunkSize = setChunkSize;
        vertexSpacing = setVertexSpacing;
        blendEdge = setBlendEdge;
        renderDistance = setRenderDistance;

        baseNoiseScale = setBaseNoiseScale;
        mediumNoiseScale = setMediumNoiseScale;
        smallNoiseScale = setSmallNoiseScale;
        biomeTempNoiseScale = setBiomeTempNoiseScale;
        biomeDeciderNoiseScale = setBiomeDeciderNoiseScale;
        treeNoiseScale = setTreeNoiseScale;
        structureNoiseScale = setStructureNoiseScale;
        mobNoiseScale = setMobNoiseScale;
        distanceBetweenTrees = setDistanceBetweenTrees;

        baseHeight = setBaseHeight;
        mediumHeight = setMediumHeight;
        smallHeight = setSmallHeight;

        biomeTempThresholds = setBiomeTempThresholds;
        hotBiomes = setHotBiomes;
        neutralBiomes = setNeutralBiomes;
        coldBiomes = setColdBiomes;



        System.Random rand = new System.Random(seed);
        baseOffsetX         = NextNoiseOffset(rand);
        baseOffsetZ         = NextNoiseOffset(rand);
        mediumOffsetX       = NextNoiseOffset(rand);
        mediumOffsetZ       = NextNoiseOffset(rand);
        smallOffsetX        = NextNoiseOffset(rand);
        smallOffsetZ        = NextNoiseOffset(rand);
        biomeTempOffsetX    = NextNoiseOffset(rand);
        biomeTempOffsetZ    = NextNoiseOffset(rand);
        biomeDeciderOffsetX = NextNoiseOffset(rand);
        biomeDeciderOffsetZ = NextNoiseOffset(rand);
        treeOffsetX         = NextNoiseOffset(rand);
        treeOffsetZ         = NextNoiseOffset(rand);
        treeOffsetX2        = NextNoiseOffset(rand);
        treeOffsetZ2        = NextNoiseOffset(rand);
        structureOffsetX    = NextNoiseOffset(rand);
        structureOffsetZ = NextNoiseOffset(rand);
        mobOffsetX = NextNoiseOffset(rand);
        mobOffsetZ = NextNoiseOffset(rand);
        
        InitializeTextures();

        Vector3 spawnPos = SetSpawn();
        player.position = spawnPos + new Vector3(1, 1, 0);
        setOffsets = true;
    }

    void Update()
    {
        if (player == null) return;
        if (!setOffsets) return;

        Vector2Int playerChunk = WorldToChunk(player.position);
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);
                if (!loadedChunks.ContainsKey(chunkCoord))
                {
                    GenerateChunk(chunkCoord);
                }
            }
        }
        // Unload chunks later
    }
    
    // Initializes the biome textures in the blend material
    void InitializeTextures()
    {
        if (blendMaterial == null) return;

        blendMaterial.SetTexture("_MainTex", null);

        // Set hot biome textures
        foreach (var biome in hotBiomes)
        {
            if (biome.texture != null)
            {
                string textureName = $"_{biome.biomeType}Tex";
                blendMaterial.SetTexture(textureName, biome.texture);
            }
        }

        // Set neutral biome textures
        foreach (var biome in neutralBiomes)
        {
            if (biome.texture != null)
            {
                string textureName = $"_{biome.biomeType}Tex";
                blendMaterial.SetTexture(textureName, biome.texture);
            }
        }

        // Set cold biome textures
        foreach (var biome in coldBiomes)
        {
            if (biome.texture != null)
            {
                string textureName = $"_{biome.biomeType}Tex";
                blendMaterial.SetTexture(textureName, biome.texture);
            }
        }
    }

    // Converts world position to chunk coordinates
    Vector2Int WorldToChunk(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / (chunkSize * vertexSpacing));
        int z = Mathf.FloorToInt(position.z / (chunkSize * vertexSpacing));
        return new Vector2Int(x, z);
    }

    // Random num generation based of seeds
    private float NextNoiseOffset(System.Random r)
    {
        return (float)(r.NextDouble() * 256.0);
    }

    // Generates a chunk GameObject with mesh, materials, and other components
    void GenerateChunk(Vector2Int chunkCoord)
    {
        GameObject chunkObj = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObj.transform.position = new Vector3(
            chunkCoord.x * chunkSize * vertexSpacing,
            0,
            chunkCoord.y * chunkSize * vertexSpacing
        );
        chunkObj.transform.parent = transform;

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        MeshCollider mc = chunkObj.AddComponent<MeshCollider>();

        Mesh mesh;
        Material mat;
        int[] biomeIndices;
        Vector3[] vertices;
        GenerateMesh(chunkCoord, out mesh, out mat, out biomeIndices, out vertices);

        mf.mesh = mesh;
        mc.sharedMesh = mesh;
        mr.material = mat;

        loadedChunks.Add(chunkCoord, chunkObj);

        SpawnVegetation(chunkObj, chunkCoord, biomeIndices, vertices);
        GenerateWater(chunkObj, chunkCoord, biomeIndices);
        GenerateLava(chunkObj, chunkCoord, biomeIndices);
        SpawnStructures(chunkObj, chunkCoord, biomeIndices);
        //SpawnMobs(chunkObj, chunkCoord, biomeIndices);
    }

    // Generates the mesh for the chunk at the given chunk coordinates
    void GenerateMesh(Vector2Int chunkCoord, out Mesh mesh, out Material mat, out int[] biomeIndices, out Vector3[] vertices)
    {
        int vertsPerLine = chunkSize + 1;
        vertices = new Vector3[vertsPerLine * vertsPerLine];
        int[] triangles = new int[chunkSize * chunkSize * 6];
        Vector2[] uvs = new Vector2[vertices.Length];
        biomeIndices = new int[vertices.Length];
        Color32[] biomeColorIndices = new Color32[vertices.Length];

        float[,] heights = new float[vertsPerLine, vertsPerLine];
        float[,] treeNoiseMap = new float[vertsPerLine, vertsPerLine];
        float[,] treeNoiseMap2 = new float[vertsPerLine, vertsPerLine];

        for (int z = 0; z < vertsPerLine; z++)
        {
            for (int x = 0; x < vertsPerLine; x++)
            {
                float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;

                float height = GetHeight(worldX, worldZ);

                heights[x, z] = height;

                (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);

                biomeIndices[z * vertsPerLine + x] = biomeData.textureIndex;
                biomeColorIndices[z * vertsPerLine + x] = new Color32((byte)biomeData.textureIndex, 0, 0, 255);

                treeNoiseMap[x, z] = Mathf.PerlinNoise((worldX * treeNoiseScale) + treeOffsetX, (worldZ * treeNoiseScale) + treeOffsetZ);
                treeNoiseMap2[x, z] = Mathf.PerlinNoise((worldX * treeNoiseScale) + treeOffsetX2, (worldZ * treeNoiseScale) + treeOffsetZ2);
            }
        }

        BlendChunkEdges(chunkCoord, heights);

        //ChatGPT Generated Code
        for (int z = 0; z < vertsPerLine; z++)
        {
            for (int x = 0; x < vertsPerLine; x++)
            {
                int i = z * vertsPerLine + x;
                vertices[i] = new Vector3(x * vertexSpacing, heights[x, z], z * vertexSpacing);
                uvs[i] = new Vector2(x * 0.2f, z * 0.2f);
            }
        }

        //ChatGPT Generated Code
        int triIndex = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int i = z * vertsPerLine + x;
                triangles[triIndex++] = i;
                triangles[triIndex++] = i + vertsPerLine;
                triangles[triIndex++] = i + 1;

                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + vertsPerLine;
                triangles[triIndex++] = i + vertsPerLine + 1;
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors32 = biomeColorIndices;
        mesh.RecalculateNormals();

        mat = blendMaterial;

        chunkObjTreeNoiseMap = treeNoiseMap;
        chunkObjTreeNoiseMap2 = treeNoiseMap2;
    }

    //Blends the edges of the chunk with neighboring chunks to ensure smooth transitions
    void BlendChunkEdges(Vector2Int chunkCoord, float[,] heights)
    {
        int vertsPerLine = chunkSize + 1;
        for (int z = 0; z < vertsPerLine; z++)
        {
            for (int b = 0; b < blendEdge; b++)
            {
                Vector2Int neighborCoord = new Vector2Int(chunkCoord.x - 1, chunkCoord.y);
                if (loadedChunks.TryGetValue(neighborCoord, out GameObject neighbor))
                {
                    MeshFilter mf = neighbor.GetComponent<MeshFilter>();
                    if (mf && mf.mesh)
                    {
                        Vector3[] neighborVerts = mf.mesh.vertices;
                        int neighborVertsPerLine = chunkSize + 1;
                        heights[b, z] = Mathf.Lerp(heights[b, z], neighborVerts[z * neighborVertsPerLine + chunkSize].y, (float)(blendEdge - b) / blendEdge);
                    }
                }
                neighborCoord = new Vector2Int(chunkCoord.x + 1, chunkCoord.y);
                if (loadedChunks.TryGetValue(neighborCoord, out neighbor))
                {
                    MeshFilter mf = neighbor.GetComponent<MeshFilter>();
                    if (mf && mf.mesh)
                    {
                        Vector3[] neighborVerts = mf.mesh.vertices;
                        int neighborVertsPerLine = chunkSize + 1;
                        heights[chunkSize - b, z] = Mathf.Lerp(heights[chunkSize - b, z], neighborVerts[z * neighborVertsPerLine].y, (float)(blendEdge - b) / blendEdge);
                    }
                }
            }
        }
        for (int x = 0; x < vertsPerLine; x++)
        {
            for (int b = 0; b < blendEdge; b++)
            {
                Vector2Int neighborCoord = new Vector2Int(chunkCoord.x, chunkCoord.y - 1);
                if (loadedChunks.TryGetValue(neighborCoord, out GameObject neighbor))
                {
                    MeshFilter mf = neighbor.GetComponent<MeshFilter>();
                    if (mf && mf.mesh)
                    {
                        Vector3[] neighborVerts = mf.mesh.vertices;
                        int neighborVertsPerLine = chunkSize + 1;
                        heights[x, b] = Mathf.Lerp(heights[x, b], neighborVerts[chunkSize * neighborVertsPerLine + x].y, (float)(blendEdge - b) / blendEdge);
                    }
                }
                neighborCoord = new Vector2Int(chunkCoord.x, chunkCoord.y + 1);
                if (loadedChunks.TryGetValue(neighborCoord, out neighbor))
                {
                    MeshFilter mf = neighbor.GetComponent<MeshFilter>();
                    if (mf && mf.mesh)
                    {
                        Vector3[] neighborVerts = mf.mesh.vertices;
                        int neighborVertsPerLine = chunkSize + 1;
                        heights[x, chunkSize - b] = Mathf.Lerp(heights[x, chunkSize - b], neighborVerts[x].y, (float)(blendEdge - b) / blendEdge);
                    }
                }
            }
        }
    }

    private float[,] chunkObjTreeNoiseMap;
    private float[,] chunkObjTreeNoiseMap2;

    // Spawns vegetation (trees, plants) on the chunk based on biome and noise
    void SpawnVegetation(GameObject chunkObj, Vector2Int chunkCoord, int[] biomeIndices, Vector3[] vertices)
    {
        int vertsPerLine = chunkSize + 1;
        var treeCandidates = new List<(int index, int natureDataIndex, float noiseSum, int biomeIndex, BiomeData biomeData)>();

        for (int z = 0; z < vertsPerLine; z += distanceBetweenTrees)
        {
            for (int x = 0; x < vertsPerLine; x += distanceBetweenTrees)
            {
                float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;
                float structureNoise = Mathf.PerlinNoise((worldX * structureNoiseScale) + structureOffsetX, (worldZ * structureNoiseScale) + structureOffsetZ);

                int i = z * vertsPerLine + x;
                int biomeIndex = biomeIndices[i];
                (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);

                if (biomeData.structureDatas.Length > 0)
                {
                    if (structureNoise > biomeData.structureDatas[biomeData.structureDatas.Length - 1].spawnThreshold)
                    {
                        continue;
                    }
                }

                float treeNoise = chunkObjTreeNoiseMap[x, z];
                float treeNoise2 = chunkObjTreeNoiseMap2[x, z];
                float noiseSum = treeNoise + treeNoise2;


                NatureData[] natureDatas = biomeData.natureDatas;

                for(int j = 0; j < natureDatas.Length; j++)
                {
                    float aboveThreshold = 1f;
                    if (j != 0)
                    {
                        aboveThreshold = natureDatas[j - 1].spawnThreshold;
                    }
                    bool passesThreshold = aboveThreshold > treeNoise && treeNoise > natureDatas[j].spawnThreshold && aboveThreshold > treeNoise2 && treeNoise2 > natureDatas[j].spawnThreshold;

                    if (passesThreshold)
                    {

                        if (natureDatas[j].isTree)
                        {
                            treeCandidates.Add((i, j, noiseSum, biomeIndex, biomeData));
                        } else
                        {
                            float heightAdd = 0;
                            if (natureDatas[j].isItem)
                            {
                                heightAdd = 0.2f;
                            }
                            Vector3 pos = chunkObj.transform.position + vertices[i] + Vector3.up * heightAdd;
                            GameObject prefabToSpawn = natureDatas[j].treePrefab;

                            if (prefabToSpawn != null)
                            {
                                Instantiate(prefabToSpawn, pos, prefabToSpawn.transform.rotation, chunkObj.transform);
                            }
                        }
                    }
                }
            }
        }

        float[,] noiseMap = new float[vertsPerLine, vertsPerLine];
        foreach (var candidate in treeCandidates)
        {
            int i = candidate.index;
            int x = i % vertsPerLine;
            int z = i / vertsPerLine;
            noiseMap[x, z] = candidate.noiseSum;
        }

        var finalTrees = new List<(int index, int natureDataIndex, BiomeData biomeData)>();
        foreach (var candidate in treeCandidates)
        {
            int i = candidate.index;
            int j = candidate.natureDataIndex;
            int x = i % vertsPerLine;
            int z = i / vertsPerLine;
            float myNoise = candidate.noiseSum;
            int dominatedCount = 0;

            for (int dz = -distanceBetweenTrees; dz <= distanceBetweenTrees; dz++)
            {
                for (int dx = -distanceBetweenTrees; dx <= distanceBetweenTrees; dx++)
                {
                    if (dx == 0 && dz == 0) continue;
                    if (Mathf.Abs(dx) == distanceBetweenTrees && Mathf.Abs(dz) == distanceBetweenTrees) continue;
                    int nx = x + dx;
                    int nz = z + dz;
                    if (nx < 0 || nx >= vertsPerLine || nz < 0 || nz >= vertsPerLine) continue;
                    if (noiseMap[nx, nz] > myNoise)
                    {
                        dominatedCount++;
                    }
                }
            }
            if (dominatedCount > candidate.biomeData.treeDominanceNeighbors)
                finalTrees.Add((i, j, candidate.biomeData));
        }

        foreach (var tree in finalTrees)
        {
            Vector3 pos = chunkObj.transform.position + vertices[tree.index];
            GameObject prefabToSpawn = tree.biomeData.natureDatas[tree.natureDataIndex].treePrefab;

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, pos, prefabToSpawn.transform.rotation, chunkObj.transform);
            }
        }
    }

    // Generates lava mesh in lava cast biomes
    void GenerateLava(GameObject chunkObj, Vector2Int chunkCoord, int[] biomeIndices)
    {
        int vertsPerLine = chunkSize + 1;
        List<Vector3> lavaVertices = new List<Vector3>();
        List<int> lavaTriangles = new List<int>();
        List<Vector2> lavaUVs = new List<Vector2>();

        int[,] vertexMap = new int[vertsPerLine, vertsPerLine];
        int vertCount = 0;

        for (int z = 0; z < vertsPerLine; z++)
        {
            for (int x = 0; x < vertsPerLine; x++)
            {
                int i = z * vertsPerLine + x;
                int biomeIndex = biomeIndices[i];
                BiomeType biomeType = GetBiomeTypeFromIndex(biomeIndex);
                BiomeType biomeTypeUp = (z < vertsPerLine - 1) ? GetBiomeTypeFromIndex(biomeIndices[(z + 1) * vertsPerLine + x]) : biomeType;
                BiomeType biomeTypeDown = (z > 0) ? GetBiomeTypeFromIndex(biomeIndices[(z - 1) * vertsPerLine + x]) : biomeType;
                BiomeType biomeTypeLeft = (x > 0) ? GetBiomeTypeFromIndex(biomeIndices[z * vertsPerLine + (x - 1)]) : biomeType;
                BiomeType biomeTypeRight = (x < vertsPerLine - 1) ? GetBiomeTypeFromIndex(biomeIndices[z * vertsPerLine + (x + 1)]) : biomeType;

                if (biomeType == BiomeType.LavaCast || 
                    biomeTypeUp == BiomeType.LavaCast ||
                    biomeTypeDown == BiomeType.LavaCast ||
                    biomeTypeLeft == BiomeType.LavaCast ||
                    biomeTypeRight == BiomeType.LavaCast)
                {
                    float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                    float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;

                    float baseNoise = Mathf.PerlinNoise((worldX * baseNoiseScale) + baseOffsetX, (worldZ * baseNoiseScale) + baseOffsetZ) * baseHeight;
                    float mediumNoise = Mathf.PerlinNoise((worldX * mediumNoiseScale) + mediumOffsetX, (worldZ * mediumNoiseScale) + mediumOffsetZ) * mediumHeight;
                    float smallNoise = Mathf.PerlinNoise((worldX * smallNoiseScale) + smallOffsetX, (worldZ * smallNoiseScale) + smallOffsetZ) * smallHeight;

                    float lavaHeight = (baseNoise + mediumNoise + smallNoise) * 0.8f;

                    lavaVertices.Add(new Vector3(x * vertexSpacing, lavaHeight, z * vertexSpacing));
                    lavaUVs.Add(new Vector2(x * 0.2f, z * 0.2f));
                    vertexMap[x, z] = vertCount;
                    vertCount++;
                }
                else
                {
                    vertexMap[x, z] = -1;
                }
            }
        }

        //ChatGPT Generated Code
        // Generate triangles for lava mesh
        for (int z = 0; z < vertsPerLine - 1; z++)
        {
            for (int x = 0; x < vertsPerLine - 1; x++)
            {
                int i00 = vertexMap[x, z];
                int i10 = vertexMap[x + 1, z];
                int i01 = vertexMap[x, z + 1];
                int i11 = vertexMap[x + 1, z + 1];

                // Only create triangles if all four vertices exist (are water)
                if (i00 != -1 && i10 != -1 && i01 != -1)
                {
                    lavaTriangles.Add(i00);
                    lavaTriangles.Add(i01);
                    lavaTriangles.Add(i10);
                }
                if (i10 != -1 && i01 != -1 && i11 != -1)
                {
                    lavaTriangles.Add(i10);
                    lavaTriangles.Add(i01);
                    lavaTriangles.Add(i11);
                }
            }
        }

        if (lavaVertices.Count > 0)
        {
            Mesh lavaMesh = new Mesh();
            lavaMesh.vertices = lavaVertices.ToArray();
            lavaMesh.triangles = lavaTriangles.ToArray();
            lavaMesh.uv = lavaUVs.ToArray();
            lavaMesh.RecalculateNormals();

            GameObject lavaObj = new GameObject("Lava");
            lavaObj.transform.parent = chunkObj.transform;
            lavaObj.transform.localPosition = Vector3.zero;

            MeshFilter mf = lavaObj.AddComponent<MeshFilter>();
            MeshRenderer mr = lavaObj.AddComponent<MeshRenderer>();
            mf.mesh = lavaMesh;

            // Assign a lava material if you have one, otherwise use blendMaterial
            mr.material = lavaMaterial;

            lavaObj.tag = "Lava";

            MeshCollider mc = lavaObj.AddComponent<MeshCollider>();
            mc.sharedMesh = lavaMesh;
            mc.convex = true;
            mc.isTrigger = true;
        }
    }

    // Generates water mesh in water biomes
    void GenerateWater(GameObject chunkObj, Vector2Int chunkCoord, int[] biomeIndices)
    {
        int vertsPerLine = chunkSize + 1;
        List<Vector3> waterVertices = new List<Vector3>();
        List<int> waterTriangles = new List<int>();
        List<Vector2> waterUVs = new List<Vector2>();

        int[,] vertexMap = new int[vertsPerLine, vertsPerLine];
        int vertCount = 0;

        for (int z = 0; z < vertsPerLine; z++)
        {
            for (int x = 0; x < vertsPerLine; x++)
            {
                int i = z * vertsPerLine + x;
                int biomeIndex = biomeIndices[i];
                BiomeType biomeType = GetBiomeTypeFromIndex(biomeIndex);
                BiomeType biomeTypeUp = (z < vertsPerLine - 1) ? GetBiomeTypeFromIndex(biomeIndices[(z + 1) * vertsPerLine + x]) : biomeType;
                BiomeType biomeTypeDown = (z > 0) ? GetBiomeTypeFromIndex(biomeIndices[(z - 1) * vertsPerLine + x]) : biomeType;
                BiomeType biomeTypeLeft = (x > 0) ? GetBiomeTypeFromIndex(biomeIndices[z * vertsPerLine + (x - 1)]) : biomeType;
                BiomeType biomeTypeRight = (x < vertsPerLine - 1) ? GetBiomeTypeFromIndex(biomeIndices[z * vertsPerLine + (x + 1)]) : biomeType;

                if (biomeType == BiomeType.Ocean || biomeType == BiomeType.WaterRocks ||
                    biomeTypeUp == BiomeType.Ocean || biomeTypeUp == BiomeType.WaterRocks ||
                    biomeTypeDown == BiomeType.Ocean || biomeTypeDown == BiomeType.WaterRocks ||
                    biomeTypeLeft == BiomeType.Ocean || biomeTypeLeft == BiomeType.WaterRocks ||
                    biomeTypeRight == BiomeType.Ocean || biomeTypeRight == BiomeType.WaterRocks)
                {
                    float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                    float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;

                    float baseNoise = Mathf.PerlinNoise((worldX * baseNoiseScale) + baseOffsetX, (worldZ * baseNoiseScale) + baseOffsetZ) * baseHeight;
                    float mediumNoise = Mathf.PerlinNoise((worldX * mediumNoiseScale) + mediumOffsetX, (worldZ * mediumNoiseScale) + mediumOffsetZ) * mediumHeight;
                    float smallNoise = Mathf.PerlinNoise((worldX * smallNoiseScale) + smallOffsetX, (worldZ * smallNoiseScale) + smallOffsetZ) * smallHeight;

                    float waterHeight = (baseNoise + mediumNoise + smallNoise) * 0.8f;

                    waterVertices.Add(new Vector3(x * vertexSpacing, waterHeight, z * vertexSpacing));
                    waterUVs.Add(new Vector2(x * 0.2f, z * 0.2f));
                    vertexMap[x, z] = vertCount;
                    vertCount++;
                }
                else
                {
                    vertexMap[x, z] = -1;
                }
            }
        }

        // ChatGPT Generated Code
        // Generate triangles for water mesh
        for (int z = 0; z < vertsPerLine - 1; z++)
        {
            for (int x = 0; x < vertsPerLine - 1; x++)
            {
                int i00 = vertexMap[x, z];
                int i10 = vertexMap[x + 1, z];
                int i01 = vertexMap[x, z + 1];
                int i11 = vertexMap[x + 1, z + 1];

                // Only create triangles if all four vertices exist (are water)
                if (i00 != -1 && i10 != -1 && i01 != -1)
                {
                    waterTriangles.Add(i00);
                    waterTriangles.Add(i01);
                    waterTriangles.Add(i10);
                }
                if (i10 != -1 && i01 != -1 && i11 != -1)
                {
                    waterTriangles.Add(i10);
                    waterTriangles.Add(i01);
                    waterTriangles.Add(i11);
                }
            }
        }

        if (waterVertices.Count > 0)
        {
            Mesh waterMesh = new Mesh();
            waterMesh.vertices = waterVertices.ToArray();
            waterMesh.triangles = waterTriangles.ToArray();
            waterMesh.uv = waterUVs.ToArray();
            waterMesh.RecalculateNormals();

            GameObject waterObj = new GameObject("Water");
            waterObj.transform.parent = chunkObj.transform;
            waterObj.transform.localPosition = Vector3.zero;

            MeshFilter mf = waterObj.AddComponent<MeshFilter>();
            MeshRenderer mr = waterObj.AddComponent<MeshRenderer>();
            mf.mesh = waterMesh;

            
            mr.material = waterMaterial;
        }
    }

    // Spawns structures on the chunk based on biome and noise
    void SpawnStructures(GameObject chunkObj, Vector2Int chunkCoord, int[] biomeIndices)
    {
        int vertsPerLine = chunkSize + 1;
        for (int z = 0; z < vertsPerLine; z += 10)
        {
            for (int x = 0; x < vertsPerLine; x += 10)
            {
                int i = z * vertsPerLine + x;
                int biomeIndex = biomeIndices[i];
                BiomeType biomeType = GetBiomeTypeFromIndex(biomeIndex);
                float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;
                float height = GetHeight(worldX, worldZ);

                float structureNoise = Mathf.PerlinNoise((worldX * structureNoiseScale) + structureOffsetX, (worldZ * structureNoiseScale) + structureOffsetZ);

                (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);

                for (int j = 0; j < biomeData.structureDatas.Length; j++)
                {
                    float aboveThreshold = 1f;
                    if (j != 0)
                    {
                        aboveThreshold = biomeData.structureDatas[j - 1].spawnThreshold;
                    }
                    if (aboveThreshold > structureNoise && structureNoise > biomeData.structureDatas[j].spawnThreshold &&
                    biomeData.structureDatas[j].prefab != null)
                    {
                        Vector3 pos = chunkObj.transform.position + new Vector3(x * vertexSpacing, height, z * vertexSpacing);
                        if (biomeType != BiomeType.LavaCast)
                        {
                            Instantiate(biomeData.structureDatas[j].prefab, pos, Quaternion.identity, chunkObj.transform);
                        }
                        else
                        {
                            float mediumNoise = Mathf.PerlinNoise((worldX * mediumNoiseScale) + mediumOffsetX, (worldZ * mediumNoiseScale) + mediumOffsetZ) * mediumHeight;
                            if (0.6f < mediumNoise / mediumHeight || mediumNoise / mediumHeight < 0.2f)
                            {
                                Instantiate(biomeData.structureDatas[j].prefab, pos, Quaternion.identity, chunkObj.transform);
                            }
                        }
                    }
                }
            }
        }
    }

    // Spawns mobs on the chunk based on biome and noise
    void SpawnMobs(GameObject chunkObj, Vector2Int chunkCoord, int[] biomeIndices)
    {
        int vertsPerLine = chunkSize + 1;
        for (int z = 0; z < vertsPerLine; z += 10)
        {
            for (int x = 0; x < vertsPerLine; x += 10)
            {
                int i = z * vertsPerLine + x;
                int biomeIndex = biomeIndices[i];
                BiomeType biomeType = GetBiomeTypeFromIndex(biomeIndex);
                float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;
                float height = GetHeight(worldX, worldZ);

                float mobNoise = Mathf.PerlinNoise((worldX * mobNoiseScale) + mobOffsetX, (worldZ * mobNoiseScale) + mobOffsetZ);

                (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);

                for (int j = 0; j < biomeData.mobDatas.Length; j++)
                {
                    float aboveThreshold = 1f;
                    if (j != 0)
                    {
                        aboveThreshold = biomeData.mobDatas[j - 1].spawnThreshold;
                    }
                    if (aboveThreshold > mobNoise && mobNoise > biomeData.mobDatas[j].spawnThreshold &&
                    biomeData.mobDatas[j].mobPrefab != null)
                    {
                        Vector3 pos = chunkObj.transform.position + new Vector3(x * vertexSpacing, height+biomeData.mobDatas[j].mobPrefab.GetComponent<EnemyScript>().height, z * vertexSpacing);
                        Instantiate(biomeData.mobDatas[j].mobPrefab, pos, Quaternion.identity, chunkObj.transform);
                    }
                }
            }
        }
    }
    
    // Calculates the height of the terrain at a given world position
    public static float GetHeight(float worldX, float worldZ)
    {
        float baseNoise = Mathf.PerlinNoise((worldX * baseNoiseScale) + baseOffsetX, (worldZ * baseNoiseScale) + baseOffsetZ) * baseHeight;
        float mediumNoise = Mathf.PerlinNoise((worldX * mediumNoiseScale) + mediumOffsetX, (worldZ * mediumNoiseScale) + mediumOffsetZ) * mediumHeight;
        float smallNoise = Mathf.PerlinNoise((worldX * smallNoiseScale) + smallOffsetX, (worldZ * smallNoiseScale) + smallOffsetZ) * smallHeight;

        float height = baseNoise + mediumNoise + smallNoise;

        float tempVal = Mathf.PerlinNoise((worldX * biomeTempNoiseScale) + biomeTempOffsetX, (worldZ * biomeTempNoiseScale) + biomeTempOffsetZ);

        (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);


        if (biomeData.biomeType == BiomeType.Mountain)
        {
            float lowestBiomeThreshold = Math.Min(biomeDeciderVal - coldBiomes[coldBiomes.Count - 2].biomeThreshold, tempVal - biomeTempThresholds[biomeTempThresholds.Count - 2]);
            height += Mathf.Pow(lowestBiomeThreshold * 40f, 2f);
            float possibleNewHeight = Mathf.Pow(height * Mathf.Pow(lowestBiomeThreshold * 1.1f, 1.7f) * 4, 1.7f);
            if (possibleNewHeight > height)
                height = possibleNewHeight;
        }
        if (biomeData.biomeType == BiomeType.Ocean)
        {
            height *= 0.5f;
        }
        if (biomeData.biomeType == BiomeType.WaterRocks)
        {
            height *= 0.1f;
            height = Mathf.Pow(height, 6);
        }
        if (biomeData.biomeType == BiomeType.LavaCast)
        {
            if(biomeDeciderVal - hotBiomes[biomeDeciderIndex-1].biomeThreshold < 0.005f || biomeTempThresholds[0] - tempVal < 0.01f)
            {
                return height;
            }
            if (mediumNoise / mediumHeight > 0.95f)
            {
                height += 45f;
            }
            else if (mediumNoise / mediumHeight > 0.75f)
            {
                height += 30f;
            }
            else if (mediumNoise / mediumHeight > 0.6f)
            {
                height += 15f;
            }
            else if (mediumNoise / mediumHeight > 0.2f)
            {
                height *= .6f;
            }
        }
        return height;
    }

    // Determines the temperature category of a biome at a given world position
    public static string GetBiomeTemp(float worldX, float worldZ)
    {
        float tempVal = Mathf.PerlinNoise((worldX * biomeTempNoiseScale) + biomeTempOffsetX, (worldZ * biomeTempNoiseScale) + biomeTempOffsetZ);
        if (tempVal < biomeTempThresholds[0])
        {
            return "hot";
        }
        else if (tempVal < biomeTempThresholds[1])
        {
            return "neutral";
        }
        else
        {
            return "cold";
        }
    }

    // Determines the biome type at a given world position
    public static (BiomeData, float, int) GetBiomeType(float worldX, float worldZ)
    {
        string biomeTemp = GetBiomeTemp(worldX, worldZ);
        List<BiomeData> biomeUsing = hotBiomes;
        switch (biomeTemp)
        {
            case "hot":
                biomeUsing = hotBiomes;
                break;
            case "neutral":
                biomeUsing = neutralBiomes;
                break;
            case "cold":
                biomeUsing = coldBiomes;
                break;
        }
        float biomeDeciderVal = Mathf.PerlinNoise((worldX * biomeDeciderNoiseScale) + biomeDeciderOffsetX, (worldZ * biomeDeciderNoiseScale) + biomeDeciderOffsetZ);
        int biomeDeciderIndex = 0;
        for (int b = 0; b < biomeUsing.Count; b++)
        {
            if (biomeDeciderVal < biomeUsing[b].biomeThreshold)
            {
                biomeDeciderIndex = b;
                break;
            }
        }
        return (biomeUsing[biomeDeciderIndex], biomeDeciderVal, biomeDeciderIndex);
    }

    public static bool PosHasLava(float worldX, float worldZ)
    {
        (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);
        if (biomeData.biomeType == BiomeType.LavaCast)
        {
            float mediumNoise = Mathf.PerlinNoise((worldX * mediumNoiseScale) + mediumOffsetX, (worldZ * mediumNoiseScale) + mediumOffsetZ) * mediumHeight;
            if (0.6f < mediumNoise / mediumHeight && mediumNoise / mediumHeight < 0.2f)
                return true;
        }
        return false;
    }

    public static MobData GetMobDataAtPosition(float worldX, float worldZ)
    {
        (BiomeData biomeData, float biomeDeciderVal, int biomeDeciderIndex) = GetBiomeType(worldX, worldZ);
        float mobNoise = Mathf.PerlinNoise((worldX * mobNoiseScale) + mobOffsetX, (worldZ * mobNoiseScale) + mobOffsetZ);
        for (int j = 0; j < biomeData.mobDatas.Length; j++)
                {
                    float aboveThreshold = 1f;
                    if (j != 0)
                    {
                        aboveThreshold = biomeData.mobDatas[j - 1].spawnThreshold;
                    }
                    if (aboveThreshold > mobNoise && mobNoise > biomeData.mobDatas[j].spawnThreshold &&
                    biomeData.mobDatas[j].mobPrefab != null)
                    {
                        return biomeData.mobDatas[j];
                    }
                }
        return biomeData.mobDatas[0];
    }

    // Maps biome index to BiomeType enum
    BiomeType GetBiomeTypeFromIndex(int index)
    {
        switch(index)
        {
            case 0:
                return BiomeType.WaterRocks;
            case 1:
                return BiomeType.Ocean;
            case 2:
                return BiomeType.Plains;
            case 3:
                return BiomeType.Taiga;
            case 4:
                return BiomeType.LavaCast;
            case 5:
                return BiomeType.Mountain;
            case 6:
                return BiomeType.Tundra;
            case 7:
                return BiomeType.Desert;
            default:
                return BiomeType.Plains;
        }
    }

    // Sets the spawn position in the world
    Vector3 SetSpawn()
    {
        int vertsPerLine = chunkSize + 1;
        Vector2 chunkCoord = new Vector2(0, 0);
        int chunkX = 0;
        Vector3 spawnPosition = Vector3.zero;
        while (true)
        {
            chunkCoord = new Vector2(chunkX, 0);
            // Your spawning logic here
            for (int z = 0; z < vertsPerLine; z++)
            {
                for (int x = 0; x < vertsPerLine; x++)
                {
                    float worldX = (chunkCoord.x * chunkSize + x) * vertexSpacing;
                    float worldZ = (chunkCoord.y * chunkSize + z) * vertexSpacing;
                    float height = GetHeight(worldX, worldZ);

                    (BiomeData biomeData, float biomeDeciderVal, int biomeIndex) = GetBiomeType(worldX, worldZ);
                    float structureNoise = Mathf.PerlinNoise((worldX * structureNoiseScale) + structureOffsetX, (worldZ * structureNoiseScale) + structureOffsetZ);

                    float treeNoiseMapVal = Mathf.PerlinNoise((worldX * treeNoiseScale) + treeOffsetX, (worldZ * treeNoiseScale) + treeOffsetZ);
                    float treeNoiseMapVal2 = Mathf.PerlinNoise((worldX * treeNoiseScale) + treeOffsetX2, (worldZ * treeNoiseScale) + treeOffsetZ2);

                    if (biomeData.structureDatas.Length > 0)
                    {
                        if (structureNoise > biomeData.structureDatas[biomeData.structureDatas.Length - 1].spawnThreshold)
                        {
                            continue; // Skip spawn if structure is spawned here
                        }
                    }
                    if (biomeData.biomeType == BiomeType.LavaCast || biomeData.biomeType == BiomeType.Ocean || biomeData.biomeType == BiomeType.WaterRocks)
                    {
                        continue; // Skip lava biome
                    }

                    spawnPosition = new Vector3(worldX, height, worldZ);
                    Instantiate(spawnLight, spawnPosition, Quaternion.identity);
                    return spawnPosition;

                }
            }
            chunkX++;
        }
    }
}
