using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    public LayerMask blockLayer;
    public GameObject TerrainGen;
    private float maxDistance = 4;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);
        if (leftClick || rightClick)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hitInfo;
            
            
            if (Physics.Raycast(ray, out hitInfo, maxDistance, blockLayer))
            {
                Vector3 PointInTargetBlock;

                if (leftClick)
                {
                    PointInTargetBlock = hitInfo.point + Camera.main.transform.forward * 0.01f;
                } 
                else
                {
                    PointInTargetBlock = hitInfo.point - Camera.main.transform.forward * 0.01f;
                }

                int ChunkPosX = Mathf.FloorToInt(PointInTargetBlock.x / 16.0f);
                int ChunkPosZ = Mathf.FloorToInt(PointInTargetBlock.z / 16.0f);

                Vector2 ChunkPos = new Vector2(ChunkPosX, ChunkPosZ);

                GameObject Chunk = TerrainGen.GetComponent<TerrainGenerator>().Chunks[ChunkPos];

                int BlockX = Mathf.FloorToInt(PointInTargetBlock.x) - (ChunkPosX * 16) + 1;
                int BlockY = Mathf.FloorToInt(PointInTargetBlock.y);
                int BlockZ = Mathf.FloorToInt(PointInTargetBlock.z) - (ChunkPosZ * 16) + 1;

                Debug.Log(new Vector3(BlockX, BlockY, BlockZ));

                if (leftClick)
                {
                    Chunk.GetComponent<ChunkGenerator>().ChunkData[BlockX, BlockY, BlockZ] = 0;
                    Destroy(Chunk.GetComponent<MeshFilter>().mesh);
                    Chunk.GetComponent<ChunkGenerator>().BuildMesh(Chunk.GetComponent<ChunkGenerator>().ChunkData);
                }
                if (rightClick)
                {
                    Chunk.GetComponent<ChunkGenerator>().ChunkData[BlockX, BlockY, BlockZ] = 3;
                    Destroy(Chunk.GetComponent<MeshFilter>().mesh);
                    Chunk.GetComponent<ChunkGenerator>().BuildMesh(Chunk.GetComponent<ChunkGenerator>().ChunkData);
                }
            }
        }
    }
}
