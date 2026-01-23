using UnityEngine;
using System.Collections; // Coroutine için gerekli

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager I;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer dragIconRenderer;
    public float snapRadius = 1.0f;

    private SkillTile sourceTile;
    private bool isDragging = false;
    private Vector3 originalPosition;

    private void Awake()
    {
        I = this;
        if (dragIconRenderer)
        {
            dragIconRenderer.gameObject.SetActive(false);
            dragIconRenderer.sortingOrder = 100;
        }
    }

    private void Update()
    {
        // 1. TIKLAMA ANI
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            foreach (var hit in hits)
            {
                SkillTile clickedTile = hit.collider.GetComponent<SkillTile>();
                if (clickedTile != null && clickedTile.currentSkill != SkillType.Bos)
                {
                    StartDrag(clickedTile);
                    break;
                }
            }
        }

        // 2. SÜRÜKLEME ANI
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
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
        sourceTile = tile;
        isDragging = true;
        originalPosition = tile.transform.position;

        dragIconRenderer.transform.position = tile.iconRenderer.transform.position;
        dragIconRenderer.transform.localScale = tile.iconRenderer.transform.lossyScale;
        dragIconRenderer.sprite = BoardManager.I.skillSprites[(int)tile.currentSkill];

        dragIconRenderer.gameObject.SetActive(true);
        tile.iconRenderer.enabled = false;
    }

    void DropSkill()
    {
        isDragging = false;
        if (dragIconRenderer) dragIconRenderer.gameObject.SetActive(false); // Hayaleti kapat

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        SkillTile closestTile = null;
        float minDistance = float.MaxValue;
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

        bool transactionSuccess = false;

        if (closestTile != null)
        {
            // Kural: Kendi yerine bırakırsa iptal
            if (closestTile == sourceTile)
            {
                transactionSuccess = false;
            }
            // Kural: Hedef BOŞSA -> Oraya koy VE ÇALIŞTIR
            else if (closestTile.currentSkill == SkillType.Bos)
            {
                if (closestTile.name == "Factory_Slot" && sourceTile.name != "Factory_Slot")
                {
                    Debug.Log("⛔ Fabrikaya geri ürün koyamazsın!");
                    transactionSuccess = false;
                }
                else
                {
                    // 1. Skilli hedefe yerleştir
                    closestTile.SetSkill(sourceTile.currentSkill);

                    // 2. Kaynağı temizle
                    sourceTile.SetSkill(SkillType.Bos);

                    // --- KRİTİK DÜZELTME BURADA ---
                    if (sourceTile.name == "Factory_Slot")
                    {
                        // Görünürlüğü tekrar aç ki yeni üretilen skill görünsün!
                        sourceTile.iconRenderer.enabled = true;
                        BoardManager.I.SpawnSkillInFactory();
                    }

                    // 3. ANINDA ETKİLEŞİM (Sadece oyun alanı için)
                    if (closestTile.y != -99)
                    {
                        StartCoroutine(ExecuteAndClearRoutine(closestTile));
                    }

                    transactionSuccess = true;
                }
            }
        }

        // Başarısızsa eve dön
        if (!transactionSuccess && sourceTile != null)
        {
            sourceTile.iconRenderer.enabled = true;
        }

        sourceTile = null;
    }

    // --- SKILL ÇALIŞTIRMA VE TEMİZLEME RUTİNİ ---
    IEnumerator ExecuteAndClearRoutine(SkillTile tile)
    {
        // 1. Skilli çok kısa (0.2sn) göster ki oyuncu ne koyduğunu algılasın
        yield return new WaitForSeconds(0.2f);

        // 2. Efekti patlat! (SkillManager'ı çağır)
        SkillManager.I.ApplySkillEffect(tile);

        // 3. Skilli kutudan sil (Kullanıldı bitti)
        tile.SetSkill(SkillType.Bos);
    }
}