using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private Texture2D heightmap = null;

    private Mesh mesh = null;
    private Vector3[] vertices;
    private int[] triangles;

    private int xSize; // how many quads not verts
    private int zSize;

    public int terrainHeightMulti = 50;

    void Start()
    {
        this.mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = this.mesh;
        this.GetComponent<MeshFilter>().sharedMesh = this.mesh;

        if(this.GetComponent<MeshCollider>())
            this.GetComponent<MeshCollider>().sharedMesh = this.mesh;

        this.xSize = this.heightmap.width - 1;
        this.zSize = this.heightmap.height - 1;

        this.GenerateTerrain();
        this.UpdateMesh();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            this.CalculateVertices();
            this.UpdateMesh();
        }
    }

    private void GenerateTerrain()
    {
        this.CalculateVertices();
        this.CalculateTriangles();
    }

    private void CalculateVertices()
    {
        int vertexCount = (this.xSize + 1) * (this.zSize + 1);
        this.vertices = new Vector3[vertexCount];

        for (int i = 0, z = 0; z <= this.zSize; z++)
        {
            for (int x = 0; x <= this.xSize; x++, i++)
            {
                float y = this.heightmap.GetPixel(x, z).grayscale * this.terrainHeightMulti;
                this.vertices[i] = new Vector3(x, y, z);
            }
        }
    }

    private void CalculateTriangles()
    {
        this.triangles = new int[this.xSize * this.zSize * 6];
 
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
