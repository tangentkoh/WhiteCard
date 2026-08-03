using UnityEngine;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject manualPanel;
    [SerializeField] private GameObject resultPanel; // リザルト非表示化用に追加

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource seAudioSource;

    [Header("Audio UI Texts")]
    [SerializeField] private TextMeshProUGUI bgmButtonText;
    [SerializeField] private TextMeshProUGUI seButtonText;

    public static bool IsCpuMode { get; private set; } = true;

    private void Start()
    {
        ShowTitle();
        UpdateAudioUI();
    }

    // --- ボタン用メソッド ---

    // CPU対戦ボタン
    public void OnClickCpuGame()
    {
        IsCpuMode = true;
        StartGame();
    }

    // リアル対戦ボタン
    public void OnClickHumanGame()
    {
        IsCpuMode = false;
        StartGame();
    }

    // タイトルへ戻るボタン（リザルト画面などから呼び出す）
    public void OnClickToTitle()
    {
        ShowTitle();
    }

    // --- 画面切り替え内部処理 ---

    private void ShowTitle()
    {
        titlePanel.SetActive(true);
        gamePanel.SetActive(false);
        manualPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void StartGame()
    {
        titlePanel.SetActive(false);
        gamePanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);

        // モード判定を渡してGameManager側でCPU戦・リアル対戦を分岐開始
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitGame(IsCpuMode);
        }
    }

    // マニュアル開閉
    public void OnClickOpenManual() => manualPanel.SetActive(true);
    public void OnClickCloseManual() => manualPanel.SetActive(false);

    // --- サウンド切り替え ---

    public void OnClickToggleBGM()
    {
        if (bgmAudioSource == null) return;
        bgmAudioSource.mute = !bgmAudioSource.mute;
        UpdateAudioUI();
    }

    public void OnClickToggleSE()
    {
        if (seAudioSource == null) return;
        seAudioSource.mute = !seAudioSource.mute;
        UpdateAudioUI();
    }

    private void UpdateAudioUI()
    {
        if (bgmButtonText != null && bgmAudioSource != null)
            bgmButtonText.text = bgmAudioSource.mute ? "BGM: OFF" : "BGM: ON";

        if (seButtonText != null && seAudioSource != null)
            seButtonText.text = seAudioSource.mute ? "SE: OFF" : "SE: ON";
    }
}