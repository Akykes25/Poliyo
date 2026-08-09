using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Poliyo.Presentation.Editor
{
/// <summary>
/// Shared construction primitives for the authored UGUI vertical-slice screens.
/// The palette is provisional, but the hierarchy, interaction states and 1920x1080
/// reference frame are intentional scene contracts.
/// </summary>
internal static class PoliyoUiTheme
{
    public const float ReferenceWidth = 1920f;
    public const float ReferenceHeight = 1080f;

    public static readonly Color Ivory = new Color32(246, 241, 231, 255);
    public static readonly Color Paper = new Color32(235, 226, 211, 255);
    public static readonly Color Ink = new Color32(23, 35, 59, 255);
    public static readonly Color InkSoft = new Color32(43, 58, 87, 255);
    public static readonly Color MutedInk = new Color32(92, 99, 115, 255);
    public static readonly Color Coral = new Color32(255, 90, 95, 255);
    public static readonly Color Turquoise = new Color32(24, 182, 164, 255);
    public static readonly Color Yellow = new Color32(242, 201, 76, 255);
    public static readonly Color Blue = new Color32(77, 163, 255, 255);
    public static readonly Color Violet = new Color32(118, 87, 214, 255);
    public static readonly Color White = new Color32(255, 255, 255, 255);

    public static GameObject CreateCanvas(string name, out EventSystem eventSystem)
    {
        var canvasObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));
        eventSystem = eventSystemObject.GetComponent<EventSystem>();
        eventSystem.sendNavigationEvents = true;
        return canvasObject;
    }

    public static Image CreatePanel(Transform parent, string name, Color color, bool raycastTarget = false)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    public static Image CreateFullScreenBackground(Transform parent, string name = "Background")
    {
        Image background = CreatePanel(parent, name, Ivory);
        Stretch(background.rectTransform, 0f, 0f, 0f, 0f);
        return background;
    }

    public static TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color,
        Vector2 anchoredPosition,
        Vector2 size,
        FontStyles fontStyle = FontStyles.Normal)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        // The bundled TMP font has no U+2026 glyph. Unity falls back to Truncate
        // after logging on every rebuild, so make the effective behavior explicit.
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        SetRect(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        return text;
    }

    public static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        PoliyoButtonStyle style = PoliyoButtonStyle.Secondary,
        float fontSize = 21f)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);

        Image image = buttonObject.GetComponent<Image>();
        image.raycastTarget = true;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

        ButtonPalette palette = GetButtonPalette(style);
        var colors = new ColorBlock
        {
            normalColor = palette.Normal,
            highlightedColor = palette.Highlighted,
            pressedColor = palette.Pressed,
            selectedColor = palette.Selected,
            disabledColor = palette.Disabled,
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };
        button.colors = colors;
        // ColorTint is applied through the CanvasRenderer. Keeping the source graphic
        // white avoids multiplying the palette color by itself at runtime.
        image.color = Color.white;
        button.enabled = false;
        button.enabled = true;

        var outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = palette.Outline;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;

        TMP_Text text = CreateText(
            buttonObject.transform,
            "Label",
            label,
            fontSize,
            TextAlignmentOptions.Center,
            palette.Text,
            Vector2.zero,
            size,
            FontStyles.Bold);
        Fill(text.rectTransform);
        return button;
    }

    public static Image CreateChip(
        Transform parent,
        string name,
        string label,
        Color background,
        Color foreground,
        Vector2 position,
        Vector2 size,
        float fontSize = 17f)
    {
        Image chip = CreatePanel(parent, name, background);
        SetRect(chip.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        TMP_Text text = CreateText(
            chip.transform,
            "Label",
            label,
            fontSize,
            TextAlignmentOptions.Center,
            foreground,
            Vector2.zero,
            size,
            FontStyles.Bold);
        Fill(text.rectTransform);
        return chip;
    }

    public static Image CreateRule(
        Transform parent,
        string name,
        Color color,
        Vector2 position,
        Vector2 size)
    {
        Image rule = CreatePanel(parent, name, color);
        SetRect(rule.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        return rule;
    }

    public static CampaignChrome CreateCampaignChrome(
        Transform parent,
        string screenTitle,
        string screenDeck,
        CampaignNavigationSelection selection)
    {
        Image masthead = CreatePanel(parent, "CampaignChrome", Ink);
        SetRect(
            masthead.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            Vector2.zero,
            new Vector2(ReferenceWidth, 132f));

        CreateRule(masthead.transform, "CoralRule", Coral, Vector2.zero, new Vector2(ReferenceWidth, 10f));
        CreateText(masthead.transform, "Brand", "POLIYO", 35f, TextAlignmentOptions.Left, Yellow, new Vector2(52f, -30f), new Vector2(210f, 52f), FontStyles.Bold);
        CreateText(masthead.transform, "ScreenTitle", screenTitle, 28f, TextAlignmentOptions.Left, Ivory, new Vector2(274f, -25f), new Vector2(470f, 44f), FontStyles.Bold);
        CreateText(masthead.transform, "ScreenDeck", screenDeck, 17f, TextAlignmentOptions.Left, new Color32(198, 207, 223, 255), new Vector2(276f, -72f), new Vector2(570f, 32f));
        CreateChip(masthead.transform, "CampaignModeChip", "CAMPAÑA NACIONAL", Violet, White, new Vector2(840f, -42f), new Vector2(210f, 34f), 15f);

        Button calendar = CreateButton(
            masthead.transform,
            "CalendarNavigationButton",
            "Calendario",
            new Vector2(1090f, -37f),
            new Vector2(174f, 56f),
            selection == CampaignNavigationSelection.Calendar ? PoliyoButtonStyle.SelectedNavigation : PoliyoButtonStyle.Navigation,
            19f);
        Button map = CreateButton(
            masthead.transform,
            "MapNavigationButton",
            "Mapa",
            new Vector2(1280f, -37f),
            new Vector2(144f, 56f),
            selection == CampaignNavigationSelection.Map ? PoliyoButtonStyle.SelectedNavigation : PoliyoButtonStyle.Navigation,
            19f);
        Button team = CreateButton(
            masthead.transform,
            "TeamNavigationButton",
            "Equipo",
            new Vector2(1440f, -37f),
            new Vector2(144f, 56f),
            selection == CampaignNavigationSelection.Team ? PoliyoButtonStyle.SelectedNavigation : PoliyoButtonStyle.Navigation,
            19f);
        Button campaign = CreateButton(
            masthead.transform,
            "CampaignNavigationButton",
            "Mesa central",
            new Vector2(1600f, -37f),
            new Vector2(252f, 56f),
            selection == CampaignNavigationSelection.Campaign ? PoliyoButtonStyle.SelectedNavigation : PoliyoButtonStyle.Navigation,
            19f);

        SetSelectedState(calendar, selection == CampaignNavigationSelection.Calendar);
        SetSelectedState(map, selection == CampaignNavigationSelection.Map);
        SetSelectedState(team, selection == CampaignNavigationSelection.Team);
        SetSelectedState(campaign, selection == CampaignNavigationSelection.Campaign);
        return new CampaignChrome(calendar, map, team, campaign);
    }

    public static void SetFirstSelected(EventSystem eventSystem, Selectable selectable)
    {
        if (eventSystem == null || selectable == null)
        {
            return;
        }

        eventSystem.firstSelectedGameObject = selectable.gameObject;
    }

    public static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    public static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(left, bottom);
        rect.anchorMax = new Vector2(1f - right, 1f - top);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static void Fill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    public static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private static void SetSelectedState(Button button, bool selected)
    {
        if (selected)
        {
            button.interactable = false;
        }
    }

    private static ButtonPalette GetButtonPalette(PoliyoButtonStyle style)
    {
        switch (style)
        {
            case PoliyoButtonStyle.Primary:
                return new ButtonPalette(Coral, Yellow, new Color32(219, 66, 73, 255), Yellow, new Color32(194, 187, 173, 255), Ink, WithAlpha(Ink, 0.42f));
            case PoliyoButtonStyle.Quiet:
                return new ButtonPalette(Paper, Yellow, new Color32(219, 209, 191, 255), Yellow, new Color32(210, 204, 191, 255), Ink, WithAlpha(Ink, 0.42f));
            case PoliyoButtonStyle.Danger:
                return new ButtonPalette(Ivory, new Color32(255, 205, 205, 255), Coral, Yellow, Paper, Ink, WithAlpha(Coral, 0.85f));
            case PoliyoButtonStyle.Navigation:
                return new ButtonPalette(InkSoft, Violet, Ink, Violet, InkSoft, Ivory, WithAlpha(Ivory, 0.18f));
            case PoliyoButtonStyle.SelectedNavigation:
                return new ButtonPalette(Turquoise, Yellow, Blue, Yellow, Turquoise, Ink, WithAlpha(Ivory, 0.28f));
            default:
                return new ButtonPalette(Ink, Violet, new Color32(70, 49, 143, 255), Violet, InkSoft, Ivory, WithAlpha(Ink, 0.45f));
        }
    }

    private readonly struct ButtonPalette
    {
        public ButtonPalette(Color normal, Color highlighted, Color pressed, Color selected, Color disabled, Color text, Color outline)
        {
            Normal = normal;
            Highlighted = highlighted;
            Pressed = pressed;
            Selected = selected;
            Disabled = disabled;
            Text = text;
            Outline = outline;
        }

        public Color Normal { get; }
        public Color Highlighted { get; }
        public Color Pressed { get; }
        public Color Selected { get; }
        public Color Disabled { get; }
        public Color Text { get; }
        public Color Outline { get; }
    }
}

internal enum PoliyoButtonStyle
{
    Primary,
    Secondary,
    Quiet,
    Danger,
    Navigation,
    SelectedNavigation
}

internal enum CampaignNavigationSelection
{
    None,
    Calendar,
    Map,
    Team,
    Campaign
}

internal readonly struct CampaignChrome
{
    public CampaignChrome(Button calendar, Button map, Button team, Button campaign)
    {
        Calendar = calendar;
        Map = map;
        Team = team;
        Campaign = campaign;
    }

    public Button Calendar { get; }
    public Button Map { get; }
    public Button Team { get; }
    public Button Campaign { get; }
}
}
