using UnityEngine;

public class TerrainMesh
{
    private Mesh mesh = new Mesh();
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3 position;
    private int xSize; // how many quads not verts
    private int zSize;

    public TerrainMesh(int xQuadCount, int zQuadCount, 
        MeshFilter meshFilter, MeshCollider meshCollider)
    {
        this.xSize = xQuadCount;
        this.zSize = zQuadCount;

        this.position = position;

        int vertexCount = (this.xSize + 1) * (this.zSize + 1);
        this.vertices = new Vector3[vertexCount];

        this.triangles = new int[this.xSize * this.zSize * 6];

        this.meshFilter = meshFilter;
        this.meshCollider = meshCollider;

        this.meshFilter.mesh = this.mesh;
        this.meshFilter.sharedMesh = this.mesh;
        this.meshCollider.sharedMesh = this.mesh;
    }

    public void GenerateTerrain()
    {
        this.CalculateVertices();
        this.CalculateTriangles();
        this.UpdateMesh();
    }

    public void GenerateTerrain(Texture2D heightmap, int terrainHeightMulti)
    {
        this.CalculateVertices(heightmap, terrainHeightMulti);
        this.CalculateTriangles();
        this.UpdateMesh();
    }

    public void GenerateTerrain(float zoomMultiplier, float terrainHeightMulti)
    {
        this.CalculateVertices(zoomMultiplier, terrainHeightMulti);
        this.CalculateTriangles();
        this.UpdateMesh();
    }

    private void CalculateVertices()
    {
        for (int i = 0, z = 0; z <= this.zSize; z++)
        {
            for (int x = 0; x <= this.xSize; x++, i++)
                this.vertices[i] = new Vector3(x, 0, z);
        }
    }

    private void CalculateVertices(Texture2D heightmap, int terrainHeightMulti)
    {
        for (int i = 0, z = 0; z <= this.zSize; z++)
        {
            for (int x = 0; x <= this.xSize; x++, i++)
                this.vertices[i] = new Vector3(x, heightmap.GetPixel(x, z).grayscale * terrainHeightMulti, z);
        }
    }

    private void CalculateVertices(float zoomMultiplier, float terrainHeightMulti)
    {
        for (int i = 0, z = 0; z <= this.zSize; z++)
        {
            for (int x = 0; x <= this.xSize; x++, i++)
            {
                // 0.3f to zoom out a bit, * 2.0f to be clearer
                this.vertices[i] = new Vector3(x, Mathf.PerlinNoise(x * zoomMultiplier, z * zoomMultiplier) * terrainHeightMulti, z);
            }
        }
    }


    private void CalculateTriangles()
    {
        for (int z = 0, vert = 0, tris = 0; z < this.zSize; z++, vert++)
        {
            for (int x = 0; x < this.xSize; x++, vert++, tris += 6)
            {
                this.triangles[tris] = vert;
                this.triangles[tris + 1] = vert + this.xSize + 1;
                this.triangles[tris + 2] = vert + 1;
                this.triangles[tris + 3] = vert + 1;
                this.triangles[tris + 4] = vert + this.xSize + 1;
                this.triangles[tris + 5] = vert + this.xSize + 2;
            }
        }
    }

    private void UpdateMesh()
    {
        this.mesh.Clear();

        this.mesh.vertices = this.vertices;
        this.mesh.triangles = this.triangles;

        this.mesh.RecalculateBounds();
        this.mesh.RecalculateNormals();
        this.mesh.RecalculateTangents();
    }
}
