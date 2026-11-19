# Implementation Steps

1. **Scaffold** — create `src/CursorAlarmOnRed` (WinForms, `net48`, `WinExe`, `AssemblyName=CursorAlarmOnRed`) and `tests/CursorAlarmOnRed.Tests` (xUnit, `net8.0`).
2. **RedDetector** — static `IsRed(r, g, b, threshold)` implementing PRD §4, plus `ClampThreshold`.
3. **AppConfig** — `Parse(string text)` → threshold (default 200 on any problem), `Format(int)`, and `Load(path)` / `Save(path, threshold)` with all IO errors swallowed safely.
4. **Unit tests** (written before the UI):
   - RedDetector: pure red, boundary at threshold, orange/pink/white/grey/black rejected, dark red passes with low threshold only.
   - AppConfig: parse valid, whitespace, empty, garbage, negative, > 255, missing file, save→load round-trip, unwritable path doesn't throw.
5. **NativeMethods** — P/Invoke: `GetCursorPos`, `GetDC`, `ReleaseDC`, `GetPixel`, `Beep`.
6. **MainForm** — fixed-size form built in code: Start/Stop buttons, RGB label + swatch, threshold textbox, status label.
   - Timer (50 ms) active only while started: read cursor → read pixel → update display → if red and no beep in flight, beep on a worker thread.
   - Threshold textbox: on every edit, validate; valid → store and save to config; invalid → red-tinted textbox, last valid value kept.
   - Stop clears the display and silences alarm.
7. **Program / app.manifest** — per-monitor DPI awareness embedded in the exe; load config on startup.
8. **Single-file check** — `dotnet build -c Release` output folder must contain only `CursorAlarmOnRed.exe` (+ `.pdb`, `.config` created at runtime) and no other DLLs.
9. **CI** — GitHub Actions on `windows-latest`: test, build, upload the exe as an artifact.
10. **Manual test on Windows** — walk through the PRD §9 acceptance criteria.

## Build & run
```
dotnet test  tests/CursorAlarmOnRed.Tests
dotnet build src/CursorAlarmOnRed -c Release
# -> src/CursorAlarmOnRed/bin/Release/net48/CursorAlarmOnRed.exe
```
