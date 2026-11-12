using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyPickupFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private Image _coinIcon;
    [SerializeField] private Camera _worldCamera;

    private const float TossDuration = 0.16f;
    private const float PulseDuration = 0.22f;

    private readonly List<FlyingCoin> _coins = new();
    private RectTransform _flightParent;
    private Canvas _canvas;
    private Quaternion _restRotation;
    private Vector3 _restScale;
    private int _balance;
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
        _balance = PlayerPrefs.GetInt("PlayerMoney", 0);
        RefreshBalance();
        GameEvents.OnMoneyChanged += UpdateBalance;
        GameEvents.OnBlockRewarded += LaunchCoin;
    }

    private void OnDisable()
    {
        GameEvents.OnMoneyChanged -= UpdateBalance;
        GameEvents.OnBlockRewarded -= LaunchCoin;

        foreach (FlyingCoin coin in _coins)
        {
            if (coin.View != null) Destroy(coin.View.gameObject);
        }

        _coins.Clear();
        _pendingReward = 0;
        ResetPulse();
    }

    private void UpdateBalance(int balance)
    {
        _balance = balance;
    }

    private void RefreshBalance()
    {
        int visibleBalance = _balance - _pendingReward;
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

        var coinObject = new GameObject("Flying Coin", typeof(RectTransform), typeof(Image));
        coinObject.layer = gameObject.layer;

        Image image = coinObject.GetComponent<Image>();
        image.sprite = _coinIcon.sprite;
        image.raycastTarget = false;
        image.preserveAspect = true;

        RectTransform view = image.rectTransform;
        view.SetParent(_flightParent, false);
        view.sizeDelta = _coinIcon.rectTransform.rect.size * 0.8f;
        view.localPosition = origin;

        int spread = _spreadIndex - 2;
        _spreadIndex = (_spreadIndex + 1) % 5;
        _coins.Add(new FlyingCoin
        {
            View = view,
            Origin = origin,
            TossEnd = origin + new Vector2(spread * 17f, 40f + Mathf.Abs(spread) * 6f),
            Reward = reward,
            TravelDuration = 0.42f + Mathf.Abs(spread) * 0.04f
        });
        _pendingReward += reward;
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
            Destroy(coin.View.gameObject);
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
