using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{

    public static readonly Vector3Int ChunkSize = new Vector3Int(16, 256, 16);
    public int[,,] ChunkData = new int[ChunkSize.x, ChunkSize.y, ChunkSize.z];
    public bool IsLoaded = false;
    public Vector2Int ChunkOffset;

    public int[,,] GenerateChunk()
    {
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    float PerlinCoordX = (x + (ChunkOffset.x * ChunkSize.x)) / ((float)ChunkSize.x - 0) * (0 - 1) + 1;
                    float PerlinCoordY = (z + (ChunkOffset.y * ChunkSize.z)) / ((float)ChunkSize.z - 0) * (0 - 1) + 1;
                    float Continentalness = Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY);
                    int SurfaceY = Mathf.FloorToInt(Mathf.Clamp(getHeight(Continentalness), 0, 256));

                    if (y == SurfaceY)
                    {
                        ChunkData[x, y, z] = 1; // Grass
                    }
                    else if (y < SurfaceY && y > SurfaceY - 4)
                    {
                        ChunkData[x, y, z] = 2; // Dirt
                    }
                    else if (y <= SurfaceY - 4 && y > 0)
                    {
                        ChunkData[x, y, z] = 3; // Stone
                    }
                    else
                    {
                        ChunkData[x, y, z] = 0; // Air
                    }
                }
            }
        }

        return ChunkData;
    }

    public void BuildMesh(int[,,] Chunk)
    {
        Mesh mesh = new Mesh();

        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Vector2> UVs = new List<Vector2>();

        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    if (Chunk[x, y, z] != 0)
                    {
                        Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                        int NumFaces = 0;

                        // No land above
                        if (y < ChunkSize.y - 1 && Chunk[x, y + 1, z] == 0)
                        {
                            Vertices.Add(blockPos + new Vector3(0, 1, 0));
                            Vertices.Add(blockPos + new Vector3(0, 1, 1));
                            Vertices.Add(blockPos + new Vector3(1, 1, 1));
                            Vertices.Add(blockPos + new Vector3(1, 1, 0));
                            NumFaces++;

                            UVs.AddRange(GetTexture(ChunkData[x, y, z], "Top"));
                        }

                        // No land below
                        if ((y > 0 && Chunk[x, y - 1, z] == 0) || y == 0)
                        {
                            Vertices.Add(blockPos + new Vector3(0, 0, 0));
                            Vertices.Add(blockPos + new Vector3(1, 0, 0));
                            Vertices.Add(blockPos + new Vector3(1, 0, 1));
                            Vertices.Add(blockPos + new Vector3(0, 0, 1));
                            NumFaces++;

                            UVs.AddRange(GetTexture(ChunkData[x, y, z], "Bottom"));
                        }

                        // No land in front
                        if ((z > 0 && Chunk[x, y, z - 1] == 0) || z == 0)
                        {
                            Vertices.Add(blockPos + new Vector3(0, 0, 0));
                            Vertices.Add(blockPos + new Vector3(0, 1, 0));
                            Vertices.Add(blockPos + new Vector3(1, 1, 0));
                            Vertices.Add(blockPos + new Vector3(1, 0, 0));
                            NumFaces++;

                            UVs.AddRange(GetTexture(ChunkData[x, y, z]));
                        }

                        // No land behind
                        if ((z < ChunkSize.z - 1 && Chunk[x, y, z + 1] == 0) || z == ChunkSize.z - 1)
                        {
                            Vertices.Add(blockPos + new Vector3(1, 0, 1));
                            Vertices.Add(blockPos + new Vector3(1, 1, 1));
                            Vertices.Add(blockPos + new Vector3(0, 1, 1));
                            Vertices.Add(blockPos + new Vector3(0, 0, 1));
                            NumFaces++;

                            UVs.AddRange(GetTexture(ChunkData[x, y, z]));
                        }

                        // No land to the right
                        if ((x < ChunkSize.x - 1 && Chunk[x + 1, y, z] == 0) || x == ChunkSize.x - 1)
                        {
                            Vertices.Add(blockPos + new Vector3(1, 0, 0));
                            Vertices.Add(blockPos + new Vector3(1, 1, 0));
                            Vertices.Add(blockPos + new Vector3(1, 1, 1));
                            Vertices.Add(blockPos + new Vector3(1, 0, 1));
                            NumFaces++;

                            UVs.AddRange(GetTexture(ChunkData[x, y, z]));
                        }

                        // No land to the left
                        if ((x > 0 && Chunk[x - 1, y, z] == 0) || x == 0)
                        {
                            Vertices.Add(blockPos + new Vector3(0, 0, 1));
                            Vertices.Add(blockPos + new Vector3(0, 1, 1));
                            Vertices.Add(blockPos + new Vector3(0, 1, 0));
                            Vertices.Add(blockPos + new Vector3(0, 0, 0));
                            NumFaces++;

                            UVs.AddRange(GetTexture(ChunkData[x, y, z]));
                        }


                        // Calculate triangles
                        int tl = Vertices.Count - 4 * NumFaces;
                        for (int i = 0; i < NumFaces; i++)
                        {
                            Triangles.AddRange(new int[] {tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3});
                        }

                    }
                }
            }
        }

        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.uv = UVs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    Vector2[] GetTexture(int BlockType, string Side="Front")
    {
        // Grass top
        Vector2[] GrassTop = new Vector2[] {
            new Vector2(1 / 16.0f + 0.001f, 0 / 16.0f + 0.001f),
            new Vector2(1 / 16.0f + 0.001f, 1 / 16.0f - 0.001f),
            new Vector2(2 / 16.0f - 0.001f, 1 / 16.0f - 0.001f),
            new Vector2(2 / 16.0f - 0.001f, 0 / 16.0f + 0.001f) };

        // Grass side
        Vector2[] GrassSide = new Vector2[]
        {
            new Vector2(0 / 16.0f + 0.001f, 1 / 16.0f + 0.001f),
            new Vector2(0 / 16.0f + 0.001f, 2 / 16.0f - 0.001f),
            new Vector2(1 / 16.0f - 0.001f, 2 / 16.0f - 0.001f),
            new Vector2(1 / 16.0f - 0.001f, 1 / 16.0f + 0.001f) };

        // Dirt
        Vector2[] Dirt = new Vector2[]
        {
            new Vector2(0 / 16.0f + 0.001f, 0 / 16.0f + 0.001f),
            new Vector2(0 / 16.0f + 0.001f, 1 / 16.0f - 0.001f),
            new Vector2(1 / 16.0f - 0.001f, 1 / 16.0f - 0.001f),
            new Vector2(1 / 16.0f - 0.001f, 0 / 16.0f + 0.001f) };

        // Stone
        Vector2[] Stone = new Vector2[]
        {
            new Vector2(1 / 16.0f + 0.001f, 1 / 16.0f + 0.001f),
            new Vector2(1 / 16.0f + 0.001f, 2 / 16.0f - 0.001f),
            new Vector2(2 / 16.0f - 0.001f, 2 / 16.0f - 0.001f),
            new Vector2(2 / 16.0f - 0.001f, 1 / 16.0f + 0.001f) };

        if (BlockType == 1) 
        { 
            if (Side == "Top")
            {
                return GrassTop;
            }
            else if (Side == "Bottom")
            {
                return Dirt;
            }
            else 
            { 
                return GrassSide; 
            }
        }

        if (BlockType == 2)
        {
            return Dirt;
        }

        if(BlockType == 3)
        {
            return Stone;
        }

        return Dirt;
    }


    private float getHeight(float x)
    {
        Vector2[] points = new Vector2[3];
        points[0] = new Vector2(0, 50);
        points[1] = new Vector2(0.5f, 60);
        points[2] = new Vector2(1, 70);

        Vector2 FinalLerp = Vector2.zero;

        // Spline Interpolation
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (x < points[i + 1].x && x >= points[i].x)
            {
                Vector2 pointH = new Vector2((points[i + 1].x + points[i].x) / 2, (points[i + 1].y + points[i].y) / 2);
                Vector2 Lerp1 = Vector2.Lerp(points[i], pointH, Mathf.InverseLerp(points[i].x, pointH.x, x));
                Vector2 Lerp2 = Vector2.Lerp(pointH, points[i + 1], Mathf.InverseLerp(pointH.x, points[i + 1].x, x));
                FinalLerp = Vector2.Lerp(Lerp1, Lerp2, Mathf.InverseLerp(points[i].x, points[i + 1].x, x));
            }
        }

        return FinalLerp.y;
        
        /*
        // Linear Interpolation
        float FinalY = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (x < points[i + 1].x && x >= points[i].x)
            {
                float PositionBetweenPoints = Mathf.InverseLerp(points[i].x, points[i + 1].x, x);
                FinalY = Mathf.Lerp(points[i].y, points[i + 1].y, PositionBetweenPoints);
                return FinalY;
            }
        }
        return FinalY;
        */
    }
    void BuildCubes(int[,,] Chunk)
    {
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int y = 0; y < ChunkSize.y; y++)
            {
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    if (Chunk[x, y, z] != 0)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = new Vector3(x, y, z);

                        // No land above
                        if (Chunk[x, y + 1, z] == 0)
                        {

                        }
                    }
                }
            }
        }
    }
}
