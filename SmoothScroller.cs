using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

[assembly: AssemblyTitle("ReadFlow Smooth Scroller")]
[assembly: AssemblyDescription("Smooth Auto-Scroller for Screen Sharing & Reading Sessions")]
[assembly: AssemblyCompany("Gary Sung")]
[assembly: AssemblyProduct("ReadFlow Scroller")]
[assembly: AssemblyCopyright("Copyright © 2026")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

namespace SmoothScroller
{
    // Custom modern slider tuned for precision slow reading speeds (1 to 30 px/s)
    public class ModernSlider : Control
    {
        private int val = 8;
        private int min = 1;
        private int max = 30;
        private bool isDragging = false;
        public event EventHandler ValueChanged;

        public int Value
        {
            get { return val; }
            set
            {
                int clamped = Math.Max(min, Math.Min(max, value));
                if (val != clamped)
                {
                    val = clamped;
                    Invalidate();
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public ModernSlider()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                          ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Height = 24;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                UpdateFromMouse(e.X);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isDragging) UpdateFromMouse(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isDragging = false;
        }

        private void UpdateFromMouse(int mouseX)
        {
            int trackMargin = 8;
            int usableWidth = this.Width - (trackMargin * 2);
            if (usableWidth <= 0) return;
            float fraction = (float)(mouseX - trackMargin) / usableWidth;
            fraction = Math.Max(0f, Math.Min(1f, fraction));
            this.Value = (int)Math.Round(min + fraction * (max - min));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int trackMargin = 8;
            int trackY = (this.Height / 2) - 3;
            int trackH = 6;
            int trackW = this.Width - (trackMargin * 2);

            // Background track (Zinc 700)
            using (GraphicsPath trackPath = GetRoundedRect(new Rectangle(trackMargin, trackY, trackW, trackH), 3))
            using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(63, 63, 70)))
            {
                g.FillPath(trackBrush, trackPath);
            }

            // Active track (Sky 500)
            float fraction = (float)(val - min) / (max - min);
            int activeW = (int)(trackW * fraction);
            if (activeW > 4)
            {
                using (GraphicsPath activePath = GetRoundedRect(new Rectangle(trackMargin, trackY, activeW, trackH), 3))
                using (SolidBrush activeBrush = new SolidBrush(Color.FromArgb(14, 165, 233)))
                {
                    g.FillPath(activeBrush, activePath);
                }
            }

            // Circular Thumb
            int thumbX = trackMargin + activeW;
            int thumbY = this.Height / 2;
            int thumbR = 8;

            // Glow ring
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(50, 14, 165, 233)))
            {
                g.FillEllipse(glow, thumbX - thumbR - 3, thumbY - thumbR - 3, (thumbR * 2) + 6, (thumbR * 2) + 6);
            }
            // Thumb inner
            using (SolidBrush thumbBrush = new SolidBrush(Color.FromArgb(240, 249, 255)))
            using (Pen thumbBorder = new Pen(Color.FromArgb(14, 165, 233), 2.5f))
            {
                g.FillEllipse(thumbBrush, thumbX - thumbR, thumbY - thumbR, thumbR * 2, thumbR * 2);
                g.DrawEllipse(thumbBorder, thumbX - thumbR, thumbY - thumbR, thumbR * 2, thumbR * 2);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class MainForm : Form
    {
        public const string CURRENT_VERSION = "1.0.1";
        private const string VERSION_URL = "https://raw.githubusercontent.com/garysung0/smooth-scroller/main/version.txt";
        private const string EXE_URL = "https://github.com/garysung0/smooth-scroller/raw/main/SmoothScroller.exe";

        // Win32 API imports for Input & Global Hooks
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const uint INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _keyboardProc;
        private IntPtr _hookId = IntPtr.Zero;

        // App state
        private bool isScrolling = false;
        private bool scrollDown = true;
        private int speed = 8; // 1 to 30 px/s
        private bool isCompact = false;

        // High precision 60 FPS animation timer
        private System.Windows.Forms.Timer scrollTimer;
        private Stopwatch scrollStopwatch = new Stopwatch();

        // UI Controls
        private Panel headerPanel;
        private Label titleLabel;
        private Button closeBtn;
        private Button minBtn;
        private Button compactToggleBtn;
        private Button updateBtn;
        private Panel bodyPanel;

        private Button toggleBtn;
        private Button dirBtn;
        private ModernSlider speedSlider;
        private Label speedLabel;
        private Label statusBadge;
        private CheckBox onTopCheck;
        private Label hintLabel;

        private Button slowPreset1;
        private Button slowPreset2;
        private Button slowPreset3;
        private Button slowPreset4;
        private Button slowPreset5;
        private Button slowerBtn;
        private Button fasterBtn;
        private Label speedTitle;

        // Accumulator for smooth continuous micro-steps
        private double fractionalAccumulator = 0.0;
        private string latestRemoteVersion = null;

        public MainForm()
        {
            InitializeComponent();
            SetupScrollTimer();
            InstallKeyboardHook();
            CheckForUpdatesInBackground();
        }

        private void InitializeComponent()
        {
            this.Text = "ReadFlow - Smooth Auto Scroller";
            this.Size = new Size(380, 296);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 27); // Zinc 900
            this.ForeColor = Color.FromArgb(244, 244, 245);
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            // Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(39, 39, 42),
                Cursor = Cursors.SizeAll
            };
            headerPanel.MouseDown += Header_MouseDown;

            titleLabel = new Label
            {
                Text = "ReadFlow v" + CURRENT_VERSION,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 250, 250),
                AutoSize = true,
                Location = new Point(12, 9),
                Cursor = Cursors.SizeAll
            };
            titleLabel.MouseDown += Header_MouseDown;

            closeBtn = new Button
            {
                Text = "✕",
                Size = new Size(32, 26),
                Location = new Point(342, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(161, 161, 170),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 29, 72);
            closeBtn.Click += (s, e) => Application.Exit();

            minBtn = new Button
            {
                Text = "—",
                Size = new Size(32, 26),
                Location = new Point(308, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(161, 161, 170),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            minBtn.FlatAppearance.BorderSize = 0;
            minBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(63, 63, 70);
            minBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            compactToggleBtn = new Button
            {
                Text = "Mini",
                Size = new Size(48, 24),
                Location = new Point(252, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(212, 212, 216),
                BackColor = Color.FromArgb(63, 63, 70),
                Cursor = Cursors.Hand
            };
            compactToggleBtn.FlatAppearance.BorderSize = 0;
            compactToggleBtn.Click += (s, e) => ToggleCompactMode();

            updateBtn = new Button
            {
                Text = "⬆ Update",
                Size = new Size(74, 24),
                Location = new Point(172, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Cursor = Cursors.Hand,
                Visible = false
            };
            updateBtn.FlatAppearance.BorderSize = 0;
            updateBtn.Click += (s, e) => ApplyUpdate();

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(updateBtn);
            headerPanel.Controls.Add(compactToggleBtn);
            headerPanel.Controls.Add(minBtn);
            headerPanel.Controls.Add(closeBtn);

            // Body Container
            bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 27)
            };

            // Status Badge (Clean & concise)
            statusBadge = new Label
            {
                Text = "PAUSED",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(251, 191, 36),
                BackColor = Color.FromArgb(45, 36, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(348, 24),
                Location = new Point(16, 8)
            };

            // Start/Pause Button
            toggleBtn = new Button
            {
                Text = "START (F8)",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(238, 40),
                Location = new Point(16, 38),
                Cursor = Cursors.Hand
            };
            toggleBtn.FlatAppearance.BorderSize = 0;
            toggleBtn.Click += (s, e) => ToggleScroll();

            // Direction Button
            dirBtn = new Button
            {
                Text = "▼ Down",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(228, 228, 231),
                BackColor = Color.FromArgb(39, 39, 42),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(102, 40),
                Location = new Point(262, 38),
                Cursor = Cursors.Hand
            };
            dirBtn.FlatAppearance.BorderSize = 1;
            dirBtn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            dirBtn.Click += (s, e) => ToggleDirection();

            // Speed Row
            speedTitle = new Label
            {
                Text = "Speed:",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(161, 161, 170),
                AutoSize = true,
                Location = new Point(16, 88)
            };

            speedLabel = new Label
            {
                Text = "8 px/s",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                AutoSize = true,
                Location = new Point(64, 87)
            };

            slowerBtn = new Button
            {
                Text = "—",
                Size = new Size(26, 24),
                Location = new Point(16, 114),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 39, 42),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            slowerBtn.FlatAppearance.BorderSize = 1;
            slowerBtn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            slowerBtn.Click += (s, e) => AdjustSpeed(-1);

            speedSlider = new ModernSlider
            {
                Value = speed,
                Location = new Point(46, 114),
                Size = new Size(288, 24)
            };
            speedSlider.ValueChanged += (s, e) => SetSpeed(speedSlider.Value);

            fasterBtn = new Button
            {
                Text = "+",
                Size = new Size(26, 24),
                Location = new Point(338, 114),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 39, 42),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            fasterBtn.FlatAppearance.BorderSize = 1;
            fasterBtn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            fasterBtn.Click += (s, e) => AdjustSpeed(1);

            // 5 Minimal Presets
            slowPreset1 = CreatePresetButton("Crawl (2)", 2, new Point(16, 148), new Size(64, 26));
            slowPreset2 = CreatePresetButton("Gentle (6)", 6, new Point(84, 148), new Size(66, 26));
            slowPreset3 = CreatePresetButton("Club (10)", 10, new Point(154, 148), new Size(66, 26));
            slowPreset4 = CreatePresetButton("Flow (16)", 16, new Point(224, 148), new Size(66, 26));
            slowPreset5 = CreatePresetButton("Brisk (24)", 24, new Point(294, 148), new Size(70, 26));

            // Options Row
            onTopCheck = new CheckBox
            {
                Text = "Always On Top",
                Checked = true,
                AutoSize = true,
                Location = new Point(18, 186),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(212, 212, 216),
                Cursor = Cursors.Hand
            };
            onTopCheck.CheckedChanged += (s, e) => this.TopMost = onTopCheck.Checked;

            // Single clean hotkey cheat line
            hintLabel = new Label
            {
                Text = "F8: Start/Pause   •   [ / ]: Speed   •   F7: Reverse",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(348, 20),
                Location = new Point(16, 222)
            };

            bodyPanel.Controls.Add(statusBadge);
            bodyPanel.Controls.Add(toggleBtn);
            bodyPanel.Controls.Add(dirBtn);
            bodyPanel.Controls.Add(speedTitle);
            bodyPanel.Controls.Add(speedLabel);
            bodyPanel.Controls.Add(slowerBtn);
            bodyPanel.Controls.Add(speedSlider);
            bodyPanel.Controls.Add(fasterBtn);
            bodyPanel.Controls.Add(slowPreset1);
            bodyPanel.Controls.Add(slowPreset2);
            bodyPanel.Controls.Add(slowPreset3);
            bodyPanel.Controls.Add(slowPreset4);
            bodyPanel.Controls.Add(slowPreset5);
            bodyPanel.Controls.Add(onTopCheck);
            bodyPanel.Controls.Add(hintLabel);

            this.Controls.Add(bodyPanel);
            this.Controls.Add(headerPanel);

            // Border
            this.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Color.FromArgb(63, 63, 70), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };
        }

        private void ToggleCompactMode()
        {
            isCompact = !isCompact;
            if (isCompact)
            {
                compactToggleBtn.Text = "Full";
                this.Size = new Size(380, 80);
                bodyPanel.Controls.Clear();

                toggleBtn.Location = new Point(10, 5);
                toggleBtn.Size = new Size(196, 32);
                toggleBtn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

                dirBtn.Location = new Point(212, 5);
                dirBtn.Size = new Size(74, 32);

                Button minSlower = new Button
                {
                    Text = "—",
                    Size = new Size(38, 32),
                    Location = new Point(292, 5),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(39, 39, 42),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                minSlower.FlatAppearance.BorderSize = 1;
                minSlower.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
                minSlower.Click += (s, e) => AdjustSpeed(-1);

                Button minFaster = new Button
                {
                    Text = "+",
                    Size = new Size(38, 32),
                    Location = new Point(334, 5),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(39, 39, 42),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                minFaster.FlatAppearance.BorderSize = 1;
                minFaster.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
                minFaster.Click += (s, e) => AdjustSpeed(1);

                bodyPanel.Controls.Add(toggleBtn);
                bodyPanel.Controls.Add(dirBtn);
                bodyPanel.Controls.Add(minSlower);
                bodyPanel.Controls.Add(minFaster);
            }
            else
            {
                compactToggleBtn.Text = "Mini";
                this.Size = new Size(380, 296);
                bodyPanel.Controls.Clear();
                RestoreFullBodyControls();
            }
            this.Refresh();
        }

        private void RestoreFullBodyControls()
        {
            toggleBtn.Location = new Point(16, 38);
            toggleBtn.Size = new Size(238, 40);
            toggleBtn.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);

            dirBtn.Location = new Point(262, 38);
            dirBtn.Size = new Size(102, 40);

            bodyPanel.Controls.Add(statusBadge);
            bodyPanel.Controls.Add(toggleBtn);
            bodyPanel.Controls.Add(dirBtn);
            bodyPanel.Controls.Add(speedTitle);
            bodyPanel.Controls.Add(speedLabel);
            bodyPanel.Controls.Add(slowerBtn);
            bodyPanel.Controls.Add(speedSlider);
            bodyPanel.Controls.Add(fasterBtn);
            bodyPanel.Controls.Add(slowPreset1);
            bodyPanel.Controls.Add(slowPreset2);
            bodyPanel.Controls.Add(slowPreset3);
            bodyPanel.Controls.Add(slowPreset4);
            bodyPanel.Controls.Add(slowPreset5);
            bodyPanel.Controls.Add(onTopCheck);
            bodyPanel.Controls.Add(hintLabel);

            UpdateStatusUI();
        }

        private Button CreatePresetButton(string text, int targetSpeed, Point loc, Size sz)
        {
            Button btn = new Button
            {
                Text = text,
                Location = loc,
                Size = sz,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(212, 212, 216),
                BackColor = Color.FromArgb(39, 39, 42),
                Font = new Font("Segoe UI", 7.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(63, 63, 70);
            btn.Click += (s, e) => SetSpeed(targetSpeed);
            return btn;
        }

        private void SetupScrollTimer()
        {
            scrollTimer = new System.Windows.Forms.Timer();
            scrollTimer.Interval = 16; // 60 FPS
            scrollTimer.Tick += ScrollTimer_Tick;
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (!isScrolling) return;

            POINT pt;
            GetCursorPos(out pt);
            bool isOverSelf = this.Bounds.Contains(new Point(pt.X, pt.Y));

            if (isOverSelf)
            {
                statusBadge.Text = "HOVER OVER BROWSER";
                statusBadge.ForeColor = Color.FromArgb(253, 224, 71);
                statusBadge.BackColor = Color.FromArgb(45, 36, 18);
                scrollStopwatch.Restart();
                return;
            }
            else
            {
                statusBadge.Text = "SCROLLING  (" + speed + " px/s)";
                statusBadge.ForeColor = Color.FromArgb(52, 211, 153);
                statusBadge.BackColor = Color.FromArgb(6, 78, 59);
            }

            double elapsedSeconds = scrollStopwatch.Elapsed.TotalSeconds;
            scrollStopwatch.Restart();

            if (elapsedSeconds > 0.08) elapsedSeconds = 0.016;

            int directionMultiplier = scrollDown ? -1 : 1;

            double unitsToAdd = (speed * 1.25) * elapsedSeconds;
            fractionalAccumulator += unitsToAdd;

            int unitsToSend = (int)fractionalAccumulator;
            if (unitsToSend >= 1)
            {
                fractionalAccumulator -= unitsToSend;
                SendWheelEvent(unitsToSend * directionMultiplier);
            }
        }

        private void SendWheelEvent(int wheelDelta)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi.dx = 0;
            inputs[0].mi.dy = 0;
            inputs[0].mi.dwFlags = MOUSEEVENTF_WHEEL;
            inputs[0].mi.mouseData = unchecked((uint)wheelDelta);
            inputs[0].mi.time = 0;
            inputs[0].mi.dwExtraInfo = IntPtr.Zero;

            uint sent = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (sent == 0)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)wheelDelta), UIntPtr.Zero);
            }
        }

        private void ToggleScroll()
        {
            isScrolling = !isScrolling;
            fractionalAccumulator = 0.0;
            scrollStopwatch.Restart();
            UpdateStatusUI();
        }

        private void ToggleDirection()
        {
            scrollDown = !scrollDown;
            dirBtn.Text = scrollDown ? "▼ Down" : "▲ Up";
            UpdateStatusUI();
        }

        private void SetSpeed(int newSpeed)
        {
            speed = Math.Max(1, Math.Min(30, newSpeed));
            if (speedSlider != null) speedSlider.Value = speed;
            if (speedLabel != null) speedLabel.Text = speed + " px/s";
            UpdateStatusUI();
        }

        private void AdjustSpeed(int delta)
        {
            SetSpeed(speed + delta);
        }

        private void UpdateStatusUI()
        {
            if (isScrolling)
            {
                scrollTimer.Start();
                scrollStopwatch.Restart();
                toggleBtn.Text = "PAUSE (F8)";
                toggleBtn.BackColor = Color.FromArgb(239, 68, 68);
                statusBadge.Text = "SCROLLING  (" + speed + " px/s)";
                statusBadge.ForeColor = Color.FromArgb(52, 211, 153);
                statusBadge.BackColor = Color.FromArgb(6, 78, 59);
            }
            else
            {
                scrollTimer.Stop();
                scrollStopwatch.Stop();
                toggleBtn.Text = "START (F8)";
                toggleBtn.BackColor = Color.FromArgb(16, 185, 129);
                statusBadge.Text = "PAUSED";
                statusBadge.ForeColor = Color.FromArgb(251, 191, 36);
                statusBadge.BackColor = Color.FromArgb(45, 36, 18);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                ToggleScroll();
                e.Handled = true;
            }
        }

        private void InstallKeyboardHook()
        {
            _keyboardProc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void UninstallKeyboardHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isCtrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;

                if (vkCode == (int)Keys.F8 || vkCode == (int)Keys.F9 || (isCtrl && vkCode == (int)Keys.Space))
                {
                    this.BeginInvoke((MethodInvoker)delegate { ToggleScroll(); });
                    return (IntPtr)1;
                }
                else if (vkCode == (int)Keys.F7 || vkCode == (int)Keys.F6)
                {
                    this.BeginInvoke((MethodInvoker)delegate { ToggleDirection(); });
                    return (IntPtr)1;
                }
                else if (isScrolling && (vkCode == 219 || vkCode == 189))
                {
                    this.BeginInvoke((MethodInvoker)delegate { AdjustSpeed(-1); });
                }
                else if (isScrolling && (vkCode == 221 || vkCode == 187))
                {
                    this.BeginInvoke((MethodInvoker)delegate { AdjustSpeed(1); });
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void CheckForUpdatesInBackground()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072; // TLS 1.2
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        string remoteVerStr = wc.DownloadString(VERSION_URL).Trim();
                        Version remoteVer = new Version(remoteVerStr);
                        Version currentVer = new Version(CURRENT_VERSION);

                        if (remoteVer > currentVer)
                        {
                            latestRemoteVersion = remoteVerStr;
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                updateBtn.Text = "⬆ v" + remoteVerStr;
                                updateBtn.Visible = true;
                                statusBadge.Text = "UPDATE AVAILABLE (v" + remoteVerStr + ")";
                                statusBadge.ForeColor = Color.FromArgb(52, 211, 153);
                            });
                        }
                    }
                }
                catch { }
            });
        }

        private void ApplyUpdate()
        {
            string ver = latestRemoteVersion ?? "latest";
            DialogResult res = MessageBox.Show(
                string.Format("Update to version {0} now?\nThe app will refresh automatically.", ver),
                "Update ReadFlow",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (res != DialogResult.Yes) return;

            try
            {
                updateBtn.Text = "...";
                updateBtn.Enabled = false;

                string currentExe = Application.ExecutablePath;
                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string tempExe = Path.Combine(currentDir, "SmoothScroller_new.exe");

                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                    wc.DownloadFile(EXE_URL, tempExe);
                }

                string script = string.Format(
                    "Start-Sleep -Milliseconds 600; Move-Item -Force '{0}' '{1}'; Start-Process '{1}'",
                    tempExe, currentExe);

                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", "-WindowStyle Hidden -Command \"" + script + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed: " + ex.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                updateBtn.Text = "⬆ Retry";
                updateBtn.Enabled = true;
            }
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            scrollTimer.Stop();
            UninstallKeyboardHook();
            base.OnFormClosing(e);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
