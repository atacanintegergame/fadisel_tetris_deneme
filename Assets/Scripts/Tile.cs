using UnityEngine; // Unity'nin temel kütüphanesini dahil et (MonoBehaviour vb. için)

// Bu sýnýf, sahnedeki her bir kutunun (hayvanýn) özelliklerini tutar
public class Tile : MonoBehaviour
{
    public int x; // Bu kutunun ýzgaradaki (Grid) yatay sýra numarasý
    public int y; // Bu kutunun ýzgaradaki dikey sýra numarasý
    public AnimalType myType; // Bu kutunun hangi hayvan türünde olduðunu tutan deðiþken (Zürafa mý, Kurt mu?)

    private SpriteRenderer spriteRenderer; // Resim çizici bileþenine eriþmek için referans

    // Obje ilk uyandýðýnda (oyun baþlamadan hemen önce) çalýþýr
    private void Awake()
    {
        // Bu objenin üzerindeki 'SpriteRenderer' bileþenini bul ve deðiþkene ata
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // BoardManager tarafýndan çaðrýlan kurulum fonksiyonu
    // gridX: Yatay konum, gridY: Dikey konum, type: Hayvan türü, sprite: Hangi resim olacaðý
    public void Setup(int gridX, int gridY, AnimalType type, Sprite sprite)
    {
        x = gridX; // Gelen yatay konumu kaydet
        y = gridY; // Gelen dikey konumu kaydet
        myType = type; // Gelen hayvan türünü kaydet

        // Eðer SpriteRenderer bileþeni baþarýyla bulunduysa
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite; // Resim çizicinin resmini, gelen hayvan resmiyle deðiþtir
        }

        // Objenin Hiyerarþi panelindeki adýný deðiþtir (Örn: Tile_2_3) ki bulmasý kolay olsun
        name = $"Tile_{x}_{y}";
    }
}