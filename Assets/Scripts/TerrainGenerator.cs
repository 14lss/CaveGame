using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject TerrainChunk;
    public Transform Player;

    public int RenderDistance = 12;

    public Dictionary<Vector2, GameObject> Chunks = new Dictionary<Vector2, GameObject>();

    private void Update()
    {
        int CurrentChunkX = Mathf.FloorToInt(Player.position.x / ChunkGenerator.ChunkSize.x);
        int CurrentChunkZ = Mathf.FloorToInt(Player.position.z / ChunkGenerator.ChunkSize.z);

        Vector2 CurrentChunkPos = new Vector2(CurrentChunkX, CurrentChunkZ);

        for (int x = CurrentChunkX - RenderDistance; x <= CurrentChunkX + RenderDistance; x++)
        {
            for (int z = CurrentChunkZ - RenderDistance; z <= CurrentChunkZ + RenderDistance; z++)
            {
                if (!Chunks.ContainsKey(new Vector2(x, z)))
                {
                    BuildChunk(x, z);
                }
            }
        }

        foreach (Vector2 ChunkPos in Chunks.Keys)
        {
            if (ChunkPos.x < CurrentChunkX - RenderDistance || ChunkPos.x > CurrentChunkX + RenderDistance || ChunkPos.y < CurrentChunkZ - RenderDistance || ChunkPos.y > CurrentChunkZ + RenderDistance) 
            {
                if (Chunks[ChunkPos].GetComponent<ChunkGenerator>().IsLoaded == true)
                {
                    UnloadChunk(Chunks[ChunkPos]);
                }
            }
            else if (Chunks[ChunkPos].GetComponent<ChunkGenerator>().IsLoaded == false)
            {
                LoadChunk(Chunks[ChunkPos]);
            }
        }
    }

    void BuildChunk(int x, int z)
    {
        GameObject Chunk = Instantiate(TerrainChunk, new Vector3(x * ChunkGenerator.ChunkSize.x, 0, z * ChunkGenerator.ChunkSize.z), Quaternion.identity);
        Chunk.GetComponent<ChunkGenerator>().ChunkOffset = new Vector2Int(x, z);
        Chunk.GetComponent<ChunkGenerator>().GenerateChunk();

        Chunks.Add(Chunk.GetComponent<ChunkGenerator>().ChunkOffset, Chunk);
    }

    void LoadChunk(GameObject Chunk)
    {
        Chunk.GetComponent<ChunkGenerator>().IsLoaded = true;
        int[,,] ChunkData = Chunk.GetComponent<ChunkGenerator>().ChunkData;
        Chunk.GetComponent<ChunkGenerator>().BuildMesh(ChunkData);
    }

    void UnloadChunk(GameObject Chunk)
    {
        Chunk.GetComponent<ChunkGenerator>().IsLoaded = false;
        Destroy(Chunk.GetComponent<MeshFilter>().mesh);
    }
}
