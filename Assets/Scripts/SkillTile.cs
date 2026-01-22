using UnityEngine;

public class SkillTile : MonoBehaviour
{
    public int x;
    public int y;
    public SkillType currentSkill;

    [Header("Ayarlar")]
    public SpriteRenderer iconRenderer; // Ýçindeki resim (Icon)

    // Ýkonun sýðacaðý maksimum boyut (Unity Birim Cinsinden)
    // Örn: 0.8 yaparsan, resim ne olursa olsun 0.8 birimlik alana sýðdýrýlýr.
    public float maxIconSize = 0.8f;

    public void Setup(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
        name = $"SkillTile_{x}_{y}";
        SetSkill(SkillType.Bos);
    }

    public void SetSkill(SkillType newSkill)
    {
        currentSkill = newSkill;

        if (currentSkill == SkillType.Bos)
        {
            iconRenderer.color = new Color(1, 1, 1, 0f); // Gizle
            iconRenderer.sprite = null;
        }
        else
        {
            iconRenderer.color = Color.white;

            // 1. Resmi ata
            Sprite s = BoardManager.I.skillSprites[(int)newSkill];
            iconRenderer.sprite = s;

            // --- OTO-BOYUTLANDIRMA (AUTO FIT) ---

            // Önce scale'i sýfýrlayalým ki hesap þaþmasýn
            iconRenderer.transform.localScale = Vector3.one;

            // Resmin Unity dünyasýndaki güncel boyutunu al
            // (Bounds bize resmin kapladýðý alaný verir)
            Vector3 spriteSize = iconRenderer.bounds.size;

            // Resim kare olmayabilir, en büyük kenarýný bulalým
            float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);

            // Eðer resim bizim istediðimizden büyükse veya küçükse orantýla
            // Formül: (Hedef Boyut / Mevcut Boyut)
            float requiredScale = maxIconSize / maxDimension;

            // Yeni scale'i uygula (En-Boy oraný bozulmadan)
            iconRenderer.transform.localScale = new Vector3(requiredScale, requiredScale, 1f);
        }
    }
}