using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardImage;

    [Header("Hover Animation Settings")]
    [SerializeField] private float hoverMoveUpDistance = 30f; // 上に上がる距離
    [SerializeField] private float animationSpeed = 15f;      // アニメーション速度

    public int HandIndex { get; private set; }
    private System.Action<int> onCardClicked;
    private bool isInteractable = false;

    private RectTransform rectTransform;
    private float defaultY;
    private float targetY;
    private Coroutine moveCoroutine;
    private bool isInitialized = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(Card card, int index, bool interactable, System.Action<int> onClick)
    {
        HandIndex = index;
        isInteractable = interactable;
        onCardClicked = onClick;

        if (nameText != null) nameText.text = card.cardName;
        if (descriptionText != null) descriptionText.text = card.description;

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (isInteractable) onCardClicked?.Invoke(HandIndex);
            });
            button.interactable = interactable;
        }

        // Layout Groupが位置を決定するのを1フレーム待ってから初期位置(Y座標)を取得する
        StartCoroutine(InitDefaultPositionNextFrame());
    }

    private IEnumerator InitDefaultPositionNextFrame()
    {
        yield return null; // 1フレーム待機（Layout Groupの自動配置完了を待つ）
        if (rectTransform != null)
        {
            defaultY = rectTransform.anchoredPosition.y;
            targetY = defaultY;
            isInitialized = true;
        }
    }

    // --- マウスホバー処理 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || !isInitialized) return;

        targetY = defaultY + hoverMoveUpDistance;
        StartMoveAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || !isInitialized) return;

        targetY = defaultY;
        StartMoveAnimation();
    }

    private void StartMoveAnimation()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateYPosition());
    }

    private IEnumerator AnimateYPosition()
    {
        while (Mathf.Abs(rectTransform.anchoredPosition.y - targetY) > 0.1f)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            float newY = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * animationSpeed);
            rectTransform.anchoredPosition = new Vector2(currentPos.x, newY);
            yield return null;
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, targetY);
    }

    // --- カードプレイ（場に出す）時のアニメーション ---
    public void PlayCardAnimation(Transform targetTransform, System.Action onComplete)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        StartCoroutine(AnimatePlay(targetTransform, onComplete));
    }

    private IEnumerator AnimatePlay(Transform target, System.Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = target != null ? target.position : startPos;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * 1.2f;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        onComplete?.Invoke();
    }
}