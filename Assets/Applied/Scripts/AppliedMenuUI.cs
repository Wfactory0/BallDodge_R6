using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// 응용 버전 화면 UI(uGUI). 실행 시 Canvas·EventSystem·버튼을 코드로 만든다.
// 시작 화면 / 키 설정 / 게임 오버 패널과 상단 HUD를 갖고, 버튼을 누르면 GameManager의 메서드를 부른다.
// 마우스 클릭은 Input System UI 모듈을 거쳐 버튼으로 전달된다.
public class AppliedMenuUI : MonoBehaviour
{
    private Font font;
    private Text timeText;
    private Text hintText;
    private GameObject menuPanel;
    private GameObject settingsPanel;
    private GameObject gameOverPanel;
    private Text survivedText;
    private Text settingsMessage;
    private readonly Text[] keyLabels = new Text[AppliedKeyBindings.ActionCount];

    public void Build(AppliedGameManager gm)
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(transform, false);
            // 코드로 추가한 UI 모듈은 클릭·이동 입력 액션이 비어 있을 수 있어 기본 UI 액션을 연결한다.
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        var canvasGo = new GameObject("AppliedCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform root = canvasGo.GetComponent<RectTransform>();

        // HUD
        timeText = CreateText(root, "TimeText", "Time: 0.0s", 40, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(24f, -16f), new Vector2(600f, 60f));
        hintText = CreateText(root, "HintText", "", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(24f, -70f), new Vector2(1200f, 40f));

        // 시작 화면
        menuPanel = CreatePanel(root, "MenuPanel");
        CreateText(menuPanel.transform, "Title", "BALL DODGE", 110, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(1200f, 160f)).color = new Color(1f, 0.85f, 0.3f);
        CreateText(menuPanel.transform, "Sub", "Applied version", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(800f, 50f));
        CreateButton(menuPanel.transform, "Btn_StartGame", "Start Game", new Vector2(0f, 0f), new Vector2(420f, 90f), gm.StartGame);
        CreateButton(menuPanel.transform, "Btn_MenuSettings", "Key Settings", new Vector2(0f, -120f), new Vector2(420f, 90f), gm.OpenSettings);

        // 키 설정
        settingsPanel = CreatePanel(root, "SettingsPanel");
        CreateText(settingsPanel.transform, "Title", "Key Settings", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 330f), new Vector2(800f, 90f));
        for (int i = 0; i < AppliedKeyBindings.ActionCount; i++)
        {
            var action = (AppliedKeyBindings.Action)i;
            float y = 220f - i * 85f;
            CreateText(settingsPanel.transform, "Label_" + action, action.ToString(), 40, TextAnchor.MiddleRight, new Vector2(0.5f, 0.5f), new Vector2(-180f, y), new Vector2(300f, 70f));
            Button keyButton = CreateButton(settingsPanel.transform, "Btn_Key_" + action, "-", new Vector2(150f, y), new Vector2(320f, 70f), () => gm.BeginRebind(action));
            keyLabels[i] = keyButton.GetComponentInChildren<Text>();
        }
        settingsMessage = CreateText(settingsPanel.transform, "Message", "", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -210f), new Vector2(1200f, 50f));
        settingsMessage.color = new Color(1f, 0.9f, 0.5f);
        CreateButton(settingsPanel.transform, "Btn_ResetDefaults", "Reset Defaults", new Vector2(-200f, -300f), new Vector2(360f, 80f), gm.ResetKeysToDefault);
        CreateButton(settingsPanel.transform, "Btn_SettingsBack", "Back", new Vector2(200f, -300f), new Vector2(360f, 80f), gm.CloseSettings);

        // 게임 오버
        gameOverPanel = CreatePanel(root, "GameOverPanel");
        CreateText(gameOverPanel.transform, "Title", "GAME OVER", 130, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1400f, 170f)).color = new Color(1f, 0.25f, 0.2f);
        survivedText = CreateText(gameOverPanel.transform, "Survived", "", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 70f));
        CreateButton(gameOverPanel.transform, "Btn_PlayAgain", "Play Again (New Round)", new Vector2(0f, -60f), new Vector2(560f, 90f), gm.PlayAgain);
        CreateButton(gameOverPanel.transform, "Btn_BackToMenu", "Back to Start Menu", new Vector2(0f, -180f), new Vector2(560f, 90f), gm.BackToMenu);

        HideAllPanels();
    }

    public void ShowMenu() { HideAllPanels(); menuPanel.SetActive(true); }
    public void ShowSettings() { HideAllPanels(); settingsPanel.SetActive(true); }
    public void ShowGameOver(float survivedSeconds)
    {
        HideAllPanels();
        survivedText.text = $"Survived {survivedSeconds:F1}s";
        gameOverPanel.SetActive(true);
    }

    public void HideAllPanels()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void SetTime(float seconds) => timeText.text = $"Time: {seconds:F1}s";
    public void SetHint(string text) => hintText.text = text;
    public void SetSettingsMessage(string text) => settingsMessage.text = text;
    public void SetKeyLabel(AppliedKeyBindings.Action action, string text) => keyLabels[(int)action].text = text;

    private GameObject CreatePanel(Transform parent, string panelName)
    {
        var panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
        return panel;
    }

    private Text CreateText(Transform parent, string textName, string content, int size, TextAnchor alignment, Vector2 anchor, Vector2 position, Vector2 boxSize)
    {
        var go = new GameObject(textName, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = boxSize;
        var text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(Transform parent, string buttonName, string label, Vector2 position, Vector2 size, UnityAction onClick)
    {
        var go = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = new Color(0.22f, 0.24f, 0.3f, 0.95f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.45f, 0.5f, 0.65f);
        colors.pressedColor = new Color(0.65f, 0.7f, 0.9f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        Text text = CreateText(go.transform, "Label", label, 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.sizeDelta = Vector2.zero;
        return button;
    }
}
