using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CursorAlarmOnRed
{
    internal sealed class MainForm : Form
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CursorAlarmOnRed.config");

        private readonly Button _start = new Button { Text = "Start", Left = 15, Top = 15, Width = 100, Height = 32 };
        private readonly Button _stop = new Button { Text = "Stop", Left = 125, Top = 15, Width = 100, Height = 32, Enabled = false };
        private readonly Label _rgb = new Label { Left = 15, Top = 62, Width = 190, Height = 24, Font = new Font("Consolas", 11f), Text = "R: -    G: -    B: -" };
        private readonly Panel _swatch = new Panel { Left = 205, Top = 60, Width = 20, Height = 24, BorderStyle = BorderStyle.FixedSingle };
        private readonly Label _thresholdLabel = new Label { Left = 15, Top = 103, Width = 150, Height = 20, Text = "Red threshold (0-255):" };
        private readonly TextBox _threshold = new TextBox { Left = 170, Top = 100, Width = 55 };
        private readonly Label _status = new Label { Left = 15, Top = 138, Width = 210, Height = 22, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Text = "Stopped" };
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer { Interval = 50 };

        private int _thresholdValue;
        private int _beeping; // 1 while a beep is playing on the worker thread

        public MainForm()
        {
            Text = "Cursor Alarm On Red";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(240, 172);
            Controls.AddRange(new Control[] { _start, _stop, _rgb, _swatch, _thresholdLabel, _threshold, _status });

            _thresholdValue = AppConfig.Load(ConfigPath);
            _threshold.Text = _thresholdValue.ToString();

            _start.Click += (s, e) => SetRunning(true);
            _stop.Click += (s, e) => SetRunning(false);
            _threshold.TextChanged += OnThresholdChanged;
            _timer.Tick += (s, e) => Poll();
        }

        private void OnThresholdChanged(object sender, EventArgs e)
        {
            int v;
            if (RedDetector.TryParseThreshold(_threshold.Text, out v))
            {
                _threshold.BackColor = SystemColors.Window;
                if (v != _thresholdValue)
                {
                    _thresholdValue = v;
                    AppConfig.Save(ConfigPath, v);
                }
            }
            else
            {
                _threshold.BackColor = Color.MistyRose; // invalid: last valid value stays in effect
            }
        }

        private void SetRunning(bool running)
        {
            _start.Enabled = !running;
            _stop.Enabled = running;
            _timer.Enabled = running;
            if (running)
            {
                _status.Text = "Monitoring";
                _status.ForeColor = SystemColors.ControlText;
            }
            else
            {
                _rgb.Text = "R: -    G: -    B: -";
                _swatch.BackColor = SystemColors.Control;
                _status.Text = "Stopped";
                _status.ForeColor = SystemColors.ControlText;
            }
        }

        private void Poll()
        {
            Color? pixel = NativeMethods.GetPixelUnderCursor();
            if (pixel == null) return;
            Color c = pixel.Value;
            _rgb.Text = string.Format("R: {0,-3}  G: {1,-3}  B: {2,-3}", c.R, c.G, c.B);
            _swatch.BackColor = c;

            if (RedDetector.IsRed(c.R, c.G, c.B, _thresholdValue))
            {
                _status.Text = "ALARM!";
                _status.ForeColor = Color.Red;
                if (Interlocked.Exchange(ref _beeping, 1) == 0)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try { NativeMethods.AlarmBeep(); }
                        finally { Interlocked.Exchange(ref _beeping, 0); }
                    });
                }
            }
            else
            {
                _status.Text = "Monitoring";
                _status.ForeColor = SystemColors.ControlText;
            }
        }
    }
}
