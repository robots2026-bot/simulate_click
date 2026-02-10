# WinForms 远程屏幕 + 触摸方案（不使用 WSL）

## 目标
提供一个 Windows WinForms 工具，实现：
1) 抓取远端帧缓冲图像。
2) 显示图像。
3) 点击图像后在远端模拟触摸。

全流程不依赖 WSL，全部在 Windows 完成。

## 约定（已确认）
- .NET 版本：**\.NET 10**（WinForms 目标框架 `net10.0-windows`）
- SSH 组件：**NuGet `SSH.NET`（Renci.SshNet），版本 2025.1.0**
- 刷新模式：**自动刷新，刷新频率可选**
- 远端命令与分辨率：**由配置提供**（抓屏命令、触摸命令、分辨率）

## 总体架构（Windows 端）
- 界面：
  - “刷新频率”选择（例如 0/0.5/1/2/5 秒）。
  - 图片显示区：展示截图。
  - 点击图片：计算坐标并发送触摸。
- 通信：
  - 使用 `SSH.NET` 进行 SSH + SCP。

## 数据流（Windows 侧视角）
1) WinForms 建立 SSH 连接。
2) 通过远端抓屏命令生成 raw 文件。
3) 通过 SCP 下载 raw 文件到 Windows。
4) WinForms 把 raw 转为 Bitmap 并显示。
5) 用户点击截图。
6) WinForms 计算设备坐标（按比例缩放）。
7) 通过远端触摸命令发送点击（默认 `/home/app/uinput_touchctl x y`）。

## 坐标映射
- 1:1 显示时，直接使用点击坐标。
- 如果缩放显示：
  - `x_device = x_click * device_width / image_width`
  - `y_device = y_click * device_height / image_height`
- `device_width/device_height` 来自配置。

## WinForms 实现要点
- 依赖：
  - NuGet 包：`SSH.NET`（Renci.SshNet）
- 实现流程：
  1) 创建 `SshClient` + `ScpClient`。
  2) `SshClient.RunCommand("<抓屏命令>")` 生成 raw。
  3) `ScpClient.Download("<raw_path>", ...)` 下载。
  4) 把 raw 转成 `Bitmap`（32bppArgb 或 32bppRgb）。
  5) 显示在 `PictureBox`。
  6) 点击后计算坐标并调用远端触摸命令（默认 `/home/app/uinput_touchctl`）。
  7) 定时器驱动自动刷新（可配置频率）。

## 工程创建与依赖安装（Windows）
1) 使用 VS 创建 **WinForms (\.NET 10)** 项目。
2) NuGet 安装 `SSH.NET`。
3) 建议新增 `Settings`（或 json 配置）保存：IP/用户/密码/端口/分辨率/刷新频率/命令模板。

## 图像转换细节
- 若颜色反转或色偏，需切换 BGRA/RGBA：
  - 先尝试 BGRA。
  - 若颜色不对，改为 RGBA。
- 32bpp 建议使用 `PixelFormat.Format32bppArgb`。

## 自动刷新策略
- 使用 `System.Windows.Forms.Timer` 驱动刷新（UI 线程）。
- 刷新过程要加锁，防止重入：
  - 当一次抓屏未完成时，忽略下一次定时触发。
- 刷新频率为 0 时表示手动刷新。

## 错误处理建议
- SSH/SCP 失败：提示并暂停自动刷新。
- 触摸命令执行失败：提示并保留连接。
- 提供“重新连接”按钮。

## 与板端联动说明
- 触摸注入基于板端 `uinput_touchd/uinput_touchctl` 方案（详见板端文档）。
- Windows 侧仅负责调用触摸命令；板端守护进程需提前启动并完成 event0 劫持。

## 安全注意
- 凭据建议后续迁移到配置文件或加密存储。
- 关闭 HostKey 校验仅适合内网开发环境。

## 下一步（Windows 侧）
1) 完成 WinForms 原型（自动刷新 + 点击）。
2) 验证像素顺序（BGRA/RGBA）。
3) 完成基本联调。
