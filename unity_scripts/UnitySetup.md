# Unity 场景与运行说明

## 适用环境
- Windows
- Unity 2022.3.62f3c1
- 串口波特率 115200

## 文件位置
脚本已放在：
- [unity_scripts/SerialHapkitReader.cs](unity_scripts/SerialHapkitReader.cs)
- [unity_scripts/HapkitSceneController.cs](unity_scripts/HapkitSceneController.cs)
- [unity_scripts/SerialPortUI.cs](unity_scripts/SerialPortUI.cs)
- [unity_scripts/HapkitSampleSceneBootstrap.cs](unity_scripts/HapkitSampleSceneBootstrap.cs)
- [unity_scripts/HapkitWallVisualizer.cs](unity_scripts/HapkitWallVisualizer.cs)
- [unity_scripts/haptic_extension/HapticFeedbackSender.cs](unity_scripts/haptic_extension/HapticFeedbackSender.cs)
- [unity_scripts/haptic_extension/HapticPresetTrigger.cs](unity_scripts/haptic_extension/HapticPresetTrigger.cs)
- [unity_scripts/haptic_extension/HapticForceDisplay.cs](unity_scripts/haptic_extension/HapticForceDisplay.cs)
- [unity_scripts/haptic_extension/HapticCalibrationSender.cs](unity_scripts/haptic_extension/HapticCalibrationSender.cs)

## 快速搭建场景（推荐）
1. 在 Unity 新建或打开项目
2. 将 [unity_scripts](unity_scripts) 文件夹整体拷贝到项目的 Assets/Scripts 下
3. 新建一个空场景（或使用现有场景）
4. 在层级面板中创建空对象 `HapkitBootstrap`
5. 给该对象挂载 `HapkitSampleSceneBootstrap` 脚本
6. 在 Inspector 中设置：
   - `Initial Port` 建议先留空（避免启动时报错），运行时再修改
   - `Create UI` 勾选（需要 TextMeshPro）
   - `Create Wall Visualization` 勾选（可视化虚拟墙/孔洞）
7. 点击 Play

该脚本会自动创建：
- 串口读写对象（HapkitSerial）
- 球体对象（HapkitBall）
- 场景控制逻辑（HapkitScene）
- 运行时串口修改 UI（TextMeshPro）
- 虚拟墙/孔洞可视化（可选）

## 自定义串口号
运行时可在 UI 输入框里输入端口号（例如 COM5），点击 Apply Port 即可重连。

## 手动搭建（不使用 Bootstrap）
1. 创建空对象 `HapkitSerial`，挂载 `SerialHapkitReader`
2. 创建空对象 `HapkitScene`，挂载 `HapkitSceneController`
3. 创建一个 Sphere，拖到 `HapkitSceneController.ball`
4. 调整 `scale` 与墙体参数（与 Processing 保持一致）
5. 可选：创建空对象挂载 `HapkitWallVisualizer` 做墙体/孔洞可视化
6. 触觉反馈：给任意物体挂载 `HapticFeedbackSender`，把 `reader` 指向 `SerialHapkitReader`

## 触觉反馈（Unity -> Leader）
- `HapticFeedbackSender` 会通过串口发送参数：虚拟墙位置/厚度、孔洞半径、刚度、最大力、约束开关
- 使用前请先烧录更新后的 Leader 固件（见下文）

### 可调参数（HapticFeedbackSender）
- `useConstraints`：是否启用虚拟墙/孔洞约束
- `wallPos`：虚拟墙中心位置（Z 方向，单位：mm）
- `wallThick`：虚拟墙厚度（单位：mm）
- `holeRadius`：孔洞半径（单位：mm）
- `stiffness`：刚度系数（越大越硬）
- `damping`：阻尼系数（已参与力计算，基于速度估计）
- `maxForce`：最大力上限

### 标定（串口命令）
已支持串口命令 `C` 触发标定（将当前姿态作为零位）。
使用 [unity_scripts/haptic_extension/HapticCalibrationSender.cs](unity_scripts/haptic_extension/HapticCalibrationSender.cs)：
1. 挂到任意物体
2. 设定 `reader`
3. 运行时通过 Inspector 的 `Calibrate Now` 触发

### 示例脚本（触发区切换触感）
使用 [unity_scripts/haptic_extension/HapticPresetTrigger.cs](unity_scripts/haptic_extension/HapticPresetTrigger.cs)：
1. 在场景放一个带 Collider 的物体，勾选 `Is Trigger`
2. 挂载 `HapticPresetTrigger`
3. 把 `sender` 指向 `HapticFeedbackSender`
4. 调整参数并运行，进入触发区时自动切换触感

### 力显示（Unity UI）
1. 创建一个 TMP_Text
2. 挂载 `HapticForceDisplay`
3. 把 `reader` 指向 `SerialHapkitReader`，把 `label` 指向 TMP_Text
4. 运行后即可看到实时力值（Leader 回传）

## Leader 固件更新
需要让 Leader 接收 Unity 的参数包：
- 位置： [3DHapkit_Learder/3DHapkit_Learder.ino](3DHapkit_Learder/3DHapkit_Learder.ino)
- 更新后重新烧录到 Leader 板

## 串口回传数据格式
默认使用带包头的数据流（Unity 已支持解析）：
- 位置包：Header `0xFE 0xEF` + Length `14` + payload(14 bytes)
- 力包：Header `0xCC 0x33` + Length `4` + payload(float)

默认兼容旧 Processing 数据流：Leader `UNITY_PACKET = 0`，Unity `acceptRawFrames = true`。

## TextMeshPro 依赖
首次使用 TMPro 组件时，Unity 会提示导入 TMP Essential Resources，请在弹窗中点击 Import。

## System.IO.Ports 依赖（串口）
如果出现 `System.IO.Ports` 找不到的编译错误：
1. 打开 Project Settings → Player → Other Settings → Api Compatibility Level
2. 将其设置为 **.NET Framework**
3. 重新导入脚本并等待编译完成

## 位置与逻辑说明
- 串口数据格式：14 字节（x、y、z 的 int32 + 2 个符号位）
- 坐标单位：x/y/z 解析后需除以 10
- 虚拟墙与孔洞逻辑参考原 Processing 实现，已在脚本中复现

## 运动范围/方向调整
如果运动范围太小或路径方向不对：
1. 在 `HapkitSceneController` 中调大 `scale`（例如 0.2 ~ 0.5）
2. 调整 `axisScale` 做非等比缩放
3. 调整 `Axis Mapping`（默认 Unity 使用 Y 为上，推荐 X->X，Y->Z，Z->Y）
4. 需要排除虚拟墙/孔洞限制时，勾掉 `Use Constraints`

## 常见问题
- **打不开串口**：确认端口号、占用情况、设备驱动
- **拒绝访问/Access Denied**：
   1. 停止播放并等待 2-3 秒，让 Unity 释放串口
   2. 关闭 Arduino 串口监视器、串口助手等可能占用 COM 的程序
   3. 设备拔插一次，确认 COM 号后再连接
- **球体不动**：检查 Leader 是否正在输出串口数据
- **位置比例不对**：调整 `scale`
- **提示找不到脚本类**：
   1. 确认脚本已放在 Unity 项目的 Assets 目录下（例如 Assets/Scripts）
   2. 打开 Console，先解决任何编译错误
   3. 确认文件名与类名一致：HapkitSampleSceneBootstrap.cs / HapkitSampleSceneBootstrap
   4. 若 Console 报 TMP 相关错误，请先导入 TMP Essential Resources
- **TMP InputField 拖拽时报 NullReference**：确认场景 UI 使用本项目脚本生成的 TMP 输入框（包含 Text Area 子物体）。

需要可视化墙体/孔洞或改成 TMPro UI，请告诉我具体需求。
