using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyPickupFeedback : MonoBehaviour
{
    [SerializeField] private GameManager _run;
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private Image _coinIcon;
    [SerializeField] private Camera _worldCamera;

    private const float TossDuration = 0.16f;
    private const float PulseDuration = 0.22f;

    private readonly List<FlyingCoin> _coins = new();
    private readonly Stack<FlyingCoin> _coinPool = new();
    private RectTransform _flightParent;
    private Canvas _canvas;
    private Quaternion _restRotation;
    private Vector3 _restScale;
    private int _pendingReward;
    private int? _shownBalance;
    private int _spreadIndex;
    private float _pulseTimeLeft;

    public bool HasActiveFeedback => _coins.Count > 0 || _pulseTimeLeft > 0f;

    private sealed class FlyingCoin
    {
        public RectTransform View;
        public Vector2 Origin;
        public Vector2 TossEnd;
        public int Reward;
        public float TravelDuration;
        public float Elapsed;
    }

    private void Awake()
    {
        _flightParent = (RectTransform)transform.parent;
        _canvas = _coinIcon.canvas.rootCanvas;
        _restRotation = transform.localRotation;
        _restScale = transform.localScale;
    }

    private void OnEnable()
    {
        RefreshBalance();
        GameEvents.OnBlockRewarded += LaunchCoin;
    }

    private void OnDisable()
    {
        GameEvents.OnBlockRewarded -= LaunchCoin;

        foreach (FlyingCoin coin in _coins)
        {
            ReleaseCoin(coin);
        }

        _coins.Clear();
        _pendingReward = 0;
        ResetPulse();
    }

    private void OnDestroy()
    {
        foreach (FlyingCoin coin in _coinPool)
        {
            if (coin.View != null) Destroy(coin.View.gameObject);
        }
    }

    private void RefreshBalance()
    {
        int visibleBalance = _run.RewardPerSession - _pendingReward;
        if (_shownBalance == visibleBalance) return;

        _shownBalance = visibleBalance;
        _balanceText.text = visibleBalance.ToString();
    }

    private void LaunchCoin(Vector2 worldPosition, int reward)
    {
        Vector3 screenPoint = _worldCamera.WorldToScreenPoint(worldPosition);
        Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        if (screenPoint.z <= 0f ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(_flightParent, screenPoint, uiCamera, out Vector2 origin))
        {
            return;
        }

        FlyingCoin coin = _coinPool.Count > 0 ? _coinPool.Pop() : CreateCoin();
        RectTransform view = coin.View;
        view.sizeDelta = _coinIcon.rectTransform.rect.size * 0.8f;
        view.localPosition = origin;
        view.localScale = Vector3.one;
        view.SetAsLastSibling();

        int spread = _spreadIndex - 2;
        _spreadIndex = (_spreadIndex + 1) % 5;
        coin.Origin = origin;
        coin.TossEnd = origin + new Vector2(spread * 17f, 40f + Mathf.Abs(spread) * 6f);
        coin.Reward = reward;
        coin.TravelDuration = 0.42f + Mathf.Abs(spread) * 0.04f;
        coin.Elapsed = 0f;
        view.gameObject.SetActive(true);
        _coins.Add(coin);
        _pendingReward += reward;
    }

    private FlyingCoin CreateCoin()
    {
        var obj = new GameObject("Flying Coin", typeof(RectTransform), typeof(Image));
        obj.layer = gameObject.layer;
        var image = obj.GetComponent<Image>();
        image.sprite = _coinIcon.sprite;
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.rectTransform.SetParent(_flightParent, false);
        return new FlyingCoin { View = image.rectTransform };
    }

    private void ReleaseCoin(FlyingCoin coin)
    {
        if (coin.View == null) return;
        coin.View.gameObject.SetActive(false);
        _coinPool.Push(coin);
    }

    private void LateUpdate()
    {
        float deltaTime = Time.unscaledDeltaTime;
        UpdatePulse(deltaTime);
        UpdateCoins(deltaTime);
        RefreshBalance();
    }

    private void UpdateCoins(float deltaTime)
    {
        if (_coins.Count == 0) return;

        RectTransform icon = _coinIcon.rectTransform;
        Vector2 target = _flightParent.InverseTransformPoint(icon.TransformPoint(icon.rect.center));

        for (int i = _coins.Count - 1; i >= 0; i--)
        {
            FlyingCoin coin = _coins[i];
            coin.Elapsed += deltaTime;
            if (coin.Elapsed < TossDuration)
            {
                float remaining = 1f - coin.Elapsed / TossDuration;
                coin.View.localPosition = Vector2.Lerp(coin.Origin, coin.TossEnd, 1f - remaining * remaining);
                continue;
            }

            float progress = Mathf.Clamp01((coin.Elapsed - TossDuration) / coin.TravelDuration);
            float travel = progress * progress;
            Vector2 position = Vector2.Lerp(coin.TossEnd, target, travel);
            position.y += 110f * travel * (1f - travel);
            coin.View.localPosition = position;
            coin.View.localScale = Vector3.one * Mathf.Lerp(1f, 0.45f, travel);

            if (progress < 1f) continue;

            _pendingReward -= coin.Reward;
            _coins.RemoveAt(i);
            ReleaseCoin(coin);
            _pulseTimeLeft = PulseDuration;
        }
    }

    private void UpdatePulse(float deltaTime)
    {
        if (_pulseTimeLeft <= 0f) return;

        _pulseTimeLeft -= deltaTime;
        if (_pulseTimeLeft <= 0f)
        {
            ResetPulse();
            return;
        }

        float remaining = _pulseTimeLeft / PulseDuration;
        float wave = Mathf.Sin((1f - remaining) * Mathf.PI * 3f) * remaining * remaining;
        transform.localRotation = _restRotation * Quaternion.Euler(0f, 0f, wave * 3f);
        transform.localScale = _restScale * (1f + wave * 0.08f);
    }

    private void ResetPulse()
    {
        _pulseTimeLeft = 0f;
        transform.localRotation = _restRotation;
        transform.localScale = _restScale;
    }
}
