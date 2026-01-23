using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager I;

    private void Awake()
    {
        I = this;
    }

    // Artık buton yok, bu fonksiyonu direkt InteractionManager çağıracak
    public void ApplySkillEffect(SkillTile tile)
    {
        if (tile == null || tile.currentSkill == SkillType.Bos) return;

        int x = tile.x;
        int y = tile.y;

        Debug.Log($"⚡ SKILL TETİKLENDİ: {tile.currentSkill} (Konum: {x},{y})");

        switch (tile.currentSkill)
        {
            case SkillType.YatayIkili: // Yatay İkili
                BoardManager.I.SwapTiles(x, y, x + 1, y);
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y + 1);
                break;

            case SkillType.DikeyIkili: // Dikey İkili
                BoardManager.I.SwapTiles(x, y, x, y + 1);
                BoardManager.I.SwapTiles(x + 1, y, x + 1, y + 1);
                break;

            
            case SkillType.Cross: // Çapraz
                BoardManager.I.SwapTiles(x, y, x + 1, y + 1);
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y);
                break;

            case SkillType.SolDiagonal:
                BoardManager.I.SwapTiles(x, y, x + 1, y + 1);
                break;

            case SkillType.SagDiagonal:
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y);
                break;

            case SkillType.SagaDaire:
                // 3 Adımlı Swap (Saat Yönü)
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y + 1);
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y);
                BoardManager.I.SwapTiles(x, y + 1, x, y);
                break;

            case SkillType.SolaDaire:
                // 3 Adımlı Swap (Ters Saat Yönü)
                BoardManager.I.SwapTiles(x, y + 1, x, y);
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y);
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y + 1);
                break;

            case SkillType.YatayAlt:
                BoardManager.I.SwapTiles(x, y, x + 1, y);
                break;

            case SkillType.YatayUst:
                BoardManager.I.SwapTiles(x, y + 1, x + 1, y + 1);
                break;

            case SkillType.DikeySol:
                BoardManager.I.SwapTiles(x, y, x, y + 1);
                break;

            case SkillType.DikeySag:
                BoardManager.I.SwapTiles(x + 1, y, x + 1, y + 1);
                break;

            default:
                Debug.LogWarning("Bilinmeyen Skill: " + tile.currentSkill);
                break;
        }

        
    }
}