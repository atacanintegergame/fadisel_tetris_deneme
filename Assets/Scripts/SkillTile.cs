using UnityEngine;

public class SkillTile : MonoBehaviour
{
    public int x; // Hangi aralýkta olduðunu bilmek için
    public int y;

    // Týklandýðýnda veya üzerine bir þey býrakýldýðýnda çalýþacak
    // (Þimdilik boþ, sonra dolduracaðýz)
    public void Setup(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
        name = $"SkillTile_{x}_{y}";
    }
}