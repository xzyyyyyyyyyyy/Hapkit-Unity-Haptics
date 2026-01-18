# Hapkit-Unity-Haptics

一个将 3 轴 Hapkit 设备接入 Unity 的最小可用开源包，包含：
- Unity 侧串口通信与可视化脚本
- 触觉参数下发与回传力值显示
- 领导板/跟随板固件（Leader/Follower）
- 串口标定流程（无按钮，串口命令触发）

> 目标：在 Unity 中快速复现 Hapkit 的交互与触觉反馈，并保持对旧 Processing 数据流的兼容。

---

## 目录结构
```
Hapkit-Unity-Haptics/
  firmware/
    Leader/3DHapkit_Learder.ino
    Follower/3DHapkit_Follower.ino
  unity_scripts/
    SerialHapkitReader.cs
    HapkitSceneController.cs
    HapkitSampleSceneBootstrap.cs
    HapkitWallVisualizer.cs
    SerialPortUI.cs
    unity_scripts.asmdef
    UnitySetup.md
    haptic_extension/
      HapticFeedbackSender.cs
      HapticPresetTrigger.cs
      HapticForceDisplay.cs
      HapticCalibrationSender.cs
```

## 快速开始（Unity）
1. 将 `unity_scripts` 拷贝到 Unity 工程的 `Assets/Scripts`。
2. 新建空物体并挂载 `HapkitSampleSceneBootstrap`。
3. 运行后在 UI 输入串口号并点击 Apply。
4. （可选）挂载 `HapticFeedbackSender` 下发触觉参数。

更详细步骤见：[unity_scripts/UnitySetup.md](unity_scripts/UnitySetup.md)

## 运行环境
- Windows
- Unity 2022.3.62f3c1
- .NET Framework 兼容性（用于 System.IO.Ports）
- TextMeshPro（首次使用需导入 TMP Essential Resources）

## 硬件接线（I2C）
1. 三块板的 GND 互联
2. 三块板的 A4/A5 互联（I2C）
3. 设定地址：
  - Follower 1：Wire.begin(8)
  - Follower 2：Wire.begin(12)

## 标定流程（串口命令）
1. 将手柄置于“机械中位”
2. 串口发送字符 `C`
3. Leader 将当前姿态作为零位

Unity 中可用 `HapticCalibrationSender` 触发。

## 固件烧录
- Leader：烧录 `firmware/Leader/3DHapkit_Learder.ino`
- Follower：烧录 `firmware/Follower/3DHapkit_Follower.ino`

## 标定（串口命令）
- 发送字符 `C` 触发零位标定。
- Unity 中可用 `HapticCalibrationSender` 触发。

## 兼容 Processing
默认兼容旧 Processing 数据流：
- Leader：`UNITY_PACKET = 0`
- Unity：`acceptRawFrames = true`

## 参数建议范围
为避免过载，建议：
- stiffness：50 ~ 200
- damping：0 ~ 5
- maxForce：0.5 ~ 5.0

## 版本信息
- Unity：2022.3.62f3c1
- 固件：Hapkit-Unity-Haptics/firmware 版本

## License
本项目使用 MIT License，见 [LICENSE](LICENSE)