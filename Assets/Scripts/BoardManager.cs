using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager I;

    [Header("Level ve Grid Ayarları")]
    public LevelData currentLevel;
    public GameObject tilePrefab;       // Kediler
    public GameObject skillTilePrefab;  // Oyun alanındaki (Grid) noktalar
    public float spacing = 1.3f;

    [Header("Skill Bar Ayarları")]
    public GameObject skillBarPrefab;   // Alt bar kutuları
    public float skillBarYOffset = 3.0f;
    public float skillBarSpacing = 1.6f;

    [Header("Fabrika Üretim Listesi")]
    // 👇 BURASI YENİ: Hangi skillerin çıkacağını buradan seçeceksin
    public List<SkillType> factorySkillPool;

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

        // 2. Oyun Alanı Noktaları
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                float nodeX = startX + (x * spacing) + (spacing / 2f);
                float nodeY = startY + (y * spacing) + (spacing / 2f);

                GameObject go = Instantiate(skillTilePrefab, transform);
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

        // Barı ortalamak için matematik
        float barStartX = -((barCount - 1) * skillBarSpacing) / 2f;

        // Barın Y konumu
        float gridBottomY = -((currentLevel.rows.Count - 1) * spacing) / 2f;
        float barY = gridBottomY - skillBarYOffset;

        for (int i = 0; i < barCount; i++)
        {
            float posX = barStartX + (i * skillBarSpacing);
            Vector3 pos = new Vector3(posX, barY, 0);

            GameObject go = Instantiate(skillBarPrefab, transform);
            go.transform.localPosition = pos;

            SkillTile sTile = go.GetComponent<SkillTile>();

            // ÖNCE Setup yapıyoruz (Bu sırada ismi SkillTile_3_-99 yapıyor)
            sTile.Setup(i, -99);

            // SONRA ismini biz veriyoruz (Son söz bizim!)
            if (i == 3) go.name = "Factory_Slot"; // Fabrika
            else go.name = $"Inventory_Slot_{i}"; // Depo

            bottomSkillBar.Add(sTile);
        }
    }

    // --- GÜNCELLENEN KISIM: LİSTEDEN SEÇME ---
    public void SpawnSkillInFactory()
    {
        if (bottomSkillBar == null || bottomSkillBar.Count < 4) return;
        SkillTile factoryTile = bottomSkillBar[3]; // Son kutu Fabrika

        // Zaten doluysa üretme
        if (factoryTile.currentSkill != SkillType.Bos) return;

        // EĞER LİSTE BOŞSA HATA VERMESİN DİYE KONTROL
        if (factorySkillPool == null || factorySkillPool.Count == 0)
        {
            Debug.LogWarning("⚠️ Factory Skill Pool boş! Lütfen Inspector'dan skill ekleyin.");
            return;
        }

        // Listeden rastgele bir tane seç
        int randomIndex = Random.Range(0, factorySkillPool.Count);
        SkillType selectedSkill = factorySkillPool[randomIndex];

        factoryTile.SetSkill(selectedSkill);
        Debug.Log("Fabrika Üretti: " + selectedSkill);
    }
    // --- EKSİK OLAN PARÇALAR ---

    public Tile GetTile(int x, int y)
    {
        // Grid sınırları içinde mi diye kontrol et
        if (grid == null || x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
            return null;

        return grid[x, y];
    }

    public void SwapTiles(int x1, int y1, int x2, int y2)
    {
        Tile tile1 = GetTile(x1, y1);
        Tile tile2 = GetTile(x2, y2);

        // İki tile da mevcutsa yer değiştir
        if (tile1 != null && tile2 != null)
        {
            // 1. GÖRSEL OLARAK YER DEĞİŞTİR (Pozisyonları takas et)
            Vector3 tempPos = tile1.transform.localPosition;
            tile1.transform.localPosition = tile2.transform.localPosition;
            tile2.transform.localPosition = tempPos;

            // 2. MATEMATİKSEL OLARAK YER DEĞİŞTİR (Array içindeki yerleri takas et)
            grid[x1, y1] = tile2;
            grid[x2, y2] = tile1;

            // 3. Tile içindeki koordinat bilgisini güncelle (Eğer Tile scriptinde x,y tutuyorsan)
            // tile1.Setup(x2, y2, ...); // Gerekirse burası açılır ama şimdilik görsel yetiyor.

            // 4. İsimleri değiştir (Hierarchy'de takip etmek kolay olsun)
            string tempName = tile1.name;
            tile1.name = tile2.name;
            tile2.name = tempName;
        }
    }
}