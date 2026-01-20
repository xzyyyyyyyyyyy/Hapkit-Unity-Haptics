using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 轨迹跟踪系统 - 显示参考轨迹并记录用户操作轨迹
/// </summary>
public class TrajectoryFollowingSystem : MonoBehaviour
{
    [Header("References")]
    public Transform ball; // 用户操作的小球
    public LineRenderer referenceTrajectory; // 参考轨迹渲染器
    public TrailRenderer userTrail; // 用户操作轨迹

    [Header("Reference Trajectory Settings")]
    public TrajectoryType trajectoryType = TrajectoryType.Circle;
    public float trajectoryScale = 1f; // 轨迹缩放比例
    public int trajectoryResolution = 100; // 轨迹点数
    public Color referenceColor = new Color(0, 1, 0, 0.5f); // 绿色半透明
    public float referenceLineWidth = 0.05f;

    [Header("User Trail Settings")]
    public Color userTrailColor = new Color(1, 0, 0, 0.8f); // 红色
    public float userTrailWidth = 0.04f;
    public float userTrailTime = 10f; // 轨迹保留时间（秒）
    public Material trailMaterial; // 轨迹材质

    [Header("Recording")]
    public bool isRecording = false;
    public bool autoStartRecording = false;
    public float recordingStartDelay = 2f; // 自动开始录制的延迟

    [Header("Comparison")]
    public bool showComparison = true;
    public float errorThreshold = 0.5f; // 误差阈值（Unity单位）

    // 运行时数据
    private List<Vector3> referencePoints = new List<Vector3>();
    private List<Vector3> recordedPoints = new List<Vector3>();
    private float recordingStartTime;
    private bool hasStartedRecording = false;
    private TrajectoryType _lastTrajectoryType;
    private float _lastScale;
    private int _lastResolution;

    // 统计数据
    public float AverageError { get; private set; }
    public float MaxError { get; private set; }
    public float CompletionPercentage { get; private set; }

    public enum TrajectoryType
    {
        Circle,      // 圆形
        StraightLine,// 直线
        Spiral,      // 螺旋
        Wave,        // 波浪
        Figure8,     // 8字形
        Square,      // 方形
        CustomPath   // 自定义路径
    }

    void OnValidate()
    {
        if (referenceTrajectory != null)
        {
            SetupReferenceTrajectory();
        }
        CacheTrajectoryConfig();
    }

    void Start()
    {
        TryBindBall();
        SetupReferenceTrajectory();
        CacheTrajectoryConfig();

        if (autoStartRecording)
        {
            Invoke(nameof(StartRecording), recordingStartDelay);
        }
    }

    void Update()
    {
        if (HasTrajectoryConfigChanged())
        {
            SetupReferenceTrajectory();
            CacheTrajectoryConfig();
        }

        if (ball == null)
        {
            TryBindBall();
        }

        if (isRecording && ball != null)
        {
            RecordUserPosition();
            
            if (showComparison)
            {
                CalculateError();
            }
        }
    }

    /// <summary>
    /// 设置参考轨迹
    /// </summary>
    void SetupReferenceTrajectory()
    {
        if (referenceTrajectory == null)
        {
            GameObject refObj = new GameObject("ReferenceTrajectory");
            refObj.transform.SetParent(transform);
            referenceTrajectory = refObj.AddComponent<LineRenderer>();
        }

        referencePoints = GenerateTrajectoryPoints(trajectoryType);
        
        referenceTrajectory.positionCount = referencePoints.Count;
        referenceTrajectory.SetPositions(referencePoints.ToArray());
        referenceTrajectory.startWidth = referenceLineWidth;
        referenceTrajectory.endWidth = referenceLineWidth;
        referenceTrajectory.startColor = referenceColor;
        referenceTrajectory.endColor = referenceColor;
        bool shouldLoop = trajectoryType != TrajectoryType.StraightLine;
        referenceTrajectory.loop = shouldLoop;
        
        // 设置材质
        if (trailMaterial != null)
        {
            referenceTrajectory.material = trailMaterial;
        }
        else
        {
            // 使用默认材质
            referenceTrajectory.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    /// <summary>
    /// 设置用户轨迹追踪
    /// </summary>
    void SetupUserTrail()
    {
        if (ball == null) return;

        userTrail = ball.GetComponent<TrailRenderer>();
        if (userTrail == null)
        {
            userTrail = ball.gameObject.AddComponent<TrailRenderer>();
        }

        userTrail.time = userTrailTime;
        userTrail.startWidth = userTrailWidth;
        userTrail.endWidth = userTrailWidth * 0.5f;
        userTrail.startColor = userTrailColor;
        userTrail.endColor = new Color(userTrailColor.r, userTrailColor.g, userTrailColor.b, 0);
        
        if (trailMaterial != null)
        {
            userTrail.material = trailMaterial;
        }
        else
        {
            userTrail.material = new Material(Shader.Find("Sprites/Default"));
        }

        userTrail.enabled = false; // 初始禁用，开始录制时启用
    }

    private bool TryBindBall()
    {
        if (ball == null)
        {
            var controller = FindObjectOfType<HapkitSceneController>();
            if (controller != null && controller.ball != null)
            {
                ball = controller.ball;
            }
        }

        if (ball == null)
        {
            var found = GameObject.Find("HapkitBall");
            if (found != null)
            {
                ball = found.transform;
            }
        }

        if (ball != null)
        {
            SetupUserTrail();
            return true;
        }

        return false;
    }

    private void CacheTrajectoryConfig()
    {
        _lastTrajectoryType = trajectoryType;
        _lastScale = trajectoryScale;
        _lastResolution = trajectoryResolution;
    }

    private bool HasTrajectoryConfigChanged()
    {
        return _lastTrajectoryType != trajectoryType
            || !Mathf.Approximately(_lastScale, trajectoryScale)
            || _lastResolution != trajectoryResolution;
    }

    /// <summary>
    /// 生成不同类型的参考轨迹
    /// </summary>
    List<Vector3> GenerateTrajectoryPoints(TrajectoryType type)
    {
        List<Vector3> points = new List<Vector3>();
        
        switch (type)
        {
            case TrajectoryType.Circle:
                for (int i = 0; i <= trajectoryResolution; i++)
                {
                    float t = (float)i / trajectoryResolution * 2 * Mathf.PI;
                    points.Add(new Vector3(
                        Mathf.Cos(t) * trajectoryScale,
                        Mathf.Sin(t) * trajectoryScale,
                        0
                    ));
                }
                break;

            case TrajectoryType.StraightLine:
                int linePoints = Mathf.Max(2, trajectoryResolution);
                for (int i = 0; i < linePoints; i++)
                {
                    float t = (float)i / (linePoints - 1);
                    points.Add(new Vector3(
                        Mathf.Lerp(-trajectoryScale, trajectoryScale, t),
                        0,
                        0
                    ));
                }
                break;

            case TrajectoryType.Spiral:
                for (int i = 0; i <= trajectoryResolution; i++)
                {
                    float t = (float)i / trajectoryResolution * 4 * Mathf.PI;
                    float r = trajectoryScale * (1 - (float)i / trajectoryResolution);
                    points.Add(new Vector3(
                        Mathf.Cos(t) * r,
                        Mathf.Sin(t) * r,
                        (float)i / trajectoryResolution * trajectoryScale
                    ));
                }
                break;

            case TrajectoryType.Wave:
                for (int i = 0; i <= trajectoryResolution; i++)
                {
                    float t = (float)i / trajectoryResolution * 4 * Mathf.PI;
                    points.Add(new Vector3(
                        (float)i / trajectoryResolution * 2 * trajectoryScale - trajectoryScale,
                        Mathf.Sin(t) * trajectoryScale * 0.5f,
                        Mathf.Cos(t * 0.5f) * trajectoryScale * 0.3f
                    ));
                }
                break;

            case TrajectoryType.Figure8:
                for (int i = 0; i <= trajectoryResolution; i++)
                {
                    float t = (float)i / trajectoryResolution * 2 * Mathf.PI;
                    points.Add(new Vector3(
                        Mathf.Sin(t) * trajectoryScale,
                        Mathf.Sin(2 * t) * trajectoryScale * 0.5f,
                        0
                    ));
                }
                break;

            case TrajectoryType.Square:
                int pointsPerSide = trajectoryResolution / 4;
                for (int i = 0; i < pointsPerSide; i++)
                {
                    float t = (float)i / pointsPerSide;
                    points.Add(new Vector3(-trajectoryScale + t * 2 * trajectoryScale, trajectoryScale, 0));
                }
                for (int i = 0; i < pointsPerSide; i++)
                {
                    float t = (float)i / pointsPerSide;
                    points.Add(new Vector3(trajectoryScale, trajectoryScale - t * 2 * trajectoryScale, 0));
                }
                for (int i = 0; i < pointsPerSide; i++)
                {
                    float t = (float)i / pointsPerSide;
                    points.Add(new Vector3(trajectoryScale - t * 2 * trajectoryScale, -trajectoryScale, 0));
                }
                for (int i = 0; i < pointsPerSide; i++)
                {
                    float t = (float)i / pointsPerSide;
                    points.Add(new Vector3(-trajectoryScale, -trajectoryScale + t * 2 * trajectoryScale, 0));
                }
                break;
        }

        return points;
    }

    /// <summary>
    /// 开始录制用户轨迹
    /// </summary>
    public void StartRecording()
    {
        isRecording = true;
        hasStartedRecording = true;
        recordedPoints.Clear();
        recordingStartTime = Time.time;
        
        if (userTrail != null)
        {
            userTrail.Clear();
            userTrail.enabled = true;
        }

        Debug.Log("开始录制轨迹");
    }

    /// <summary>
    /// 停止录制
    /// </summary>
    public void StopRecording()
    {
        isRecording = false;
        Debug.Log($"停止录制，共记录 {recordedPoints.Count} 个点");
        CalculateFinalStats();
    }

    /// <summary>
    /// 重置轨迹
    /// </summary>
    public void ResetTrajectory()
    {
        isRecording = false;
        hasStartedRecording = false;
        recordedPoints.Clear();
        
        if (userTrail != null)
        {
            userTrail.Clear();
            userTrail.enabled = false;
        }

        AverageError = 0;
        MaxError = 0;
        CompletionPercentage = 0;

        Debug.Log("轨迹已重置");
    }

    /// <summary>
    /// 记录用户当前位置
    /// </summary>
    void RecordUserPosition()
    {
        if (ball == null) return;
        recordedPoints.Add(ball.position);
    }

    /// <summary>
    /// 实时计算误差
    /// </summary>
    void CalculateError()
    {
        if (recordedPoints.Count == 0 || referencePoints.Count == 0) return;

        Vector3 currentPos = ball.position;
        float minDist = float.MaxValue;

        // 找到参考轨迹上最近的点
        foreach (var refPoint in referencePoints)
        {
            float dist = Vector3.Distance(currentPos, refPoint);
            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        // 更新最大误差
        if (minDist > MaxError)
        {
            MaxError = minDist;
        }
    }

    /// <summary>
    /// 计算最终统计数据
    /// </summary>
    void CalculateFinalStats()
    {
        if (recordedPoints.Count == 0 || referencePoints.Count == 0) return;

        float totalError = 0;
        int validPoints = 0;

        foreach (var recordedPoint in recordedPoints)
        {
            float minDist = float.MaxValue;
            foreach (var refPoint in referencePoints)
            {
                float dist = Vector3.Distance(recordedPoint, refPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
            
            totalError += minDist;
            if (minDist < errorThreshold)
            {
                validPoints++;
            }
        }

        AverageError = totalError / recordedPoints.Count;
        CompletionPercentage = (float)validPoints / recordedPoints.Count * 100f;

        Debug.Log($"平均误差: {AverageError:F3}");
        Debug.Log($"最大误差: {MaxError:F3}");
        Debug.Log($"完成度: {CompletionPercentage:F1}%");
    }

    /// <summary>
    /// 切换轨迹类型
    /// </summary>
    public void SetTrajectoryType(TrajectoryType type)
    {
        trajectoryType = type;
        SetupReferenceTrajectory();
        ResetTrajectory();
    }

    /// <summary>
    /// 导出录制数据（用于进一步分析）
    /// </summary>
    public void ExportData()
    {
        string data = "Time,X,Y,Z,Error\n";
        for (int i = 0; i < recordedPoints.Count; i++)
        {
            Vector3 point = recordedPoints[i];
            float error = GetErrorAtPoint(point);
            float time = (float)i / recordedPoints.Count * (Time.time - recordingStartTime);
            data += $"{time:F3},{point.x:F3},{point.y:F3},{point.z:F3},{error:F3}\n";
        }

        string filename = $"TrajectoryData_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        System.IO.File.WriteAllText(filename, data);
        Debug.Log($"数据已导出到: {filename}");
    }

    float GetErrorAtPoint(Vector3 point)
    {
        float minDist = float.MaxValue;
        foreach (var refPoint in referencePoints)
        {
            float dist = Vector3.Distance(point, refPoint);
            if (dist < minDist)
            {
                minDist = dist;
            }
        }
        return minDist;
    }
}
