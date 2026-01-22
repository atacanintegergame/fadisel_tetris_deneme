using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager I;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer dragIconRenderer; // Parmağının ucundaki hayalet resim

    private SkillTile selectedTile; // Hangi kutudan aldık?
    private bool isDragging = false;

    private void Awake()
    {
        I = this;
        if (dragIconRenderer) dragIconRenderer.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 1. TIKLAMA ANI
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                SkillTile clickedTile = hit.collider.GetComponent<SkillTile>();

                // Kutu var mı ve içinde skill var mı?
                if (clickedTile != null && clickedTile.currentSkill != SkillType.Bos)
                {
                    StartDrag(clickedTile);
                }
            }
        }

        // 2. SÜRÜKLEME ANI
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragIconRenderer.transform.position = mousePos;
        }

        // 3. BIRAKMA ANI
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            DropSkill();
        }
    }

    void StartDrag(SkillTile tile)
    {
        selectedTile = tile;
        isDragging = true;

        // Görseli ayarla ve göster
        dragIconRenderer.sprite = BoardManager.I.skillSprites[(int)tile.currentSkill];
        dragIconRenderer.gameObject.SetActive(true);

        Debug.Log("Tutuldu: " + tile.name);
    }

    void DropSkill()
    {
        isDragging = false;
        if (dragIconRenderer) dragIconRenderer.gameObject.SetActive(false);

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            SkillTile targetTile = hit.collider.GetComponent<SkillTile>();

            if (targetTile != null)
            {
                // SENARYO 1: Aldığımız yere geri bıraktık (Fabrika -> Fabrika)
                if (targetTile == selectedTile)
                {
                    Debug.Log("Yerine geri bırakıldı.");
                    selectedTile = null;
                    return; // Hiçbir şey yapma, skill olduğu yerde kalsın
                }

                // SENARYO 2: Hedef kutu BOŞ ise taşı
                if (targetTile.currentSkill == SkillType.Bos)
                {
                    // Transfer işlemi
                    targetTile.SetSkill(selectedTile.currentSkill);
                    selectedTile.SetSkill(SkillType.Bos);
                    Debug.Log("Başarıyla taşındı!");

                    // KRİTİK NOKTA: Eğer Fabrikadan (Factory_Slot) alıp BAŞKA yere koyduysak
                    if (selectedTile.name == "Factory_Slot")
                    {
                        Debug.Log("Fabrika boşaldı, yeni üretim yapılıyor...");
                        BoardManager.I.SpawnSkillInFactory();
                    }
                }
                else
                {
                    Debug.Log("Hata: Hedef kutu dolu!");
                }
            }
        }
        else
        {
            Debug.Log("Boşluğa bırakıldı, işlem iptal.");
        }

        selectedTile = null;
    }
}