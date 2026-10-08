using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class WaterUpgradePopupLoader
{
    static WaterUpgradePopupLoader()
    {
        EditorApplication.delayCall += OpenPopupSafely;
    }

    static void OpenPopupSafely()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += OpenPopupSafely;
            return;
        }

        WaterUpgradePopup.Open();
    }
}

public class WaterUpgradePopup : EditorWindow
{
    const string STORE_URL = "https://assetstore.unity.com/packages/slug/essw-easy-setup-stylized-water-2-0-317597";

    // Resources path for the full-size screenshot shown when the hero is
    // clicked. The hero itself stays a flat gradient (no baked-in text/logo
    // clutter) — clicking it is how you get to the actual product shot.
    const string PREVIEW_RESOURCE_PATH = "HoudisoftPromo/cover";

    // Colors
    static readonly Color BG          = new Color(0.027f, 0.051f, 0.090f);
    static readonly Color HERO_TOP    = new Color(0.000f, 0.094f, 0.157f);
    static readonly Color HERO_BOT    = new Color(0.000f, 0.180f, 0.220f);
    static readonly Color ACCENT      = new Color(0.000f, 0.722f, 0.847f);
    static readonly Color ACCENT_DIM  = new Color(0.000f, 0.722f, 0.847f, 0.18f);
    static readonly Color WARN        = new Color(1.000f, 0.800f, 0.267f);
    static readonly Color PANEL       = new Color(1f, 1f, 1f, 0.025f);
    static readonly Color BORDER      = new Color(1f, 1f, 1f, 0.06f);
    static readonly Color TEXT_DIM    = new Color(1f, 1f, 1f, 0.35f);
    static readonly Color TEXT_GHOST  = new Color(1f, 1f, 1f, 0.18f);

    GUIStyle _titleStyle, _heroTitleStyle, _badgeStyle, _warnBadgeStyle;
    GUIStyle _featNameStyle, _featDescStyle, _planLblStyle, _planItemStyle;
    GUIStyle _priceStyle, _priceOldStyle, _priceNoteStyle;
    GUIStyle _btnBuyStyle, _btnSkipStyle, _ratingStyle, _previewHintStyle;
    GUIStyle _trustStyle;
    bool _stylesBuilt;

    Texture2D _previewImage;
    Texture2D[] _previewImages = System.Array.Empty<Texture2D>();
    int _previewIndex;
    double _nextPreviewSwitch;
    const double PREVIEW_INTERVAL = 3.0;

    // ── Entry point ─────────────────────────────────────────────────────────
    public static void Open()
    {
        // Close any previous instance so the popup always opens cleanly.
        var existing = Resources.FindObjectsOfTypeAll<WaterUpgradePopup>();
        foreach (var popup in existing)
        {
            if (popup != null)
                popup.Close();
        }

        var w = GetWindow<WaterUpgradePopup>(true, "Easy Setup Stylized Water — Upgrade to Pro", true);
        w.minSize = new Vector2(680, 820);
        w.maxSize = new Vector2(680, 820);
        w.ShowUtility();
        w.Focus();
        w.Repaint();
    }

    [MenuItem("Window/Houidisoft/Upgrade to Water Pro")]
    [MenuItem("Tools/Houidisoft/Upgrade to Water Pro")]
    public static void MenuUpgradeDirect()
    {
        Application.OpenURL(STORE_URL);
    }

    [MenuItem("Window/Houidisoft/Show Upgrade Popup")]
    [MenuItem("Tools/Houidisoft/Show Upgrade Popup")]
    public static void MenuShowPopup()
    {
        Open();
    }

    void OnEnable()
    {
        LoadPreviewCarousel();
        ResetPreviewTimer();
    }

    void LoadPreviewCarousel()
    {
        _previewImages = Resources.LoadAll<Texture2D>("HoudisoftPromo");

        if (_previewImages == null || _previewImages.Length == 0)
        {
            _previewImage = null;
            return;
        }

        string[] preferredOrder = { "cover", "reflections", "refraction", "foam", "inspector" };

        System.Array.Sort(_previewImages, (a, b) =>
        {
            int ai = System.Array.FindIndex(preferredOrder,
                n => a.name.Equals(n, System.StringComparison.OrdinalIgnoreCase));
            int bi = System.Array.FindIndex(preferredOrder,
                n => b.name.Equals(n, System.StringComparison.OrdinalIgnoreCase));

            if (ai < 0) ai = preferredOrder.Length;
            if (bi < 0) bi = preferredOrder.Length;

            if (ai != bi) return ai.CompareTo(bi);
            return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
        });

        _previewIndex = 0;
        _previewImage = _previewImages[0];
    }

    void ResetPreviewTimer()
    {
        _nextPreviewSwitch = EditorApplication.timeSinceStartup + PREVIEW_INTERVAL;
    }

    void Update()
    {
        if (_previewImages.Length <= 1)
            return;

        if (EditorApplication.timeSinceStartup >= _nextPreviewSwitch)
        {
            _previewIndex = (_previewIndex + 1) % _previewImages.Length;
            _previewImage = _previewImages[_previewIndex];
            ResetPreviewTimer();
            Repaint();
        }
    }


    // ── Style builder ────────────────────────────────────────────────────────
    void BuildStyles()
    {
        if (_stylesBuilt) return;
        _stylesBuilt = true;

        _heroTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 22, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerLeft,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = ACCENT }
        };

        _warnBadgeStyle = new GUIStyle(_badgeStyle)
        {
            normal = { textColor = WARN }
        };

        _featNameStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11, fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _featDescStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10, wordWrap = true,
            normal = { textColor = TEXT_DIM }
        };

        _planLblStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 9, fontStyle = FontStyle.Bold,
            normal = { textColor = TEXT_GHOST }
        };

        _planItemStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10,
            normal = { textColor = TEXT_DIM }
        };

        _priceStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 40, fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _priceOldStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = TEXT_GHOST }
        };

        _priceNoteStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9,
            normal = { textColor = TEXT_GHOST }
        };

        _btnBuyStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16, fontStyle = FontStyle.Bold,
            normal   = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.00f, 0.72f, 0.85f)) },
            hover    = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.00f, 0.81f, 0.96f)) },
            active   = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.00f, 0.60f, 0.72f)) },
            border   = new RectOffset(4,4,4,4),
            padding  = new RectOffset(24,24,14,14)
        };

        _btnSkipStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9, alignment = TextAnchor.MiddleRight,
            normal = { textColor = TEXT_GHOST }
        };

        _ratingStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9,
            normal = { textColor = TEXT_GHOST }
        };

        _previewHintStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9, alignment = TextAnchor.MiddleRight,
            normal = { textColor = new Color(1f, 1f, 1f, 0.75f) }
        };

        _trustStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9,
            normal = { textColor = TEXT_GHOST }
        };
    }

    // ── Main GUI ─────────────────────────────────────────────────────────────
    void OnGUI()
    {
        BuildStyles();

        // Full background
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BG);

        float W = position.width;
        float y = 0;

        // ── Hero ──────────────────────────────────────────────────────────
        // Show the complete screenshot inside the popup.
        float imageH = 360f;
        if (_previewImage != null && _previewImage.height > 0)
        {
            float aspect = _previewImage.width / (float)_previewImage.height;
            imageH = W / aspect;
        }
        imageH = Mathf.Clamp(imageH, 250f, 340f);

        float titleH = 48f;
        float heroH = imageH + titleH;
        Rect heroRect = new Rect(0, y, W, imageH);

        if (_previewImage != null)
        {
            GUI.DrawTexture(heroRect, _previewImage, ScaleMode.ScaleToFit);
        }
        else
        {
            DrawVertGradient(heroRect, HERO_TOP, HERO_BOT);
        }

        // Badges
        float badgeY = y + 14;
        DrawBadge(new Rect(14, badgeY, 130, 20), "FREE VERSION ACTIVE", ACCENT);
        DrawBadge(new Rect(150, badgeY, 130, 20), "UPGRADE AVAILABLE", WARN);

        // Headline is completely below the screenshot.
        Rect titleRect = new Rect(0, y + imageH, W, titleH);
        EditorGUI.DrawRect(titleRect, BG);
        GUI.Label(new Rect(14, titleRect.y + 8, W - 28, 30),
            "Advanced Stylized Foam", _heroTitleStyle);

        // The actual product screenshot is visible in the hero and can be
        // clicked to open the full-size version.
        DrawHeroPreviewHotspot(heroRect);
        DrawPreviewArrows(heroRect);
        DrawPreviewDots(new Rect(0, titleRect.y + 34, W, 8));

        y += heroH + 2;

        // ── Body ──────────────────────────────────────────────────────────
        float pad = 16f;
        float bodyW = W - pad * 2;

        y += 8;

        // Pro-only feature highlights
        float featW = (bodyW - 8) / 2f;
        float featH = 52f;

        DrawFeature(new Rect(pad, y, featW, featH),
            "REAL-TIME PLANAR REFLECTIONS",
            "Dynamic reflections from the scene");

        DrawFeature(new Rect(pad + featW + 8, y, featW, featH),
            "ADVANCED FOAM SYSTEM",
            "Multiple foam styles and controls");

        y += featH + 6;

        DrawFeature(new Rect(pad, y, featW, featH),
            "ORGANIZED INSPECTOR UI",
            "Clean controls built for Unity");

        DrawFeature(new Rect(pad + featW + 8, y, featW, featH),
            "QUALITY & PERFORMANCE",
            "Fine-tune quality for your project");

        y += featH + 10;

        // Separator
        DrawSeparator(new Rect(pad, y, bodyW, 1), "FREE VS PRO");
        y += 12;

        // Compare columns
        float colW = (bodyW - 8) / 2f;
        float colH = 78f;
        DrawPlanFree(new Rect(pad, y, colW, colH));
        DrawPlanPro(new Rect(pad + colW + 8, y, colW, colH));
        y += colH + 10;

        // Purchase bar
        float purchaseH = 168f;
        DrawPurchaseBar(new Rect(pad, y, bodyW, purchaseH));
        y += purchaseH + 8;

        // Footer
        GUI.Label(new Rect(pad, y, 260, 18), "★★★★★  Houidisoft Technology · Unity Asset Store", _ratingStyle);
        if (GUI.Button(new Rect(W - pad - 80, y, 80, 16), "Maybe later", _btnSkipStyle))
            Close();

        // Hover feedback on the hero hotspot needs repaints on mouse move.
        if (Event.current.type == EventType.MouseMove)
            Repaint();
    }

    // ── Draw helpers ─────────────────────────────────────────────────────────
    void DrawVertGradient(Rect r, Color top, Color bot)
    {
        int steps = 32;
        float h = r.height / steps;
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)(steps - 1);
            EditorGUI.DrawRect(new Rect(r.x, r.y + i * h, r.width, h + 1), Color.Lerp(top, bot, t));
        }
    }

    void DrawBadge(Rect r, string label, Color col)
    {
        var borderCol = new Color(col.r, col.g, col.b, 0.45f);
        var bgCol     = new Color(0, 0, 0, 0.5f);
        EditorGUI.DrawRect(r, bgCol);
        // border lines
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), borderCol);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), borderCol);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), borderCol);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), borderCol);

        var s = new GUIStyle(_badgeStyle) { normal = { textColor = col } };
        GUI.Label(r, label, s);
    }

    void DrawFeature(Rect r, string name, string desc)
    {
        EditorGUI.DrawRect(r, PANEL);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 2, r.height), ACCENT); // left accent bar
        // border
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), BORDER);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), BORDER);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), BORDER);

        GUI.Label(new Rect(r.x + 10, r.y + 6,  r.width - 14, 16), name, _featNameStyle);
        GUI.Label(new Rect(r.x + 10, r.y + 24, r.width - 14, 20), desc, _featDescStyle);
    }

    void DrawSeparator(Rect r, string label)
    {
        float lw = (r.width - 90) / 2f;
        EditorGUI.DrawRect(new Rect(r.x, r.y, lw, 1), BORDER);
        EditorGUI.DrawRect(new Rect(r.x + lw + 90, r.y, lw, 1), BORDER);
        GUI.Label(new Rect(r.x + lw, r.y - 6, 90, 14), label, new GUIStyle(_planLblStyle) { alignment = TextAnchor.MiddleCenter });
    }

    void DrawPlanFree(Rect r)
    {
        EditorGUI.DrawRect(r, PANEL);
        DrawBorderRect(r, BORDER);

        var lbl = new GUIStyle(_planLblStyle) { normal = { textColor = TEXT_GHOST } };
        GUI.Label(new Rect(r.x + 10, r.y + 6, r.width - 14, 12), "CURRENT", lbl);

        string[] items = { "✓  Basic stylized water", "✓  Water refraction", "✕  Planar reflections", "✕  Advanced foam system" };
        for (int i = 0; i < items.Length; i++)
        {
            var s = new GUIStyle(_planItemStyle) { normal = { textColor = i == 0 ? TEXT_DIM : TEXT_GHOST } };
            GUI.Label(new Rect(r.x + 10, r.y + 20 + i * 14, r.width - 14, 14), items[i], s);
        }
    }

    void DrawPlanPro(Rect r)
    {
        EditorGUI.DrawRect(r, ACCENT_DIM);
        DrawBorderRect(r, new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.25f));

        // PRO pill
        var pillR = new Rect(r.xMax - 46, r.y + 2, 36, 12);
        EditorGUI.DrawRect(pillR, ACCENT);
        GUI.Label(pillR, "PRO", new GUIStyle(_badgeStyle) { normal = { textColor = Color.black }, fontSize = 8 });

        var lbl = new GUIStyle(_planLblStyle) { normal = { textColor = ACCENT } };
        GUI.Label(new Rect(r.x + 10, r.y + 6, r.width - 60, 12), "UPGRADE — PRO", lbl);

        string[] items = {
            "✓  Real-time planar reflections",
            "✓  Advanced foam system",
            "✓  Extra wave controls",
            "✓  Gradient depth color controls"
        };
        var itemStyle = new GUIStyle(_planItemStyle) { normal = { textColor = new Color(1f,1f,1f,0.82f) } };
        for (int i = 0; i < items.Length; i++)
            GUI.Label(new Rect(r.x + 10, r.y + 20 + i * 14, r.width - 14, 14), items[i], itemStyle);
    }

    void DrawPurchaseBar(Rect r)
    {
        EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.35f));
        DrawBorderRect(r, BORDER);

        // Top accent line
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 2), ACCENT);

        float padX = 14f;
        float contentY = r.y + 10f;

        // Price
        GUI.Label(new Rect(r.x + padX, contentY, 100, 44), "$13", _priceStyle);

        // Old price with strikethrough
        Rect oldPriceRect = new Rect(r.x + padX + 86, contentY + 20, 44, 18);
        GUI.Label(oldPriceRect, "$20", _priceOldStyle);
        EditorGUI.DrawRect(new Rect(oldPriceRect.x, oldPriceRect.y + 9, 34, 1), TEXT_GHOST);

        // Save badge
        var saveRect = new Rect(r.x + padX + 136, contentY + 18, 72, 18);
        EditorGUI.DrawRect(saveRect, new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.12f));
        DrawBorderRect(saveRect, new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.35f));
        GUI.Label(saveRect, "Save 35%", new GUIStyle(_badgeStyle)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = ACCENT }
        });

        // CTA button
        float btnY = contentY + 50;
        float btnH = 48f;
        float btnW = r.width - padX * 2;
        if (GUI.Button(new Rect(r.x + padX, btnY, btnW, btnH), "Upgrade to Pro →", _btnBuyStyle))
        {
            Application.OpenURL(STORE_URL);
            Close();
        }

        // Trust signals
        float trustY = btnY + btnH + 10;
        string[] trusts = { "✓ Lifetime Updates", "✓ Full Source Included", "✓ Built for URP" };
        for (int i = 0; i < trusts.Length; i++)
        {
            GUI.Label(
                new Rect(r.x + padX, trustY + i * 12, r.width - padX * 2, 12),
                trusts[i],
                _trustStyle
            );
        }
    }

    // Clickable hero image. Clicking the preview opens the full-size screenshot.
    void DrawHeroPreviewHotspot(Rect heroRect)
    {
        if (_previewImage == null) return;

        bool hovering = heroRect.Contains(Event.current.mousePosition);
        if (hovering)
        {
            DrawBorderRect(heroRect, new Color(1f, 1f, 1f, 0.28f));
            var hintRect = new Rect(heroRect.xMax - 132, heroRect.yMax - 20, 122, 16);
            GUI.Label(hintRect, "🔍  View screenshot", _previewHintStyle);
        }

        EditorGUIUtility.AddCursorRect(heroRect, MouseCursor.Link);
        if (GUI.Button(heroRect, GUIContent.none, GUIStyle.none))
        {
            WaterPreviewViewer.ShowImage(_previewImage);
        }
    }

    void DrawPreviewArrows(Rect r)
    {
        if (_previewImages.Length <= 1 || !r.Contains(Event.current.mousePosition))
            return;

        var arrowStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 26,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        if (GUI.Button(new Rect(r.x + 6, r.center.y - 18, 32, 36), "‹", arrowStyle))
        {
            _previewIndex = (_previewIndex - 1 + _previewImages.Length) % _previewImages.Length;
            _previewImage = _previewImages[_previewIndex];
            ResetPreviewTimer();
            Repaint();
        }

        if (GUI.Button(new Rect(r.xMax - 38, r.center.y - 18, 32, 36), "›", arrowStyle))
        {
            _previewIndex = (_previewIndex + 1) % _previewImages.Length;
            _previewImage = _previewImages[_previewIndex];
            ResetPreviewTimer();
            Repaint();
        }
    }

    void DrawPreviewDots(Rect r)
    {
        if (_previewImages.Length <= 1) return;

        float dotSize = 6f;
        float gap = 6f;
        float totalW = _previewImages.Length * dotSize + (_previewImages.Length - 1) * gap;
        float startX = r.center.x - totalW * 0.5f;

        for (int i = 0; i < _previewImages.Length; i++)
        {
            Color dotColor = i == _previewIndex ? ACCENT : new Color(1f, 1f, 1f, 0.45f);
            EditorGUI.DrawRect(new Rect(startX + i * (dotSize + gap), r.y, dotSize, dotSize), dotColor);
        }
    }

    void DrawBorderRect(Rect r, Color col)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), col);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), col);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), col);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), col);
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var t = new Texture2D(w, h);
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        t.SetPixels(px);
        t.Apply();
        return t;
    }
}

// Lightweight full-size lightbox for the preview screenshot. Click anywhere
// (or press Escape) to close. Sized to fit within the main Editor window's
// screen bounds without ever upscaling or distorting the source image.
public class WaterPreviewViewer : EditorWindow
{
    Texture2D _image;

    public static void ShowImage(Texture2D tex)
    {
        if (tex == null) return;

        var viewer = CreateInstance<WaterPreviewViewer>();
        viewer._image = tex;
        viewer.titleContent = new GUIContent("Preview");

        Rect main = EditorGUIUtility.GetMainWindowPosition();
        float maxWidth = main.width * 0.85f;
        float maxHeight = main.height * 0.85f;

        float aspect = tex.width / (float)tex.height;
        float width = tex.width;
        float height = tex.height;

        if (width > maxWidth)
        {
            width = maxWidth;
            height = width / aspect;
        }

        if (height > maxHeight)
        {
            height = maxHeight;
            width = height * aspect;
        }

        var pos = new Rect(0, 0, width, height)
        {
            x = main.x + (main.width - width) * 0.5f,
            y = main.y + (main.height - height) * 0.5f,
        };

        viewer.position = pos;
        viewer.ShowUtility();
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.black);

        if (_image != null)
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), _image, ScaleMode.ScaleToFit);

        var e = Event.current;
        if (e.type == EventType.MouseDown || (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape))
        {
            Close();
            e.Use();
        }
    }
}