using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PikunikuAPMod;

/// <summary>
/// Bottom-left overlay showing recent Archipelago log lines, each with a drop shadow and fade.
/// AP events may arrive on the socket thread, so handlers only enqueue; UI work happens in Update().
/// </summary>
public class ClientView : MonoBehaviour
{
    private const float MessageLifetime = 7.5f;
    private const float FadeIn = 0.25f;
    private const float FadeOut = 0.6f;
    private const float PanelWidth = 440f;
    private const float PanelHeight = 300f;
    private const float FontSize = 18f;

    private static readonly Color   ShadowColor    = new Color(0f, 0f, 0f, 0.9f);
    private static readonly Vector2 ShadowDistance = new Vector2(1f, -1f);

    private Canvas _canvas;
    private RectTransform _content;
    private TMP_FontAsset _font;
    private bool _built;

    private volatile bool _wantVisible;

    private class Message
    {
        public GameObject Go;
        public CanvasGroup Cg;
        public float Born;
        public float Expiry;
    }

    private readonly List<Message> _messages = new();
    private readonly Queue<string> _incoming = new();
    private readonly object _lock = new();

    private void Awake()
    {
        var ap = PikunikuAPMod.ArchipelagoHandler;
        if (ap != null)
        {
            ap.OnLogMessage   += OnLogMessage;
            ap.OnConnected    += OnConnected;
            ap.OnDisconnected += OnDisconnected;
        }
    }

    private void OnDestroy()
    {
        var ap = PikunikuAPMod.ArchipelagoHandler;
        if (ap != null)
        {
            ap.OnLogMessage   -= OnLogMessage;
            ap.OnConnected    -= OnConnected;
            ap.OnDisconnected -= OnDisconnected;
        }
    }

    private void OnLogMessage(string message)
    {
        lock (_lock) _incoming.Enqueue(message);
    }

    private void OnConnected()    => _wantVisible = true;
    private void OnDisconnected() => _wantVisible = false;

    private void Update()
    {
        // Build lazily once visible, so the game's TMP font exists by then.
        if (_wantVisible && !_built)
            Build();

        if (!_built)
            return;

        if (_canvas.enabled != _wantVisible)
            _canvas.enabled = _wantVisible;

        // Unscaled time so lines still expire/animate while paused.
        float now = Time.unscaledTime;

        lock (_lock)
        {
            while (_incoming.Count > 0)
                AddMessage(_incoming.Dequeue(), now);
        }

        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            var m = _messages[i];
            if (now >= m.Expiry)
            {
                if (m.Go != null) Destroy(m.Go);
                _messages.RemoveAt(i);
                continue;
            }
            if (m.Cg != null) m.Cg.alpha = ComputeAlpha(m, now);
        }
    }

    private static float ComputeAlpha(Message m, float now)
    {
        float a = 1f;
        float age = now - m.Born;
        if (age < FadeIn) a = age / FadeIn;
        float remaining = m.Expiry - now;
        if (remaining < FadeOut) a = Mathf.Min(a, remaining / FadeOut);
        return Mathf.Clamp01(a);
    }

    private void AddMessage(string text, float now)
    {
        // TMP honours neither the UI Shadow effect nor (with the game's font shader) its own underlay,
        // so the shadow is a second copy. The stripped-tag shadow is the layout child (drives wrapping);
        // the white text is a child drawn on top, nudged a pixel up-left to reveal the shadow.
        var shadowGo = MakeRect("Msg", _content);
        var shadowTmp = ConfigureText(shadowGo);
        shadowTmp.color = ShadowColor;
        shadowTmp.text = StripColorTags(text);

        var textGo = MakeRect("Text", shadowGo.transform);
        var tmp = ConfigureText(textGo);
        tmp.color = Color.white;
        tmp.text = text;

        // Set the rect AFTER ConfigureText: adding the TMP graphic resets the RectTransform.
        var textRt = (RectTransform)textGo.transform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = new Vector2(-ShadowDistance.x, -ShadowDistance.y);

        var cg = shadowGo.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        _messages.Add(new Message { Go = shadowGo, Cg = cg, Born = now, Expiry = now + MessageLifetime });
    }

    private TextMeshProUGUI ConfigureText(GameObject go)
    {
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = FontSize;
        tmp.richText = true;
        tmp.enableWordWrapping = true;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        tmp.raycastTarget = false;
        if (_font != null) tmp.font = _font;
        return tmp;
    }

    // Messages only carry <color> tags; stripping them keeps glyph positions identical.
    private static string StripColorTags(string text)
        => Regex.Replace(text, "</?color[^>]*>", string.Empty);

    private void Build()
    {
        _font = FindGameFont();

        var go = new GameObject("AP_ClientView");
        DontDestroyOnLoad(go);

        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 1000; // above the game's own UI

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        // Fixed-size clip region in the bottom-left.
        var panel = MakeRect("Panel", go.transform);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = prt.pivot = Vector2.zero;
        prt.anchoredPosition = new Vector2(14f, 14f);
        prt.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        panel.AddComponent<RectMask2D>(); // clip older lines above

        var contentGO = MakeRect("Content", panel.transform);
        _content = contentGO.GetComponent<RectTransform>();
        _content.anchorMin = _content.anchorMax = _content.pivot = Vector2.zero;
        _content.anchoredPosition = new Vector2(10f, 8f);
        _content.sizeDelta = new Vector2(PanelWidth - 20f, 0f);

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.spacing = 3f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        _canvas.enabled = _wantVisible;
        _built = true;
        Log.Info($"AP client view built (font={(_font != null ? _font.name : "default")}).");
    }

    private static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, worldPositionStays: false);
        return go;
    }

    // Grab the game's TMP font from any loaded text element.
    private static TMP_FontAsset FindGameFont()
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
            if (t.font != null)
                return t.font;
        return null;
    }
}
