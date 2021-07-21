using UnityEngine;

public class GridManager : MonoBehaviour
{
    private int rows = 16;
    private int cols = 16;
    private int tileSize = 1;
    private System.Random rand;
    private int seed = 42069;

    // Start is called before the first frame update
    void Start()
    {
        rand = new System.Random(seed);
        generateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void generateGrid() {

        GameObject[] referenceTiles = new GameObject[3];
        referenceTiles[0] = (GameObject) Instantiate(Resources.Load("MudTile"));
        referenceTiles[1] = (GameObject) Instantiate(Resources.Load("GrassLandTile"));
        referenceTiles[2] = (GameObject) Instantiate(Resources.Load("HighlandsTile"));

        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < cols; col++) {
                GameObject tile = (GameObject) Instantiate(referenceTiles[rand.Next(0, referenceTiles.Length)], transform);

                float posX = col * tileSize;
                float posY = row * -tileSize;

                tile.transform.position = new Vector2(posX, posY);
            }
        }

        foreach (GameObject gameObject in referenceTiles) {
            Destroy(gameObject);
        }

    }
}
