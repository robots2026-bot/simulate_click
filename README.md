# simulate_click

Windows WinForms tool to view a remote framebuffer over SSH/SCP and inject touch events via a board-side helper.

## Features
- SSH connect + remote framebuffer capture
- SCP download of raw frame
- Display with click-to-touch mapping
- Optional RGBA/BGRA conversion
- Auto refresh with configurable interval

## Requirements
- .NET 10 (WinForms target `net10.0-windows`)
- NuGet package `SSH.NET`
- Board-side helper (see docs)

## Docs
- Windows plan: `docs/remote_touch_winform_plan.md`
- Board design: `docs/board_uinput_touch_hijack.md`
