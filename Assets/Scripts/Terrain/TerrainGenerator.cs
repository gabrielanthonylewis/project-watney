using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private Texture2D heightmap = null;

    public int terrainHeightMulti = 50;

    private TerrainMesh terrain;

    void Start()
    {
        this.GenerateHeightMapTerrain();
    }

    private void GenerateHeightMapTerrain()
    {
        int xQuadCount = this.heightmap.width - 1;
        int zQuadCount = this.heightmap.height - 1;
        this.terrain = new TerrainMesh(xQuadCount, zQuadCount,
            this.gameObject.GetComponent<MeshFilter>() ? this.gameObject.GetComponent<MeshFilter>() : this.gameObject.AddComponent<MeshFilter>(),
            this.gameObject.GetComponent<MeshCollider>() ? this.gameObject.GetComponent<MeshCollider>() : this.gameObject.AddComponent<MeshCollider>());

        this.terrain.GenerateTerrain(this.heightmap, this.terrainHeightMulti);
    }

    private void GenerateRandomTerrain(int xQuadCount, int zQuadCount)
    {
        this.terrain = new TerrainMesh(xQuadCount, zQuadCount,
            this.gameObject.GetComponent<MeshFilter>() ? this.gameObject.GetComponent<MeshFilter>() : this.gameObject.AddComponent<MeshFilter>(),
            this.gameObject.GetComponent<MeshCollider>() ? this.gameObject.GetComponent<MeshCollider>() : this.gameObject.AddComponent<MeshCollider>());

        this.terrain.GenerateTerrain(0.3f, this.terrainHeightMulti);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
            this.terrain.GenerateTerrain();
        if (Input.GetKeyDown(KeyCode.O))
            this.GenerateHeightMapTerrain();
        if (Input.GetKeyDown(KeyCode.P))
            this.GenerateRandomTerrain(20, 20);
    }
}
