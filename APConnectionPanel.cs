using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PikunikuAPMod;

public class APConnectionPanel : UI_MenuPanel
{
    // Set by CreateFor before UI is built.
    internal UI_MenuPanel_Aventure AventurePanel;

    private TMP_InputField _hostnameField;
    private TMP_InputField _portField;
    private TMP_InputField _slotField;
    private TMP_InputField _passwordField;
    private TextMeshProUGUI _statusText;

    private TMP_FontAsset _gameFont;
    private Sprite _bgSprite;

    // Manual navigation: a focused TMP_InputField swallows the EventSystem's move/submit events,
    // so we drive selection ourselves to move off a focused field.
    private NavVisual[] _nav;
    private int _navIndex;

    // Cached visuals for one navigable item (also disambiguates the Shadow from the Outline).
    private class NavVisual
    {
        public Selectable    Selectable;
        public RectTransform Rect;
        public Image         Background;
        public Outline       Outline;
        public Shadow        Shadow;

        // Animation state
        public Vector2 CurrentOffset     = Vector2.zero;
        public Vector2 CurrentShadowDist = Vector2.zero;
        public float CurrentShadowAlpha;
    }

    // Base-game selection feel: selected box pops out and casts a solid bottom-right shadow.
    private static readonly Color   NormalBg          = Color.white;
    private static readonly Color   NormalOutline     = Color.black;
    private static readonly Vector2 NormalOutlineDist = new Vector2(4f, -4f);
    private static readonly Color   SolidShadowColor  = new Color(0f, 0f, 0f, 1f);
    private static readonly Vector2 SolidShadowDist   = new Vector2(8f, -8f);
    private static readonly Vector2 SelectedOffset    = new Vector2(-4f, 4f);

    public static APConnectionPanel Instance { get; private set; }

    // ---- Factory — called once from the UI_PauseMenu.Launch patch ----

    public static APConnectionPanel CreateFor(UI_PauseMenu pauseMenu, UI_MenuPanel_Aventure aventurePanel)
    {
        if (Instance != null) return Instance;

        // Sibling of the adventure panel so it shares the canvas root and slide animation.
        Transform parent = aventurePanel.selfRect.parent;

        var go = new GameObject("APConnectionPanel");
        var rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, worldPositionStays: false);

        // Match the adventure panel's X layout, but use an explicit centred height: copying its
        // Y-stretch anchors would give height=0 (and a culled canvas) under a rect-less parent.
        var src = aventurePanel.selfRect;

        var parentRT = parent.GetComponent<RectTransform>();
        float panelH = (parentRT != null && parentRT.rect.height > 1f)
                        ? parentRT.rect.height
                        : 720f;

        rt.anchorMin        = new Vector2(src.anchorMin.x, 0.5f);
        rt.anchorMax        = new Vector2(src.anchorMax.x, 0.5f);
        rt.pivot            = src.pivot;
        rt.sizeDelta        = new Vector2(src.sizeDelta.x, panelH);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale       = src.localScale;
        rt.localPosition    = src.localPosition;
        rt.localRotation    = src.localRotation;

        // Copy the GO layer so the UI camera's culling mask includes our panel.
        go.layer = aventurePanel.gameObject.layer;

        // Copy the adventure panel's Canvas settings so our coordinate system matches.
        var srcCanvas = aventurePanel.selfCanvas;
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode      = srcCanvas.renderMode;
        canvas.sortingOrder    = srcCanvas.sortingOrder;
        canvas.sortingLayerID  = srcCanvas.sortingLayerID;
        canvas.overrideSorting = srcCanvas.overrideSorting;
        canvas.pixelPerfect    = srcCanvas.pixelPerfect;
        if (srcCanvas.renderMode == RenderMode.ScreenSpaceCamera ||
            srcCanvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = srcCanvas.worldCamera;
        canvas.enabled = false;

        // Copy CanvasScaler — it defines the reference resolution font sizes/heights are relative to.
        var srcScaler = aventurePanel.selfCanvas.GetComponent<CanvasScaler>();
        if (srcScaler != null)
        {
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = srcScaler.uiScaleMode;
            scaler.referenceResolution  = srcScaler.referenceResolution;
            scaler.screenMatchMode      = srcScaler.screenMatchMode;
            scaler.matchWidthOrHeight   = srcScaler.matchWidthOrHeight;
            scaler.referencePixelsPerUnit = srcScaler.referencePixelsPerUnit;
        }

        go.AddComponent<GraphicRaycaster>();

        var cg = go.AddComponent<CanvasGroup>();
        cg.interactable = false;

        var panel = go.AddComponent<APConnectionPanel>();

        // Populate the UI_MenuPanel fields that SwitchToPanel reads.
        panel.manager      = pauseMenu;
        panel.selfRect     = rt;
        panel.selfCanvas   = canvas;
        panel.canvasGroup  = cg;
        panel.FullGameObject = go;
        panel.allowBackButton = true;
        panel.AventurePanel = aventurePanel;

        // UnityEvent fields are null on dynamically created MonoBehaviours.
        panel.OnBackButton     = new UnityEngine.Events.UnityEvent();
        panel.OnPreEnableEvent = new UnityEngine.Events.UnityEvent();
        panel.OnEnableEvent    = new UnityEngine.Events.UnityEvent();
        panel.OnDisableEvent   = new UnityEngine.Events.UnityEvent();

        panel.OnBackButton.AddListener(() => pauseMenu.SwitchToPanelFromBack(panel));

        // Clear the status label each time the panel is about to appear.
        panel.OnPreEnableEvent.AddListener(() =>
        {
            if (panel._statusText != null) panel._statusText.text = string.Empty;
        });

        panel.GrabGameStyle(aventurePanel);
        panel.BuildUI();

        // Hook AP events once; they persist for the panel's lifetime.
        var ap = PikunikuAPMod.ArchipelagoHandler;
        if (ap != null)
        {
            ap.OnConnected       += panel.OnConnected;
            ap.OnConnectionFailed += panel.OnConnectionFailed;
        }

        Instance = panel;
        Log.Info("APConnectionPanel created as sibling of adventure panel.");
        return panel;
    }

    private void OnDestroy()
    {
        var ap = PikunikuAPMod.ArchipelagoHandler;
        if (ap != null)
        {
            ap.OnConnected       -= OnConnected;
            ap.OnConnectionFailed -= OnConnectionFailed;
        }
        Instance = null;
    }

    // ---- Navigation ----

    // Polled every frame by UI_PauseMenu; works even while a field is focused.
    public override void CustomPanelUpdate()
    {
        UpdateAnimation();

        if (_nav == null || _nav.Length == 0) return;

        // Stay in sync if focus changed via mouse click or anything external.
        SyncIndexFromEventSystem();

        bool down = Input.GetKeyDown(KeyCode.DownArrow) || RewiredNavDown();
        bool up   = Input.GetKeyDown(KeyCode.UpArrow)   || RewiredNavUp();

        // Tab / Shift+Tab as a keyboard convenience.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) up = true;
            else                                                                     down = true;
        }

        if (down)    Move(+1);
        else if (up) Move(-1);

        ApplyHighlight();
    }

    private void Move(int delta)
    {
        int n = _nav.Length;
        int next = _navIndex;
        for (int step = 0; step < n; step++)
        {
            next = (next + delta + n) % n;
            if (_nav[next]?.Selectable != null && _nav[next].Selectable.IsInteractable()) break;
        }
        SelectIndex(next);
    }

    private void SelectIndex(int i)
    {
        if (_nav == null || i < 0 || i >= _nav.Length) return;

        // Deactivate any currently-focused field so it stops trapping input.
        foreach (var v in _nav)
            if (v?.Selectable is TMP_InputField f && f.isFocused) f.DeactivateInputField();

        _navIndex = i;
        var sel = _nav[i].Selectable;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(sel.gameObject);

        // Activate input fields so the keyboard can type into them immediately.
        if (sel is TMP_InputField field)
        {
            field.ActivateInputField();
            field.MoveTextEnd(false);
        }

        ApplyHighlight();
    }

    private void SyncIndexFromEventSystem()
    {
        var cur = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (cur == null) return;
        for (int k = 0; k < _nav.Length; k++)
            if (_nav[k]?.Selectable != null && _nav[k].Selectable.gameObject == cur) { _navIndex = k; return; }
    }

    private void ApplyHighlight()
    {
        // Now just a stub for backwards compatibility if needed, 
        // but the animation loop handles the actual work.
    }

    private void UpdateAnimation()
    {
        if (_nav == null) return;

        float dt = Time.unscaledDeltaTime;
        float offsetSpeed = 60f;      // pixels per second
        float shadowSpeed = 120f;     // units per second
        float alphaSpeed  = 10f;      // units per second

        for (int k = 0; k < _nav.Length; k++)
        {
            var v = _nav[k];
            if (v == null) continue;
            bool on = (k == _navIndex);

            // Targets
            Vector2 targetOffset     = on ? SelectedOffset    : Vector2.zero;
            Vector2 targetShadowDist = on ? SolidShadowDist   : Vector2.zero;
            float   targetShadowAlpha = on ? 1f : 0f;

            // Linear Interpolate
            v.CurrentOffset      = Vector2.MoveTowards(v.CurrentOffset, targetOffset, dt * offsetSpeed);
            v.CurrentShadowDist  = Vector2.MoveTowards(v.CurrentShadowDist, targetShadowDist, dt * shadowSpeed);
            v.CurrentShadowAlpha = Mathf.MoveTowards(v.CurrentShadowAlpha, targetShadowAlpha, dt * alphaSpeed);

            // Apply
            if (v.Rect != null)
            {
                v.Rect.anchoredPosition = v.CurrentOffset;
                v.Rect.localScale = Vector3.one;
            }

            if (v.Shadow != null)
            {
                bool visible = v.CurrentShadowAlpha > 0.01f;
                v.Shadow.enabled = visible;
                if (visible)
                {
                    var col = SolidShadowColor;
                    col.a = v.CurrentShadowAlpha;
                    v.Shadow.effectColor = col;
                    v.Shadow.effectDistance = v.CurrentShadowDist;
                }
            }

            // Outline and Background are static in the current style
            if (v.Outline != null)
            {
                v.Outline.effectColor = NormalOutline;
                v.Outline.effectDistance = NormalOutlineDist;
                v.Outline.enabled = true;
            }
            if (v.Background != null) v.Background.color = NormalBg;
        }
    }

    // Builds the cached visual references for one navigable item, disambiguating
    // the real Shadow from the Outline (Outline derives from Shadow).
    private static NavVisual BuildNavVisual(Selectable sel)
    {
        var go = sel.gameObject;
        var nv = new NavVisual
        {
            Selectable = sel,
            Rect       = go.GetComponent<RectTransform>(),
            Background = go.GetComponent<Image>(),
            Outline    = go.GetComponent<Outline>(),
        };
        foreach (var s in go.GetComponents<Shadow>())
            if (s is not Outline) { nv.Shadow = s; break; }
        return nv;
    }

    // Enter in a field advances to the next item (lands on Connect after Password).
    private void WireSubmitAdvance(TMP_InputField field, int indexOfField)
    {
        field.onSubmit.AddListener(_ => SelectIndex(Mathf.Min(indexOfField + 1, _nav.Length - 1)));
    }

    // --- Rewired controller navigation (UIVertical axis action) ---------------

    private static Rewired.Player GetRewiredPlayer()
    {
        if (PlayerManager.S == null) return null;
        var p = PlayerManager.S.GetPlayer(0);
        return p != null ? p.GetRewiredPlayer() : null;
    }

    private static bool RewiredNavUp()
    {
        var p = GetRewiredPlayer();
        return p != null && p.GetButtonDown("UIVertical");          // positive = up
    }

    private static bool RewiredNavDown()
    {
        var p = GetRewiredPlayer();
        return p != null && p.GetNegativeButtonDown("UIVertical");  // negative = down
    }

    // -------------------------------------------------------------------------
    // AP event handlers
    // -------------------------------------------------------------------------

    private void OnConnected()
    {
        // Connect() just repointed the game's save slot to this seed. Force the adventure
        // panel to recompute Continue/New Game against it (it caches the result once), so a
        // fresh seed correctly hides Continue instead of showing a stale one.
        if (AventurePanel != null)
        {
            AventurePanel.hasCheckedForAdventure = false;
            AventurePanel.CheckMainStory();
        }
        manager.SwitchToPanel(AventurePanel);
    }

    private void OnConnectionFailed(string error)
    {
        if (_statusText != null)
            _statusText.text = "Failed: " + error;
    }

    // -------------------------------------------------------------------------
    // UI construction
    // -------------------------------------------------------------------------

    private void GrabGameStyle(UI_MenuPanel_Aventure aventurePanel)
    {
        var tmp = aventurePanel.selfCanvas.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        if (tmp != null) _gameFont = tmp.font;

        var allBtns = Resources.FindObjectsOfTypeAll<UI_Button>();
        foreach (var btn in allBtns)
        {
            if (btn.img_background?.sprite != null)
            {
                _bgSprite = btn.img_background.sprite;
                break;
            }
        }

        Log.Info($"APConnectionPanel style: font={(_gameFont != null ? _gameFont.name : "null")}, sprite={(_bgSprite != null ? _bgSprite.name : "null")}");
    }

    private void BuildUI()
    {
        var last     = FileWriter.ReadLastConnection();
        string host  = Fallback(last?.Host,     "archipelago.gg");
        string port  = Fallback(last?.Port,     "38281");
        string slot  = Fallback(last?.SlotName, "");
        string pass  = last?.Password ?? "";

        // Centered vertical-layout container.
        var container = MakeRect("Container", transform);
        var cRT = container.GetComponent<RectTransform>();
        cRT.anchorMin = cRT.anchorMax = cRT.pivot = new Vector2(0.5f, 0.5f);
        cRT.anchoredPosition = new Vector2(0f, 15f);
        cRT.sizeDelta = new Vector2(560f, 660f);

        var vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment     = TextAnchor.UpperCenter;
        vlg.spacing            = 10f;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        AddSpacer(container.transform, 15f);
        AddTitle(container.transform, "Archipelago");
        AddSpacer(container.transform, 5f);
        AddHorizontalLine(container.transform, 8f);
        AddSpacer(container.transform, 10f);
        _hostnameField = AddInputRow(container.transform, "Host",      host);
        _portField     = AddInputRow(container.transform, "Port",      port);
        _slotField     = AddInputRow(container.transform, "Slot Name", slot);
        _passwordField = AddInputRow(container.transform, "Password",  pass, isPassword: true);
        
        AddSpacer(container.transform, 25f);
        var connectBtn = AddConnectButton(container.transform);
        _statusText    = AddStatusLabel(container.transform);

        // Ordered navigation set, driven manually in CustomPanelUpdate.
        Selectable[] items = { _hostnameField, _portField, _slotField, _passwordField, connectBtn };
        _nav = new NavVisual[items.Length];
        for (int i = 0; i < items.Length; i++)
            _nav[i] = BuildNavVisual(items[i]);

        // Disable Unity's own navigation & color transitions on every item — we
        // handle both movement and the selection highlight ourselves so a focused
        // field can't hijack or block them.
        foreach (var v in _nav)
        {
            var nav = v.Selectable.navigation;
            nav.mode = Navigation.Mode.None;
            v.Selectable.navigation = nav;
            v.Selectable.transition = Selectable.Transition.None;
            if (v.Shadow != null) v.Shadow.enabled = false; // off until selected
        }

        // Enter in a field advances to the next item.
        WireSubmitAdvance(_hostnameField, 0);
        WireSubmitAdvance(_portField,     1);
        WireSubmitAdvance(_slotField,     2);
        WireSubmitAdvance(_passwordField, 3);

        // UI_MenuPanel auto-selects this when the slide-in transition ends.
        toSelectFirst = _hostnameField.gameObject;
        _navIndex = 0;
        ApplyHighlight();
    }

    private void AddTitle(Transform parent, string text)
    {
        var go  = MakeRect("Title", parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = 65f;
        tmp.color      = Color.black;
        tmp.fontStyle  = FontStyles.Normal;
        tmp.alignment  = TextAlignmentOptions.BottomRight;
        ApplyFont(tmp);
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor    = new Color(1f, 1f, 1f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);
        go.AddComponent<LayoutElement>().preferredHeight = 65f;
    }

    private TMP_InputField AddInputRow(Transform parent, string label,
        string defaultValue, bool isPassword = false)
    {
        var rowGO = MakeRect("Row_" + label, parent);
        var rowVLG = rowGO.AddComponent<VerticalLayoutGroup>();
        rowVLG.childAlignment = TextAnchor.UpperLeft;
        rowVLG.spacing = 2f;
        rowVLG.childForceExpandWidth = true;
        rowVLG.childForceExpandHeight = false;
        rowGO.AddComponent<LayoutElement>().preferredHeight = 90f;

        var labelGO = MakeRect("Label_" + label, rowGO.transform);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = label;
        labelTMP.fontSize  = 30f;
        labelTMP.color     = Color.black;
        labelTMP.alignment = TextAlignmentOptions.Left;
        ApplyFont(labelTMP);
        labelGO.AddComponent<LayoutElement>().preferredHeight = 30f;

        var bgWrapperGO = MakeRect("FieldWrapper_" + label, rowGO.transform);
        bgWrapperGO.AddComponent<LayoutElement>().preferredHeight = 52f;

        var bgGO  = MakeRect("Field_" + label, bgWrapperGO.transform);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = Color.white;
        if (_bgSprite != null) { bgImg.sprite = _bgSprite; bgImg.type = Image.Type.Sliced; }
        
        // Shadow and Outline are managed by the animation loop.
        var bgShadow = bgGO.AddComponent<Shadow>();
        bgShadow.enabled = false;
        var bgOutline = bgGO.AddComponent<Outline>();
        bgOutline.effectColor    = Color.black;
        bgOutline.effectDistance = new Vector2(4f, -4f);
        // bgGO is no longer the layout element, the wrapper is.
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.3f, 0f);
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        var viewport = MakeRect("Viewport", bgGO.transform);
        var vpRT     = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = new Vector2(12f,  6f);
        vpRT.offsetMax = new Vector2(-12f, -6f);
        viewport.AddComponent<RectMask2D>();

        var textGO  = MakeRect("Text", viewport.transform);
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.fontSize  = 26f;
        textTMP.color     = Color.black;
        textTMP.alignment = TextAlignmentOptions.Left;
        ApplyFont(textTMP);
        FillParent(textGO.GetComponent<RectTransform>());

        var phGO  = MakeRect("Placeholder", viewport.transform);
        var phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text      = label.ToLower() + "...";
        phTMP.fontSize  = 26f;
        phTMP.color     = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.alignment = TextAlignmentOptions.Left;
        ApplyFont(phTMP);
        FillParent(phGO.GetComponent<RectTransform>());

        var field = bgGO.AddComponent<TMP_InputField>();
        field.textViewport  = vpRT;
        field.textComponent = textTMP;
        field.placeholder   = phTMP;
        field.text          = defaultValue;
        field.caretColor    = Color.black;
        field.selectionColor = new Color(0.3f, 0.6f, 1f, 0.4f);
        if (isPassword) field.inputType = TMP_InputField.InputType.Password;

        return field;
    }

    private Button AddConnectButton(Transform parent)
    {
        var wrapperGO = MakeRect("ButtonWrapper_Connect", parent);
        wrapperGO.AddComponent<LayoutElement>().preferredHeight = 65f;

        var go  = MakeRect("Button_Connect", wrapperGO.transform);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = Color.white;
        if (_bgSprite != null) { img.sprite = _bgSprite; img.type = Image.Type.Sliced; }

        var outline = go.AddComponent<Outline>();
        outline.effectColor    = Color.black;
        outline.effectDistance = new Vector2(4f, -4f);

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor    = new Color(0f, 0f, 0f, 0.25f);
        shadow.effectDistance = new Vector2(3f, -3f);

        var labelGO  = MakeRect("Label", go.transform);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = "Connect";
        labelTMP.fontSize  = 30f;
        labelTMP.color     = Color.black;
        labelTMP.fontStyle = FontStyles.Normal;
        labelTMP.alignment = TextAlignmentOptions.Center;
        ApplyFont(labelTMP);
        FillParent(labelGO.GetComponent<RectTransform>());

        go.AddComponent<LayoutElement>().preferredHeight = 65f;

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(OnConnectClicked);
        
        return btn;
    }

    private TextMeshProUGUI AddStatusLabel(Transform parent)
    {
        var go  = MakeRect("Status", parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text              = string.Empty;
        tmp.fontSize          = 20f;
        tmp.color             = new Color(0.75f, 0.1f, 0.1f);
        tmp.alignment         = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        ApplyFont(tmp);
        go.AddComponent<LayoutElement>().preferredHeight = 55f;
        return tmp;
    }

    private void AddSpacer(Transform parent, float height)
    {
        var go  = MakeRect("Spacer", parent);
        var img = go.AddComponent<Image>();
        img.color          = Color.clear;
        img.raycastTarget  = false;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight       = height;
    }

    private void AddHorizontalLine(Transform parent, float thickness, float widthPadding = 0f)
    {
        var go  = MakeRect("Line", parent);
        var img = go.AddComponent<Image>();
        img.color = Color.black;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = thickness;
        le.minHeight       = thickness;

        if (widthPadding > 0f)
        {
            var rt = go.GetComponent<RectTransform>();
        }
    }


    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, worldPositionStays: false);
        return go;
    }

    private static void FillParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private void ApplyFont(TextMeshProUGUI tmp)
    {
        if (_gameFont != null) tmp.font = _gameFont;
    }

    private static string Fallback(string value, string fallback)
        => string.IsNullOrEmpty(value) ? fallback : value;

    // -------------------------------------------------------------------------
    // Connect logic
    // -------------------------------------------------------------------------

    private void OnConnectClicked()
    {
        string hostname = _hostnameField.text.Trim();
        string portStr  = _portField.text.Trim();
        string slot     = _slotField.text.Trim();
        string pass     = _passwordField.text;

        if (string.IsNullOrEmpty(slot)) { _statusText.text = "Enter a slot name!"; return; }
        if (!int.TryParse(portStr, out int port)) port = 38281;

        _statusText.text = "Connecting…";

        try
        {
            var ap = PikunikuAPMod.ArchipelagoHandler;
            ap.CreateSession(hostname, port, slot, pass);
            ap.Connect();
            FileWriter.WriteLastConnection(hostname, port, slot, pass);
        }
        catch (System.Exception ex)
        {
            _statusText.text = "Error: " + ex.Message;
            Log.Error($"Connect failed: {ex}");
        }
    }
}
