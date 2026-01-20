using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 轨迹跟踪系统的UI控制器
/// </summary>
public class TrajectoryUIController : MonoBehaviour
{
    [Header("References")]
    public TrajectoryFollowingSystem trajectorySystem;

    [Header("UI Elements")]
    public Button startButton;
    public Button stopButton;
    public Button resetButton;
    public Button exportButton;
    public TMP_Dropdown trajectoryTypeDropdown;
    public Toggle showReferenceToggle;
    public Toggle showUserTrailToggle;
    public Slider trailTimeSlider;
    public TMP_Text trailTimeText;

    [Header("Stats Display")]
    public TMP_Text averageErrorText;
    public TMP_Text maxErrorText;
    public TMP_Text completionText;
    public TMP_Text recordingStatusText;
    public TMP_Text recordedPointsText;

    [Header("Settings")]
    public Color recordingColor = Color.red;
    public Color idleColor = Color.gray;
    public float updateInterval = 0.2f; // 统计信息更新间隔

    private float lastUpdateTime;

    void Start()
    {
        if (trajectorySystem == null)
        {
            trajectorySystem = FindObjectOfType<TrajectoryFollowingSystem>();
        }

        SetupUI();
        UpdateUI();
    }

    void SetupUI()
    {
        // 按钮事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (stopButton != null)
        {
            stopButton.onClick.AddListener(OnStopButtonClicked);
            stopButton.interactable = false;
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }

        if (exportButton != null)
        {
            exportButton.onClick.AddListener(OnExportButtonClicked);
        }

        // 轨迹类型下拉菜单
        if (trajectoryTypeDropdown != null)
        {
            trajectoryTypeDropdown.ClearOptions();
            trajectoryTypeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "圆形", "螺旋", "波浪", "8字形", "方形"
            });
            trajectoryTypeDropdown.onValueChanged.AddListener(OnTrajectoryTypeChanged);
        }

        // 显示开关
        if (showReferenceToggle != null)
        {
            showReferenceToggle.isOn = true;
            showReferenceToggle.onValueChanged.AddListener(OnShowReferenceToggled);
        }

        if (showUserTrailToggle != null)
        {
            showUserTrailToggle.isOn = true;
            showUserTrailToggle.onValueChanged.AddListener(OnShowUserTrailToggled);
        }

        // 轨迹时间滑块
        if (trailTimeSlider != null)
        {
            trailTimeSlider.minValue = 1f;
            trailTimeSlider.maxValue = 30f;
            trailTimeSlider.value = trajectorySystem != null ? trajectorySystem.userTrailTime : 10f;
            trailTimeSlider.onValueChanged.AddListener(OnTrailTimeChanged);
            UpdateTrailTimeText(trailTimeSlider.value);
        }
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateUI()
    {
        if (trajectorySystem == null) return;

        // 更新录制状态
        if (recordingStatusText != null)
        {
            recordingStatusText.text = trajectorySystem.isRecording ? "录制中..." : "未录制";
            recordingStatusText.color = trajectorySystem.isRecording ? recordingColor : idleColor;
        }

        // 更新统计信息
        if (averageErrorText != null)
        {
            averageErrorText.text = $"平均误差: {trajectorySystem.AverageError:F3}";
        }

        if (maxErrorText != null)
        {
            maxErrorText.text = $"最大误差: {trajectorySystem.MaxError:F3}";
        }

        if (completionText != null)
        {
            completionText.text = $"完成度: {trajectorySystem.CompletionPercentage:F1}%";
        }

        // 更新按钮状态
        if (startButton != null)
        {
            startButton.interactable = !trajectorySystem.isRecording;
        }

        if (stopButton != null)
        {
            stopButton.interactable = trajectorySystem.isRecording;
        }
    }

    void OnStartButtonClicked()
    {
        if (trajectorySystem != null)
        {
            trajectorySystem.StartRecording();
            Debug.Log("UI: 开始录制");
        }
    }

    void OnStopButtonClicked()
    {
        if (trajectorySystem != null)
        {
            trajectorySystem.StopRecording();
            Debug.Log("UI: 停止录制");
        }
    }

    void OnResetButtonClicked()
    {
        if (trajectorySystem != null)
        {
            trajectorySystem.ResetTrajectory();
            Debug.Log("UI: 重置轨迹");
        }
    }

    void OnExportButtonClicked()
    {
        if (trajectorySystem != null)
        {
            trajectorySystem.ExportData();
            Debug.Log("UI: 导出数据");
        }
    }

    void OnTrajectoryTypeChanged(int index)
    {
        if (trajectorySystem != null)
        {
            TrajectoryFollowingSystem.TrajectoryType type = (TrajectoryFollowingSystem.TrajectoryType)index;
            trajectorySystem.SetTrajectoryType(type);
            Debug.Log($"UI: 切换轨迹类型为 {type}");
        }
    }

    void OnShowReferenceToggled(bool value)
    {
        if (trajectorySystem != null && trajectorySystem.referenceTrajectory != null)
        {
            trajectorySystem.referenceTrajectory.enabled = value;
        }
    }

    void OnShowUserTrailToggled(bool value)
    {
        if (trajectorySystem != null && trajectorySystem.userTrail != null)
        {
            trajectorySystem.userTrail.enabled = value && trajectorySystem.isRecording;
        }
    }

    void OnTrailTimeChanged(float value)
    {
        if (trajectorySystem != null)
        {
            trajectorySystem.userTrailTime = value;
            if (trajectorySystem.userTrail != null)
            {
                trajectorySystem.userTrail.time = value;
            }
        }
        UpdateTrailTimeText(value);
    }

    void UpdateTrailTimeText(float value)
    {
        if (trailTimeText != null)
        {
            trailTimeText.text = $"轨迹时长: {value:F1}秒";
        }
    }
}
