using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("UI Transforms & Prefabs")]
    [SerializeField] private Transform p1HandContainer;
    [SerializeField] private Transform p2HandContainer;
    [SerializeField] private Transform graveyardTransform;
    [SerializeField] private GameObject cardPrefab;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI p1ScoreText;
    [SerializeField] private TextMeshProUGUI p2ScoreText;
    [SerializeField] private TextMeshProUGUI spellDeckCountText;
    [SerializeField] private TextMeshProUGUI whiteStockCountText;

    private List<Card> spellDeck = new List<Card>();
    private List<Card> graveyard = new List<Card>();
    private int whiteCardStock = 50;

    public PlayerData[] players = new PlayerData[2];
    public int currentPlayerIndex { get; private set; } = 0;
    private bool isCpuMode = true;

    private void Awake() => Instance = this;

    public void InitGame(bool isCpu)
    {
        isCpuMode = isCpu;
        resultPanel.SetActive(false);
        gamePanel.SetActive(true);

        spellDeck.Clear();
        graveyard.Clear();
        whiteCardStock = 50;

        GenerateSpellDeck();

        players[0] = new PlayerData { id = 0, isCpu = false };
        players[1] = new PlayerData { id = 1, isCpu = isCpuMode };

        currentPlayerIndex = Random.Range(0, 2);

        for (int i = 0; i < 2; i++)
        {
            players[0].hand.Add(DrawSpellCard());
            players[1].hand.Add(DrawSpellCard());
        }

        UpdateBoardUI();
        StartTurn();
    }

    private void StartTurn()
    {
        if (spellDeck.Count > 0)
        {
            players[currentPlayerIndex].hand.Add(DrawSpellCard());
        }

        UpdateBoardUI();

        if (players[currentPlayerIndex].isCpu)
        {
            StartCoroutine(CpuTurnRoutine());
        }
    }

    public void OnCardSelected(int handIndex)
    {
        if (players[currentPlayerIndex].isCpu) return;

        // SE再生
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCardPlaySE();

        PlayCardProcess(handIndex);
    }

    private void PlayCardProcess(int handIndex)
    {
        PlayerData user = players[currentPlayerIndex];
        PlayerData target = players[(currentPlayerIndex + 1) % 2];

        Card card = user.hand[handIndex];
        user.hand.RemoveAt(handIndex);

        // 選択されたUIを取得して演出再生
        Transform currentContainer = (currentPlayerIndex == 0) ? p1HandContainer : p2HandContainer;
        if (handIndex < currentContainer.childCount)
        {
            CardUI selectedCardUI = currentContainer.GetChild(handIndex).GetComponent<CardUI>();
            if (selectedCardUI != null)
            {
                // 画面中央（墓地位置）に向かってポップアップして消える
                selectedCardUI.PlayCardAnimation(graveyardTransform, () =>
                {
                    ExecuteSpellEffect(card, user, target);
                    graveyard.Add(card);

                    UpdateBoardUI();

                    if (CheckWinCondition()) return;

                    currentPlayerIndex = (currentPlayerIndex + 1) % 2;
                    StartTurn();
                });
                return;
            }
        }

        // UI取得に失敗した場合はそのまま処理
        ExecuteSpellEffect(card, user, target);
        graveyard.Add(card);
        UpdateBoardUI();

        if (CheckWinCondition()) return;

        currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        StartTurn();
    }

    private void ExecuteSpellEffect(Card card, PlayerData user, PlayerData target)
    {
        switch (card.effectType)
        {
            case SpellEffectType.AddWhite:
                GainWhiteCard(user, (int)card.effectValue);
                break;

            case SpellEffectType.MultiplyWhite:
                int current = user.whiteCardCount;
                int targetAmount = Mathf.FloorToInt(current * card.effectValue);
                GainWhiteCard(user, targetAmount - current);
                break;

            case SpellEffectType.StealWhite:
                int steal = Mathf.Min(target.whiteCardCount, (int)card.effectValue);
                target.whiteCardCount -= steal;
                user.whiteCardCount += steal;
                break;

            case SpellEffectType.DestroyWhite:
                int destroyAmount = Mathf.Min(target.whiteCardCount, (int)card.effectValue);
                target.whiteCardCount -= destroyAmount;
                whiteCardStock += destroyAmount;
                break;

            case SpellEffectType.SwapWhite:
                int tempWhite = user.whiteCardCount;
                user.whiteCardCount = target.whiteCardCount;
                target.whiteCardCount = tempWhite;
                break;

            case SpellEffectType.SpellChange:
                List<Card> tempHand = new List<Card>(user.hand);
                user.hand = new List<Card>(target.hand);
                target.hand = tempHand;
                break;

            case SpellEffectType.RecycleHand:
                if (graveyard.Count > 0 && user.hand.Count > 0)
                {
                    int discardIdx = Random.Range(0, user.hand.Count);
                    graveyard.Add(user.hand[discardIdx]);
                    user.hand.RemoveAt(discardIdx);

                    int recycleIdx = Random.Range(0, graveyard.Count);
                    user.hand.Add(graveyard[recycleIdx]);
                    graveyard.RemoveAt(recycleIdx);
                }
                break;
        }
    }

    private void GainWhiteCard(PlayerData user, int amount)
    {
        int actualGain = Mathf.Min(whiteCardStock, amount);
        user.whiteCardCount += actualGain;
        whiteCardStock -= actualGain;
    }

    private bool CheckWinCondition()
    {
        // 表記用プレイヤー名の設定
        string p1Name = isCpuMode ? "あなた" : "1P";
        string p2Name = isCpuMode ? "CPU" : "2P";

        // 1. KO判定 (26枚以上)
        if (players[0].whiteCardCount >= 26) { ShowResult($"{p1Name} のKO勝利！"); return true; }
        if (players[1].whiteCardCount >= 26) { ShowResult($"{p2Name} のKO勝利！"); return true; }

        // 2. 山札切れ判定
        if (spellDeck.Count == 0 && currentPlayerIndex == 1)
        {
            if (players[0].whiteCardCount > players[1].whiteCardCount)
                ShowResult($"山札終了：{p1Name} の勝利！");
            else if (players[1].whiteCardCount > players[0].whiteCardCount)
                ShowResult($"山札終了：{p2Name} の勝利！");
            else
                ShowResult($"山札終了（同数）：先攻により {p1Name} の勝利！");
            return true;
        }

        return false;
    }

    private void ShowResult(string message)
    {
        resultText.text = message;
        resultPanel.SetActive(true);
    }

    private void UpdateBoardUI()
    {
        string p1Label = isCpuMode ? "YOU" : "1P";
        string p2Label = isCpuMode ? "CPU" : "2P";

        if (p1ScoreText != null) p1ScoreText.text = $"[{p1Label}]:{players[0].whiteCardCount}";
        if (p2ScoreText != null) p2ScoreText.text = $"[{p2Label}]:{players[1].whiteCardCount}";
        if (spellDeckCountText != null) spellDeckCountText.text = $"SPELL:{spellDeck.Count}";
        if (whiteStockCountText != null) whiteStockCountText.text = $"WHITE:{whiteCardStock}";

        GenerateHandUI(p1HandContainer, players[0], currentPlayerIndex == 0 && !players[0].isCpu);
        GenerateHandUI(p2HandContainer, players[1], currentPlayerIndex == 1 && !players[1].isCpu);
    }

    private void GenerateHandUI(Transform container, PlayerData player, bool isCurrentTurn)
    {
        if (container == null || cardPrefab == null) return;
        foreach (Transform child in container) Destroy(child.gameObject);

        for (int i = 0; i < player.hand.Count; i++)
        {
            GameObject obj = Instantiate(cardPrefab, container);
            CardUI cardUI = obj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(player.hand[i], i, isCurrentTurn, OnCardSelected);
            }
        }
    }

    private Card DrawSpellCard()
    {
        if (spellDeck.Count == 0) return null;
        Card c = spellDeck[0];
        spellDeck.RemoveAt(0);
        return c;
    }

    private void GenerateSpellDeck()
    {
        AddCardsToDeck("WC+1", "ホワイトカード+1枚", SpellEffectType.AddWhite, 1, 12);
        AddCardsToDeck("WC+2", "ホワイトカード+2枚", SpellEffectType.AddWhite, 2, 8);
        AddCardsToDeck("WC+3", "ホワイトカード+3枚", SpellEffectType.AddWhite, 3, 4);
        AddCardsToDeck("WC*1.5", "ホワイトカード1.5倍", SpellEffectType.MultiplyWhite, 1.5f, 4);
        AddCardsToDeck("WC強奪", "相手からホワイトカードを1枚奪う", SpellEffectType.StealWhite, 1, 6);
        AddCardsToDeck("WC破棄", "相手のホワイトカードを2枚破棄", SpellEffectType.DestroyWhite, 2, 6);
        AddCardsToDeck("逆転", "お互いのホワイトカードを交換", SpellEffectType.SwapWhite, 0, 3);
        AddCardsToDeck("チェンジ", "お互いのスペルカードを交換", SpellEffectType.SpellChange, 0, 4);
        AddCardsToDeck("リサイクル", "ランダムなスペルカード1枚と墓地の1枚を交換", SpellEffectType.RecycleHand, 0, 3);

        for (int i = 0; i < spellDeck.Count; i++)
        {
            Card temp = spellDeck[i];
            int rand = Random.Range(i, spellDeck.Count);
            spellDeck[i] = spellDeck[rand];
            spellDeck[rand] = temp;
        }
    }

    private void AddCardsToDeck(string name, string desc, SpellEffectType type, float val, int count)
    {
        for (int i = 0; i < count; i++)
        {
            spellDeck.Add(new Card { cardName = name, description = desc, effectType = type, effectValue = val });
        }
    }

    private IEnumerator CpuTurnRoutine()
    {
        yield return new WaitForSeconds(1.2f);
        PlayCardProcess(0);
    }
}

// --- プレイヤーデータ定義 ---
[System.Serializable]
public class PlayerData
{
    public int id;
    public bool isCpu;
    public int whiteCardCount = 0;
    public List<Card> hand = new List<Card>();
}