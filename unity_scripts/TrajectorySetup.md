# 轨迹跟踪系统设置指南

## 功能概述
这个系统允许用户通过Hapkit设备操作3D空间中的小球，跟踪预设的参考轨迹，并实时对比分析操作精度。

## 核心特性
- ✅ 多种预设参考轨迹（圆形、螺旋、波浪、8字形、方形）
- ✅ 实时轨迹可视化（TrailRenderer拖尾效果）
- ✅ 误差分析和完成度统计
- ✅ 数据导出功能（CSV格式）
- ✅ 可自定义轨迹参数

---

## 快速设置步骤

### 1. 添加脚本到场景

#### 方法A：在现有小球上添加
1. 找到你场景中的小球对象（由 `HapkitSceneController` 控制）
2. 在小球的父对象或场景根对象上创建空物体，命名为 `TrajectorySystem`
3. 添加 `TrajectoryFollowingSystem.cs` 组件到此物体

#### 方法B：从头创建
```
Hierarchy:
  - HapkitScene
    - TrajectorySystem (空物体)
      - [TrajectoryFollowingSystem组件]
    - Ball (你的小球对象)
```

### 2. 配置组件

在 `TrajectoryFollowingSystem` Inspector 中设置：

#### References
- **Ball**: 拖入你的小球Transform
- **Reference Trajectory**: 留空（会自动创建）
- **User Trail**: 留空（会自动创建）

#### Reference Trajectory Settings
- **Trajectory Type**: 选择轨迹类型（建议从 Circle 开始）
- **Trajectory Scale**: `1.0`（根据你的场景大小调整）
- **Trajectory Resolution**: `100`
- **Reference Color**: 绿色半透明 `(0, 1, 0, 0.5)`
- **Reference Line Width**: `0.05`

#### User Trail Settings
- **User Trail Color**: 红色 `(1, 0, 0, 0.8)`
- **User Trail Width**: `0.04`
- **User Trail Time**: `10`秒（轨迹保留时间）
- **Trail Material**: 可选，使用默认材质即可

#### Recording
- **Is Recording**: 取消勾选（通过UI或代码控制）
- **Auto Start Recording**: 勾选此项会在场景启动后自动开始录制
- **Recording Start Delay**: `2`秒（给用户准备时间）

#### Comparison
- **Show Comparison**: 勾选
- **Error Threshold**: `0.5`（Unity单位，根据场景调整）

---

### 3. 创建UI（可选但推荐）

#### 手动创建UI
在Canvas下创建以下UI元素：

```
Canvas
  - TrajectoryPanel
    - TitleText: "轨迹跟踪系统"
    - StartButton: "开始录制"
    - StopButton: "停止录制"
    - ResetButton: "重置"
    - ExportButton: "导出数据"
    - TrajectoryTypeDropdown: 轨迹类型选择
    - TogglePanel
      - ShowReferenceToggle: "显示参考轨迹"
      - ShowUserTrailToggle: "显示用户轨迹"
    - TrailTimeSlider: 轨迹时长调节
    - StatsPanel
      - RecordingStatusText
      - AverageErrorText
      - MaxErrorText
      - CompletionText
```

#### 配置 TrajectoryUIController
1. 在Canvas或面板上添加 `TrajectoryUIController.cs`
2. 将 `TrajectoryFollowingSystem` 拖入 **Trajectory System** 字段
3. 将对应的UI元素拖入各个字段

---

### 4. 材质设置（重要！）

轨迹需要合适的材质才能正确显示：

#### 创建轨迹材质
1. 在项目中 **右键 → Create → Material**
2. 命名为 `TrailMaterial`
3. 设置 Shader 为 `Sprites/Default` 或 `Unlit/Color`
4. 将此材质拖入 `TrajectoryFollowingSystem` 的 **Trail Material** 字段

---

## 使用流程

### 基本操作
1. **启动场景** → 参考轨迹自动显示（绿色线条）
2. **连接Hapkit** → 通过 `SerialPortUI` 连接设备
3. **点击"开始录制"** → 用户轨迹开始记录（红色拖尾）
4. **操作Hapkit** → 小球移动，轨迹实时记录
5. **尝试跟随参考轨迹** → 尽可能接近绿色线条
6. **点击"停止录制"** → 查看统计结果
7. **导出数据（可选）** → CSV文件包含所有轨迹点和误差

### 对比实验示例
```
实验设计：
1. 第一组：用户先看到参考轨迹，然后操作
2. 第二组：用户先操作，后显示参考轨迹对比
3. 对比两组的 平均误差、最大误差、完成度

数据收集：
- 使用 ExportData() 导出每次实验的CSV
- CSV包含：时间戳、XYZ坐标、每点误差
```

---

## 进阶设置

### 自定义轨迹

在 `TrajectoryFollowingSystem.cs` 中添加自定义轨迹：

```csharp
case TrajectoryType.CustomPath:
    // 你的自定义轨迹逻辑
    for (int i = 0; i <= trajectoryResolution; i++)
    {
        float t = (float)i / trajectoryResolution;
        // 例如：心形曲线
        float angle = t * 2 * Mathf.PI;
        float x = 16 * Mathf.Pow(Mathf.Sin(angle), 3) * trajectoryScale;
        float y = (13 * Mathf.Cos(angle) - 5 * Mathf.Cos(2*angle) 
                   - 2 * Mathf.Cos(3*angle) - Mathf.Cos(4*angle)) * trajectoryScale;
        points.Add(new Vector3(x * 0.1f, y * 0.1f, 0));
    }
    break;
```

### 通过代码控制

```csharp
// 获取组件
TrajectoryFollowingSystem trajectorySystem = GetComponent<TrajectoryFollowingSystem>();

// 切换轨迹类型
trajectorySystem.SetTrajectoryType(TrajectoryFollowingSystem.TrajectoryType.Spiral);

// 开始/停止录制
trajectorySystem.StartRecording();
trajectorySystem.StopRecording();

// 重置
trajectorySystem.ResetTrajectory();

// 导出数据
trajectorySystem.ExportData();

// 获取统计数据
float avgError = trajectorySystem.AverageError;
float maxError = trajectorySystem.MaxError;
float completion = trajectorySystem.CompletionPercentage;
```

---

## 调试提示

### 如果轨迹不显示
1. 检查材质是否正确设置
2. 检查 `referenceTrajectory.enabled` 是否为 true
3. 检查轨迹缩放 `trajectoryScale` 是否合适
4. 在Scene视图中查看轨迹位置

### 如果用户拖尾不显示
1. 确保录制已开始 `isRecording = true`
2. 检查 `userTrail.enabled` 是否为 true
3. 检查小球是否在移动
4. 确认 TrailRenderer 的材质设置正确

### 性能优化
- 降低 `trajectoryResolution` (如 50) 以减少顶点数
- 减少 `userTrailTime` 以降低内存占用
- 录制完成后及时停止以节省资源

---

## 数据分析

导出的CSV文件格式：
```csv
Time,X,Y,Z,Error
0.000,0.500,0.200,0.000,0.123
0.020,0.510,0.215,0.005,0.098
...
```

可用于：
- Excel/MATLAB/Python 进一步分析
- 绘制误差曲线图
- 计算速度、加速度等衍生指标
- 统计分析和对比实验

---

## 常见问题

**Q: 参考轨迹太大/太小？**  
A: 调整 `trajectoryScale` 参数，建议值 0.5 - 2.0

**Q: 用户轨迹消失太快？**  
A: 增加 `userTrailTime` 值，或通过UI滑块实时调整

**Q: 想要更平滑的轨迹？**  
A: 增加 `trajectoryResolution` 和 `userTrail` 的精度

**Q: 如何添加触觉反馈？**  
A: 结合 `HapticFeedbackSender`，在偏离轨迹时发送力反馈信号

**Q: 能实时显示误差吗？**  
A: 可以，在 `Update()` 中访问 `AverageError` 和 `MaxError` 更新UI

---

## 扩展建议

### 添加触觉引导
在偏离轨迹时，通过 Hapkit 产生力反馈，引导用户回到正确路径：

```csharp
// 在 TrajectoryFollowingSystem 中
if (minDist > errorThreshold)
{
    // 计算指向最近参考点的方向
    Vector3 direction = closestRefPoint - ball.position;
    // 发送力反馈（需结合 HapticFeedbackSender）
    SendHapticFeedback(direction);
}
```

### 多人对比模式
记录多个用户的轨迹，同时显示在场景中：
- 不同颜色的 TrailRenderer
- 对比统计数据
- 排行榜系统

### 难度等级
- 简单：大轨迹、慢速、高容错
- 中等：中等轨迹、正常速度
- 困难：小轨迹、快速、低容错、带干扰力

---

## 技术支持

如有问题，请检查：
1. Unity Console 日志
2. `TrajectoryFollowingSystem` Inspector 实时数据
3. 确保 HapkitSceneController 正常工作

祝实验顺利！🎯
