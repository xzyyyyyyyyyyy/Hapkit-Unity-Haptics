using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HapkitSampleSceneBootstrap : MonoBehaviour
{
    [Header("Serial")]
    public string initialPort = "COM8";

    [Header("Scene")]
    public bool createCameraLight = true;
    public bool createUI = true;
    public bool createWallVisualization = true;
    public float scale = 0.1f;
    public Vector3 ballStartPosition = Vector3.zero;

    [Header("Geometry (match Processing)")]
    public float dWallPos = 130f;
    public float zWallThick = 10f;
    public float circleRad = 20f;
    public Vector2[] circleCenter = new Vector2[]
    {
        new Vector2(-20, -20),
        new Vector2( 20,  20),
    };

    private void Awake()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        if (createCameraLight)
        {
            if (Camera.main == null)
            {
                var camGO = new GameObject("Main Camera");
                var cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
                camGO.transform.position = new Vector3(0, 0, -10);
                camGO.transform.LookAt(Vector3.zero);
            }

            if (FindObjectOfType<Light>() == null)
            {
                var lightGO = new GameObject("Directional Light");
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
        }

        var serialGO = new GameObject("HapkitSerial");
        var reader = serialGO.AddComponent<SerialHapkitReader>();
        if (!string.IsNullOrWhiteSpace(initialPort))
        {
            reader.SetPort(initialPort);
        }

        var sceneGO = new GameObject("HapkitScene");
        var controller = sceneGO.AddComponent<HapkitSceneController>();
        controller.reader = reader;
        controller.scale = scale;
        controller.dWallPos = dWallPos;
        controller.zWallThick = zWallThick;
        controller.circleRad = circleRad;
        controller.circleCenter = circleCenter;

        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "HapkitBall";
        ball.transform.position = ballStartPosition;
        controller.ball = ball.transform;

        if (createWallVisualization)
        {
            var visGO = new GameObject("HapkitWallVisualizer");
            var vis = visGO.AddComponent<HapkitWallVisualizer>();
            vis.scale = scale;
            vis.dWallPos = dWallPos;
            vis.zWallThick = zWallThick;
            vis.circleRad = circleRad;
            vis.circleCenter = circleCenter;
        }

        if (createUI)
        {
            EnsureEventSystem();
            var canvas = CreateCanvas();
            var inputField = CreateInputField(canvas.transform, "COM8");
            var button = CreateButton(canvas.transform, "Apply Port");

            var ui = canvas.gameObject.AddComponent<SerialPortUI>();
            ui.reader = reader;
            ui.portInput = inputField;
            ui.applyButton = button;
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private Canvas CreateCanvas()
    {
        var canvasGO = new GameObject("HapkitUI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private TMP_InputField CreateInputField(Transform parent, string defaultText)
    {
        var inputGO = new GameObject("PortInput");
        inputGO.transform.SetParent(parent, false);
        var rect = inputGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 32);
        rect.anchoredPosition = new Vector2(140, 80);

        var image = inputGO.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var input = inputGO.AddComponent<TMP_InputField>();

        // Text Area
        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGO.transform, false);
        var textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 6);
        textAreaRect.offsetMax = new Vector2(-10, -6);

        var textAreaMask = textArea.AddComponent<RectMask2D>();

        var placeholder = CreateTMPText(textArea.transform, "COM8", new Color(0.6f, 0.6f, 0.6f, 0.8f));
        placeholder.name = "Placeholder";
        placeholder.alignment = TextAlignmentOptions.Left;

        var text = CreateTMPText(textArea.transform, defaultText, Color.black);
        text.name = "Text";
        text.alignment = TextAlignmentOptions.Left;

        input.textViewport = textAreaRect;
        input.placeholder = placeholder;
        input.textComponent = text;
        input.text = defaultText;

        return input;
    }

    private Button CreateButton(Transform parent, string label)
    {
        var btnGO = new GameObject("ApplyButton");
        btnGO.transform.SetParent(parent, false);
        var rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(140, 32);
        rect.anchoredPosition = new Vector2(350, 80);

        var image = btnGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 0.9f);

        var button = btnGO.AddComponent<Button>();

        var text = CreateTMPText(btnGO.transform, label, Color.white);
        text.alignment = TextAlignmentOptions.Midline;

        return button;
    }

    private TMP_Text CreateTMPText(Transform parent, string value, Color color)
    {
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(parent, false);
        var rect = textGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(6, 2);
        rect.offsetMax = new Vector2(-6, -2);

        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.color = color;
        text.alignment = TextAlignmentOptions.Left;
        return text;
    }
}
