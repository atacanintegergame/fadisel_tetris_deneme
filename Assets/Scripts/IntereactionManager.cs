using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager I;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer dragIconRenderer; // Sürüklediğimiz kopya görsel

    [Header("Hassasiyet Ayarları")]
    public float snapRadius = 1.0f; // Ne kadar yakınına gelirse yapışsın? (Project A'daki distanceThreshold gibi)

    private SkillTile sourceTile;   // Skilli nereden aldık?
    private bool isDragging = false;
    private Vector3 originalPosition; // Sürüklemeye başladığımız koordinat (Geri dönüş için)

    private void Awake()
    {
        I = this;
        if (dragIconRenderer)
        {
            dragIconRenderer.gameObject.SetActive(false);
            // Sürüklenen parça her zaman en önde gözüksün
            dragIconRenderer.sortingOrder = 100;
        }
    }

    private void Update()
    {
        // 1. TIKLAMA ANI (OnBeginDrag Mantığı)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Tıkladığımız yerdeki her şeye bak (UI gibi davranması için RaycastAll)
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            foreach (var hit in hits)
            {
                SkillTile clickedTile = hit.collider.GetComponent<SkillTile>();

                // Sadece dolu olan kutuları tutabiliriz
                if (clickedTile != null && clickedTile.currentSkill != SkillType.Bos)
                {
                    StartDrag(clickedTile);
                    break; // İlk bulduğunu tut, gerisine bakma
                }
            }
        }

        // 2. SÜRÜKLEME ANI (OnDrag Mantığı)
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Z eksenini sabitle
            dragIconRenderer.transform.position = mousePos;
        }

        // 3. BIRAKMA ANI (OnEndDrag Mantığı)
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            DropSkill();
        }
    }

    void StartDrag(SkillTile tile)
    {
        sourceTile = tile;
        isDragging = true;

        // 1. Kaynak Tile'ın pozisyonunu kaydet
        originalPosition = tile.transform.position;

        // 2. DragIcon'u tam resmin olduğu yere taşı
        // (Dikkat: tile.transform.position yerine tile.iconRenderer.transform.position kullanıyoruz)
        dragIconRenderer.transform.position = tile.iconRenderer.transform.position;

        // --- İŞTE ÇÖZÜM BURASI ---
        // localScale yerine "lossyScale" kullanıyoruz.
        // Bu, hem kutunun hem de resmin toplam büyüklüğünü alır.
        dragIconRenderer.transform.localScale = tile.iconRenderer.transform.lossyScale;

        dragIconRenderer.sprite = BoardManager.I.skillSprites[(int)tile.currentSkill];

        // 3. Görünür yap ve yerdekini gizle
        dragIconRenderer.gameObject.SetActive(true);
        tile.iconRenderer.enabled = false;
    }

    void DropSkill()
    {
        isDragging = false;

        // Mouse'un olduğu yer
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // --- EN YAKIN KUTUYU BULMA (Project A'daki FindClosestSkillTile mantığı) ---
        SkillTile closestTile = null;
        float minDistance = float.MaxValue;

        // Sahnedeki o noktaya yakın tüm objeleri tara
        Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePos, snapRadius);

        foreach (var col in colliders)
        {
            SkillTile tile = col.GetComponent<SkillTile>();
            if (tile != null)
            {
                float dist = Vector2.Distance(mousePos, tile.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTile = tile;
                }
            }
        }

        // --- KARAR ANI ---
        bool isSuccess = false;

        if (closestTile != null)
        {
            // 1. Kural: Kendisine geri bırakırsa işlem iptal (Ama başarılı sayılır, yerine oturur)
            if (closestTile == sourceTile)
            {
                isSuccess = false; // "İşlem yapılmadı" olarak işaretle ki yerine dönsün
            }
            // 2. Kural: Hedef boşsa yerleştir
            else if (closestTile.currentSkill == SkillType.Bos)
            {
                // Fabrika Koruması: Fabrikaya dışarıdan mal giremez
                if (closestTile.name == "Factory_Slot" && sourceTile.name != "Factory_Slot")
                {
                    Debug.Log("⛔ Fabrikaya geri ürün koyamazsın!");
                    isSuccess = false;
                }
                else
                {
                    // TRANSFER BAŞARILI!
                    closestTile.SetSkill(sourceTile.currentSkill);
                    sourceTile.SetSkill(SkillType.Bos); // Eskisini boşalt

                    // Eğer Fabrikadan aldıysak yenisini üret
                    if (sourceTile.name == "Factory_Slot")
                    {
                        BoardManager.I.SpawnSkillInFactory();
                    }

                    isSuccess = true;
                }
            }
        }

        // --- SONUÇ ---

        // İşlem bittiği için DragIcon'u kapat
        dragIconRenderer.gameObject.SetActive(false);

        if (isSuccess)
        {
            // Başarılıysa zaten yeni yere yerleşti, eski yer (sourceTile) boşaltıldı veya yenilendi.
            // Sadece görseli açmamız gerekiyorsa (Factory durumunda) açalım.
            if (sourceTile.currentSkill != SkillType.Bos)
                sourceTile.iconRenderer.enabled = true;
            else
                sourceTile.iconRenderer.enabled = true; // Boş olsa bile renderer açık kalsın
        }
        else
        {
            // --- RETURN START POS (Eski yerine geri dön) ---
            // Eğer koyacak yer bulamadıysa veya yasaklıysa
            Debug.Log("↩️ Yerine dönüyor...");
            sourceTile.iconRenderer.enabled = true; // Görseli geri aç
        }

        sourceTile = null;
    }
}