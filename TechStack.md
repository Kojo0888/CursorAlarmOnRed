# Tech Stack

| Concern | Choice | Why |
|---------|--------|-----|
| Language | **C#** | First-class Windows desktop support, small code. |
| UI | **Windows Forms** on **.NET Framework 4.8** | .NET Framework 4.8 is pre-installed on Windows 10 (1903+) and Windows 11, so the exe needs **no runtime and no extra DLLs** — it is a single small file. WinForms needs no designer or extra packages. |
| Pixel read | **GDI32 `GetPixel`** on the screen DC (P/Invoke) | Reads exactly one pixel; no screenshot needed. Fast and simple. |
| Cursor position | **User32 `GetCursorPos`** (P/Invoke) | Same physical-pixel coordinate space as `GetPixel`. |
| DPI | `app.manifest` (embedded in the exe) declares per-monitor DPI awareness | Keeps cursor coordinates and pixels aligned on scaled displays. |
| Alarm sound | **Kernel32 `Beep`** (P/Invoke) | OS-generated tone, no audio file to ship. Called on a worker thread so the UI never blocks. |
| Polling | `System.Windows.Forms.Timer`, 50 ms | Simple; runs on the UI thread, no locking. |
| Config | Plain-text `Threshold=NNN` file via `System.IO` | No JSON/XML library → no extra DLL. Lives next to the exe. |
| Unit tests | **xUnit** on **.NET 8** (`dotnet test`) | Test project compiles the pure-logic files (`RedDetector.cs`, `AppConfig.cs`) directly, so tests run on any OS. |
| Build | `dotnet build -c Release` (.NET SDK 8) | Output: `CursorAlarmOnRed.exe`. GitHub Actions workflow builds and uploads it on a Windows runner. |

## Project layout
```
src/CursorAlarmOnRed/    app (WinForms) — Program, MainForm, NativeMethods, RedDetector, AppConfig, app.manifest
tests/CursorAlarmOnRed.Tests/   xUnit tests for RedDetector and AppConfig
.github/workflows/build.yml     Windows build + test, uploads the exe
```

## Design rule
All decision logic (is-it-red, parse/save config) lives in two UI-free classes so it can be unit-tested. `MainForm` only wires UI, timer and native calls.
