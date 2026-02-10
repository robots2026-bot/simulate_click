# 板端远程触摸注入（uinput + event0 劫持）

## 目的
在无法现场触屏操作时，通过 SSH 在板子上“模拟一次触摸点击”，让 Qt/QWS 应用（NVR）接收到点击事件。

## 关键结论（为什么不能直接写 event0）
- `/dev/input/event0` 是 evdev 导出的“内核 -> 用户态读取口”，设计用途是给应用 `read()` 硬件事件。
- “注入输入事件”的标准方式是 `/dev/uinput`：用户态创建虚拟输入设备，内核把它当成真实设备分发事件。
- 本项目的 NVR 进程实际只打开了一个触摸设备（通常硬编码为 `/dev/input/event0`）。如果你只创建虚拟 `event1`，NVR 不一定会去读。

因此方案是：
1) 用 `uinput` 创建虚拟触摸设备（会出现 `/dev/input/eventX`，例如 `event1`）。  
2) 对“硬编码只读 event0”的应用，启用 event0 劫持：把 `/dev/input/event0` 临时指向虚拟设备。

## 组件

### 1) `uinput_touchd`（守护进程）
代码：`board_tools/uinput_touch/uinput_touchd.c`

功能：
- 通过 `/dev/uinput` 创建虚拟多点触摸设备（`ABS_MT_*` + `BTN_TOUCH`）。
- 监听 FIFO（默认 `/tmp/uinput_touch.cmd`），读取命令并注入触摸。
- 写出虚拟设备节点到 `/tmp/uinput_touch.event`（例如 `/dev/input/event1`）。
- 可选劫持：
  - `--hijack-event0`：把硬件 `/dev/input/event0` 移到 `/dev/input/event0.hw`，并创建 symlink：`/dev/input/event0 -> /dev/input/eventX`
  - `--restore-event0`：从 `/dev/input/event0.hw` 恢复硬件 event0

### 2) `uinput_touchctl`（命令行客户端）
代码：`board_tools/uinput_touch/uinput_touchctl.c`

功能：
- 向 FIFO 写入 `tap x y`，触发一次点击。
- 典型用法：`/home/app/uinput_touchctl 640 400`

## 使用流程（推荐）

### 1) 启动守护进程并劫持 event0
在板子上执行：
```sh
pkill -f uinput_touchd || true
rm -f /tmp/uinput_touch.cmd /tmp/uinput_touch.event /tmp/uinput_touchd.log
(/home/app/uinput_touchd --hijack-event0 </dev/null >/tmp/uinput_touchd.log 2>&1 &)
sleep 0.3
```

验证劫持是否生效：
```sh
ls -l /dev/input/event0 /dev/input/event0.hw 2>/dev/null || true
cat /tmp/uinput_touch.event 2>/dev/null || true
tail -n 20 /tmp/uinput_touchd.log 2>/dev/null || true
```

### 2) 重启 NVR（让它重新打开 event0）
如果 NVR 是长期运行的，需要重启一次让它重新 `open("/dev/input/event0")`，此时会实际读到虚拟设备。

示例（按你们当前环境，注意加载 profile 和库路径）：
```sh
. /etc/profile
export LD_LIBRARY_PATH=/home/app/lib:$LD_LIBRARY_PATH
killall -9 NVR 2>/dev/null || true
sleep 0.4
cd /home/app && ./NVR -qws >/tmp/NVR.hijack.log 2>&1 &
```

验证 NVR 读到的是虚拟设备：
```sh
pidof NVR
ls -l /proc/$(pidof NVR)/fd | grep "/dev/input/event" || true
```
期望看到类似：`/dev/input/event1`（而不是硬件的 event0）。

### 3) 注入点击
```sh
/home/app/uinput_touchctl 640 400
```

## 恢复硬件触摸（停止远控后必须做）
```sh
pkill -f uinput_touchd || true
/home/app/uinput_touchd --restore-event0
```

恢复后验证：
```sh
ls -l /dev/input/event0 /dev/input/event0.hw 2>/dev/null || true
```

## 坐标与错位说明
- `uinput_touchd` 默认把坐标范围当成像素坐标：`x=0..1279`、`y=0..799`。
- 如果你的 GUI 点击点位有偏移，优先排查：
  - 你发送的坐标是否经过“截图缩放/居中”的映射计算。
  - 触摸驱动/tslib 是否用非像素范围（raw 0..4095 等）。这种情况下需要补一个 `absinfo` 工具用 `EVIOCGABS` 读 min/max，再做映射。

## 风险与注意事项
- `--hijack-event0` 会让“只读 event0 的应用”只看到虚拟触摸输入；硬件触摸被挪到 `/dev/input/event0.hw`，但不会自动被应用使用。
- `/dev` 在该系统上通常是可写 tmpfs（已验证），所以 symlink/rename 可行；但仍建议在任务结束后执行 `--restore-event0` 还原。

