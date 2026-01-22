using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager I;

    [Header("Level ve Grid Ayarlarý")]
    public LevelData currentLevel;
    public GameObject tilePrefab;       // Kediler
    public GameObject skillTilePrefab;  // Oyun alanýndaki (Grid) noktalar
    public float spacing = 1.3f;

    [Header("Skill Bar Ayarlarý")]
    public GameObject skillBarPrefab;   // <-- YENÝ: Sadece alt bar için özel prefab!
    public float skillBarYOffset = 3.0f;
    public float skillBarSpacing = 1.8f; // Bar kutularý daha geniþ aralýklý olabilir

    [Header("Görseller")]
    public Sprite[] animalSprites;
    public Sprite[] skillSprites;

    private Tile[,] grid;
    private SkillTile[,] skillGrid;
    public List<SkillTile> bottomSkillBar;

    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        StartCoroutine(StartupRoutine());
    }

    IEnumerator StartupRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        GenerateBoard();
        CreateSkillBar();
        yield return new WaitForSeconds(0.1f);
        SpawnSkillInFactory();
    }

    public void GenerateBoard()
    {
        if (currentLevel == null) return;
        foreach (Transform child in transform) Destroy(child.gameObject);

        int height = currentLevel.rows.Count;
        int width = currentLevel.rows[0].columns.Count;

        grid = new Tile[width, height];
        skillGrid = new SkillTile[width - 1, height - 1];

        float startX = -((width - 1) * spacing) / 2f;
        float startY = -((height - 1) * spacing) / 2f;

        // 1. Hayvanlar
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int rowIndex = (height - 1) - y;
                AnimalType type = currentLevel.rows[rowIndex].columns[x];
                Vector3 pos = new Vector3(startX + (x * spacing), startY + (y * spacing), 0);

                GameObject go = Instantiate(tilePrefab, transform);
                go.transform.localPosition = pos;
                Tile tile = go.GetComponent<Tile>();
                Sprite img = null;
                int typeIndex = (int)type;
                if (typeIndex > 0 && typeIndex < animalSprites.Length) img = animalSprites[typeIndex];
                tile.Setup(x, y, type, img);
                grid[x, y] = tile;
            }
        }

        // 2. Oyun Alaný Noktalarý (Eski Prefab'ý kullanýr)
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                float nodeX = startX + (x * spacing) + (spacing / 2f);
                float nodeY = startY + (y * spacing) + (spacing / 2f);

                GameObject go = Instantiate(skillTilePrefab, transform); // <-- BURASI ESKÝSÝ
                go.transform.localPosition = new Vector3(nodeX, nodeY, 0);

                SkillTile node = go.GetComponent<SkillTile>();
                node.Setup(x, y);
                skillGrid[x, y] = node;
            }
        }
    }

    void CreateSkillBar()
    {
        bottomSkillBar = new List<SkillTile>();
        int barCount = 4;
        float barStartX = -((barCount - 1) * skillBarSpacing) / 2f;
        float gridBottomY = -((currentLevel.rows.Count - 1) * spacing) / 2f;
        float barY = gridBottomY - skillBarYOffset;

        for (int i = 0; i < barCount; i++)
        {
            float posX = barStartX + (i * skillBarSpacing);
            Vector3 pos = new Vector3(posX, barY, 0);

            // <-- ÝÞTE BURASI DEÐÝÞTÝ: Artýk yeni prefabý yaratýyoruz!
            GameObject go = Instantiate(skillBarPrefab, transform);
            go.transform.localPosition = pos;

            if (i == 3) go.name = "Factory_Slot";
            else go.name = $"Inventory_Slot_{i}";

            SkillTile sTile = go.GetComponent<SkillTile>();
            sTile.Setup(i, -99);
            bottomSkillBar.Add(sTile);
        }
    }

    public void SpawnSkillInFactory()
    {
        if (bottomSkillBar == null || bottomSkillBar.Count < 4) return;
        SkillTile factoryTile = bottomSkillBar[3];
        if (factoryTile.currentSkill != SkillType.Bos) return;
        int randomSkillIndex = Random.Range(1, 12);
        SkillType randomSkill = (SkillType)randomSkillIndex;
        factoryTile.SetSkill(randomSkill);
    }
}