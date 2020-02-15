using UnityEngine;

struct TerrainObject
{
    public GameObject gameObject;
    public TerrainMesh terrain;
};

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private Texture2D heightmap = null;

    [SerializeField]
    private Material meshMaterial = null;

    public int terrainHeightMulti = 50;


    void Start()
    {
        this.GenerateHeightMapTerrain();
    }

    private void GenerateHeightMapTerrain()
    {
        // assumes width and height are same and divisible by 256
        int horizontalMeshCount = Mathf.RoundToInt(this.heightmap.width / 256.0f);
        int verticalMeshCount = Mathf.RoundToInt(this.heightmap.height / 256.0f);

        for (int row = 0; row < verticalMeshCount; row++)
        {
            for (int col = 0; col < horizontalMeshCount; col++)
            {
                Color32[] pixels = new Color32[256 * 256];
                for(int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        pixels[j + i * 256] = this.heightmap.GetPixel(col * 256 + j, row * 256 + i);
                    }
                }

                Texture2D tempHeightmap = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                tempHeightmap.filterMode = FilterMode.Point;
                tempHeightmap.SetPixels32(pixels);
                tempHeightmap.Apply();

                int xQuadCount = 256;//this.heightmap.width - 1;
                int zQuadCount = 256;// this.heightmap.height - 1;

                TerrainObject newTerrain = new TerrainObject();
                newTerrain.gameObject = new GameObject(tempHeightmap.name + "_" + row.ToString() + "_" + col.ToString());
                newTerrain.gameObject.transform.position = new Vector3(256 * col, 0, 256 * row);

                newTerrain.gameObject.AddComponent<MeshRenderer>();
                newTerrain.gameObject.AddComponent<MeshFilter>();
                newTerrain.gameObject.AddComponent<MeshCollider>();

                newTerrain.gameObject.GetComponent<MeshRenderer>().material = this.meshMaterial;

                newTerrain.terrain = new TerrainMesh(xQuadCount, zQuadCount,
                    newTerrain.gameObject.GetComponent<MeshFilter>() ? newTerrain.gameObject.GetComponent<MeshFilter>() : newTerrain.gameObject.AddComponent<MeshFilter>(),
                    newTerrain.gameObject.GetComponent<MeshCollider>() ? newTerrain.gameObject.GetComponent<MeshCollider>() : newTerrain.gameObject.AddComponent<MeshCollider>());

                newTerrain.terrain.GenerateTerrain(tempHeightmap, this.terrainHeightMulti);
            }
        }
    }

    private void GenerateRandomTerrain(int xQuadCount, int zQuadCount)
    {
        TerrainObject newTerrain = new TerrainObject();
        newTerrain.gameObject = new GameObject(this.heightmap.name);
        newTerrain.gameObject.transform.position = Vector3.zero;

        newTerrain.gameObject.AddComponent<MeshRenderer>();
        newTerrain.gameObject.AddComponent<MeshFilter>();
        newTerrain.gameObject.AddComponent<MeshCollider>();

        newTerrain.gameObject.GetComponent<MeshRenderer>().material = this.meshMaterial;

        newTerrain.terrain = new TerrainMesh(xQuadCount, zQuadCount,
            newTerrain.gameObject.GetComponent<MeshFilter>() ? newTerrain.gameObject.GetComponent<MeshFilter>() : newTerrain.gameObject.AddComponent<MeshFilter>(),
            newTerrain.gameObject.GetComponent<MeshCollider>() ? newTerrain.gameObject.GetComponent<MeshCollider>() : newTerrain.gameObject.AddComponent<MeshCollider>());
        
        newTerrain.terrain.GenerateTerrain(0.3f, this.terrainHeightMulti);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            this.GenerateHeightMapTerrain();
        if (Input.GetKeyDown(KeyCode.P))
            this.GenerateRandomTerrain(20, 20);
    }
}
