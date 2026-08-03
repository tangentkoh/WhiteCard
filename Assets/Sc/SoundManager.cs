using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource seAudioSource;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip cardPlayClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ボタンクリック用SE
    public void PlayButtonClickSE()
    {
        if (seAudioSource != null && buttonClickClip != null)
        {
            seAudioSource.PlayOneShot(buttonClickClip);
        }
    }

    // カード使用時SE
    public void PlayCardPlaySE()
    {
        if (seAudioSource != null && cardPlayClip != null)
        {
            seAudioSource.PlayOneShot(cardPlayClip);
        }
    }
}