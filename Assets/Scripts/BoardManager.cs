using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager I;

    [Header("Ayarlar")]
    public LevelData currentLevel;
    public GameObject tilePrefab;
    public GameObject skillTilePrefab; // <-- YENÝ: SkillNode Prefab'ýný buraya atacaðýz
    public float spacing = 1.3f;

    [Header("Görseller")]
    public Sprite[] animalSprites;

    private Tile[,] grid;
    private SkillTile[,] skillGrid; // <-- YENÝ: SkillNode'larý tutacaðýmýz liste

    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        // Coroutine ile biraz bekle ki her þey otursun
        StartCoroutine(GenerateBoardRoutine());
    }

    // IEnumerator yaparak minik bir bekleme ekledik, daha saðlýklý çalýþýr
    System.Collections.IEnumerator GenerateBoardRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        GenerateBoard();
    }

    void GenerateBoard()
    {
        if (currentLevel == null) return;

        // Önceki oluþturulanlarý temizle (Eðer varsa)
        foreach (Transform child in transform) Destroy(child.gameObject);

        int height = currentLevel.rows.Count;
        int width = currentLevel.rows[0].columns.Count;

        grid = new Tile[width, height];

        // SkillGrid boyutu, normal gridin 1 eksiði olur (4x4 kutu varsa, 3x3 ara boþluk vardýr)
        skillGrid = new SkillTile[width - 1, height - 1];

        float startX = -((width - 1) * spacing) / 2f;
        float startY = -((height - 1) * spacing) / 2f;

        // --- 1. KISIM: HAYVANLARI OLUÞTUR ---
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int rowIndex = (height - 1) - y;
                AnimalType type = currentLevel.rows[rowIndex].columns[x];

                Vector3 pos = new Vector3(startX + (x * spacing), startY + (y * spacing), 0);

                GameObject go = Instantiate(tilePrefab, transform);
                go.transform.localPosition = pos; // LocalPosition kullanýyoruz!

                Tile tile = go.GetComponent<Tile>();

                Sprite img = null;
                int typeIndex = (int)type;
                if (typeIndex > 0 && typeIndex < animalSprites.Length) img = animalSprites[typeIndex];

                tile.Setup(x, y, type, img);
                grid[x, y] = tile;
            }
        }

        // --- 2. KISIM: SKILL NODE (ARA NOKTALARI) OLUÞTUR ---
        // Dikkat: Döngü (width - 1) ve (height - 1) kadar dönüyor
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // Pozisyon hesabý: Hayvanýn tam "yarým boþluk" (spacing/2) saðýna ve yukarýsýna
                float nodeX = startX + (x * spacing) + (spacing / 2f);
                float nodeY = startY + (y * spacing) + (spacing / 2f);

                Vector3 pos = new Vector3(nodeX, nodeY, 0);

                GameObject go = Instantiate(skillTilePrefab, transform);
                go.transform.localPosition = pos;

                SkillTile node = go.GetComponent<SkillTile>();
                node.Setup(x, y);
                skillGrid[x, y] = node;
            }
        }
    }
}