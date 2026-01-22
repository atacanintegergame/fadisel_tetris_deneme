using UnityEngine;
using System.Collections.Generic;

// Hayvan Türlerimiz (Resimdeki sýraya göre ekleyebilirsin)
public enum AnimalType
{
    Bos = 0,
    Zurafa,   // 1
    Kurt,     // 2
    Aslan,    // 3
    Domuz,    // 4
    Kopek,    // 5
    Kedi,     // 6
    Tilki,    // 7
    Kopek2,   // 8
    Kedi2,    // 9
    Maymun,   // 10
    Koyun,    // 11
    Ayi       // 12
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "Board/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Tasarýmý")]
    public List<RowData> rows = new List<RowData>();
}

[System.Serializable]
public struct RowData
{
    public List<AnimalType> columns;
}