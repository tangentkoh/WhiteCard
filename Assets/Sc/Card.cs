using UnityEngine;

public enum SpellEffectType
{
    None,
    AddWhite,      // 1: ホワイト獲得 (+1, +2, +3)
    MultiplyWhite, // 2: ホワイト倍率 (1.5倍)
    StealWhite,    // 3: 相手から強奪
    DestroyWhite,  // 4: 相手のホワイト破棄
    SwapWhite,     // 5: ホワイト枚数の逆転入れ替え
    SpellChange,   // 6: お互いの手札交換
    RecycleHand    // 7: 手札リサイクル（捨てて回収）
}

[System.Serializable]
public class Card
{
    public string cardName;
    public string description;
    public SpellEffectType effectType;
    public float effectValue; // 獲得枚数や倍率パラメータ
}