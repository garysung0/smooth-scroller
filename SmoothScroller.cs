using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmoothScroller
{
    // Custom modern slider: no black ticks, no WinForms clipping bugs, sleek dark style
    public class ModernSlider : Control
    {
        private int val = 12;
        private int min = 1;
        private int max = 100;
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
            this.Value = (int)(min + fraction * (max - min));
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
        // Win32 API imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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
        private const int WM_HOTKEY = 0x0312;

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

        // Hotkey IDs
        private const int HOTKEY_TOGGLE_F8 = 1001;
        private const int HOTKEY_TOGGLE_CTRL_SPACE = 1002;
        private const int HOTKEY_SLOWER = 1003;
        private const int HOTKEY_FASTER = 1004;
        private const int HOTKEY_REVERSE = 1005;

        // Modifiers
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;

        // Virtual Keys
        private const uint VK_F8 = 0x77;
        private const uint VK_F7 = 0x76;
        private const uint VK_SPACE = 0x20;
        private const uint VK_OEM_4 = 0xDB; // '['
        private const uint VK_OEM_6 = 0xDD; // ']'

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // App state
        private bool isScrolling = false;
        private bool scrollDown = true;
        private int speed = 10; // 1 to 100 (default 10 = calm reading)
        private bool smoothMode = true; // Liquid continuous glide
        private bool isCompact = false;
        private System.Windows.Forms.Timer scrollTimer;

        // UI Controls
        private Panel headerPanel;
        private Label titleLabel;
        private Button closeBtn;
        private Button minBtn;
        private Button compactToggleBtn;
        private Panel bodyPanel;

        private Button toggleBtn;
        private Button dirBtn;
        private ModernSlider speedSlider;
        private Label speedLabel;
        private Label wpmLabel;
        private Label statusBadge;
        private CheckBox onTopCheck;
        private CheckBox smoothCheck;
        private Label hintLabel;

        private Button slowPreset;
        private Button bookPreset;
        private Button briskPreset;
        private Button skimPreset;
        private Button slowerBtn;
        private Button fasterBtn;
        private Label speedTitle;

        // Accumulator for smooth continuous micro-steps
        private double fractionalAccumulator = 0.0;

        public MainForm()
        {
            InitializeComponent();
            SetupScrollTimer();
            RegisterAppHotkeys();
        }

        private void InitializeComponent()
        {
            this.Text = "ReadFlow - Smooth Auto Scroller";
            this.Size = new Size(400, 356);
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

            // Header Panel (Draggable title bar)
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.FromArgb(39, 39, 42),
                Cursor = Cursors.SizeAll
            };
            headerPanel.MouseDown += Header_MouseDown;

            titleLabel = new Label
            {
                Text = "ReadFlow Scroller",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 250, 250),
                AutoSize = true,
                Location = new Point(14, 9),
                Cursor = Cursors.SizeAll
            };
            titleLabel.MouseDown += Header_MouseDown;

            closeBtn = new Button
            {
                Text = "✕",
                Size = new Size(34, 28),
                Location = new Point(360, 5),
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
                Size = new Size(34, 28),
                Location = new Point(324, 5),
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
                Text = "Mini Bar",
                Size = new Size(68, 26),
                Location = new Point(250, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(212, 212, 216),
                BackColor = Color.FromArgb(63, 63, 70),
                Cursor = Cursors.Hand
            };
            compactToggleBtn.FlatAppearance.BorderSize = 0;
            compactToggleBtn.Click += (s, e) => ToggleCompactMode();

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(compactToggleBtn);
            headerPanel.Controls.Add(minBtn);
            headerPanel.Controls.Add(closeBtn);

            // Main Body Container
            bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 27)
            };

            // Status Badge
            statusBadge = new Label
            {
                Text = "PAUSED  (Press F8 to Scroll)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(251, 191, 36), // Amber
                BackColor = Color.FromArgb(45, 36, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(368, 26),
                Location = new Point(16, 10)
            };

            // Big Start/Stop Button
            toggleBtn = new Button
            {
                Text = "START SCROLLING (F8)",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald 500
                FlatStyle = FlatStyle.Flat,
                Size = new Size(254, 44),
                Location = new Point(16, 44),
                Cursor = Cursors.Hand
            };
            toggleBtn.FlatAppearance.BorderSize = 0;
            toggleBtn.Click += (s, e) => ToggleScroll();

            // Direction Toggle Button
            dirBtn = new Button
            {
                Text = "Down v",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(228, 228, 231),
                BackColor = Color.FromArgb(39, 39, 42),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(106, 44),
                Location = new Point(278, 44),
                Cursor = Cursors.Hand
            };
            dirBtn.FlatAppearance.BorderSize = 1;
            dirBtn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            dirBtn.Click += (s, e) => ToggleDirection();

            // Speed Control Row
            speedTitle = new Label
            {
                Text = "Reading Speed:",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(161, 161, 170),
                AutoSize = true,
                Location = new Point(16, 98)
            };

            speedLabel = new Label
            {
                Text = "10 px/s",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248), // Sky blue
                AutoSize = true,
                Location = new Point(112, 97)
            };

            wpmLabel = new Label
            {
                Text = "~80 WPM (Calm / In-depth Reading)",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(161, 161, 170),
                AutoSize = true,
                Location = new Point(176, 98)
            };

            slowerBtn = new Button
            {
                Text = "—",
                Size = new Size(28, 26),
                Location = new Point(16, 126),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 39, 42),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            slowerBtn.FlatAppearance.BorderSize = 1;
            slowerBtn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            slowerBtn.Click += (s, e) => AdjustSpeed(-3);

            speedSlider = new ModernSlider
            {
                Value = speed,
                Location = new Point(50, 126),
                Size = new Size(304, 26)
            };
            speedSlider.ValueChanged += (s, e) => SetSpeed(speedSlider.Value);

            fasterBtn = new Button
            {
                Text = "+",
                Size = new Size(28, 26),
                Location = new Point(356, 126),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 39, 42),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            fasterBtn.FlatAppearance.BorderSize = 1;
            fasterBtn.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            fasterBtn.Click += (s, e) => AdjustSpeed(3);

            // Preset Buttons tuned for real group reading
            slowPreset = CreatePresetButton("Very Slow (4)", 4, new Point(16, 164), new Size(84, 28));
            bookPreset = CreatePresetButton("Calm (10)", 10, new Point(110, 164), new Size(84, 28));
            briskPreset = CreatePresetButton("Reader (20)", 20, new Point(204, 164), new Size(84, 28));
            skimPreset = CreatePresetButton("Brisk (35)", 35, new Point(298, 164), new Size(86, 28));

            // Options Row
            onTopCheck = new CheckBox
            {
                Text = "Always On Top",
                Checked = true,
                AutoSize = true,
                Location = new Point(20, 206),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(212, 212, 216),
                Cursor = Cursors.Hand
            };
            onTopCheck.CheckedChanged += (s, e) => this.TopMost = onTopCheck.Checked;

            smoothCheck = new CheckBox
            {
                Text = "Liquid Continuous Glide (Zero-Burst, Buttery Smooth)",
                Checked = true,
                AutoSize = true,
                Location = new Point(136, 206),
                Font = new Font("Segoe UI", 8.2f),
                ForeColor = Color.FromArgb(212, 212, 216),
                Cursor = Cursors.Hand
            };
            smoothCheck.CheckedChanged += (s, e) => { smoothMode = smoothCheck.Checked; };

            // Hotkey cheat sheet footer
            hintLabel = new Label
            {
                Text = "Hotkeys: F8: Play/Pause  |  [: Slower  |  ]: Faster  |  F7: Reverse\nMove mouse over your browser window to auto-scroll.",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(368, 36),
                Location = new Point(16, 240)
            };

            bodyPanel.Controls.Add(statusBadge);
            bodyPanel.Controls.Add(toggleBtn);
            bodyPanel.Controls.Add(dirBtn);
            bodyPanel.Controls.Add(speedTitle);
            bodyPanel.Controls.Add(speedLabel);
            bodyPanel.Controls.Add(wpmLabel);
            bodyPanel.Controls.Add(slowerBtn);
            bodyPanel.Controls.Add(speedSlider);
            bodyPanel.Controls.Add(fasterBtn);
            bodyPanel.Controls.Add(slowPreset);
            bodyPanel.Controls.Add(bookPreset);
            bodyPanel.Controls.Add(briskPreset);
            bodyPanel.Controls.Add(skimPreset);
            bodyPanel.Controls.Add(onTopCheck);
            bodyPanel.Controls.Add(smoothCheck);
            bodyPanel.Controls.Add(hintLabel);

            this.Controls.Add(bodyPanel);
            this.Controls.Add(headerPanel);

            // Outer border paint
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
                compactToggleBtn.Text = "Full UI";
                this.Size = new Size(400, 88);
                bodyPanel.Controls.Clear();

                toggleBtn.Location = new Point(12, 6);
                toggleBtn.Size = new Size(204, 36);
                toggleBtn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

                dirBtn.Location = new Point(222, 6);
                dirBtn.Size = new Size(78, 36);

                Button minSlower = new Button
                {
                    Text = "—",
                    Size = new Size(40, 36),
                    Location = new Point(306, 6),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(39, 39, 42),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                minSlower.FlatAppearance.BorderSize = 1;
                minSlower.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
                minSlower.Click += (s, e) => AdjustSpeed(-3);

                Button minFaster = new Button
                {
                    Text = "+",
                    Size = new Size(40, 36),
                    Location = new Point(350, 6),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(39, 39, 42),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                minFaster.FlatAppearance.BorderSize = 1;
                minFaster.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
                minFaster.Click += (s, e) => AdjustSpeed(3);

                bodyPanel.Controls.Add(toggleBtn);
                bodyPanel.Controls.Add(dirBtn);
                bodyPanel.Controls.Add(minSlower);
                bodyPanel.Controls.Add(minFaster);
            }
            else
            {
                compactToggleBtn.Text = "Mini Bar";
                this.Size = new Size(400, 356);
                bodyPanel.Controls.Clear();
                RestoreFullBodyControls();
            }
            this.Refresh();
        }

        private void RestoreFullBodyControls()
        {
            toggleBtn.Location = new Point(16, 44);
            toggleBtn.Size = new Size(254, 44);
            toggleBtn.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);

            dirBtn.Location = new Point(278, 44);
            dirBtn.Size = new Size(106, 44);

            bodyPanel.Controls.Add(statusBadge);
            bodyPanel.Controls.Add(toggleBtn);
            bodyPanel.Controls.Add(dirBtn);
            bodyPanel.Controls.Add(speedTitle);
            bodyPanel.Controls.Add(speedLabel);
            bodyPanel.Controls.Add(wpmLabel);
            bodyPanel.Controls.Add(slowerBtn);
            bodyPanel.Controls.Add(speedSlider);
            bodyPanel.Controls.Add(fasterBtn);
            bodyPanel.Controls.Add(slowPreset);
            bodyPanel.Controls.Add(bookPreset);
            bodyPanel.Controls.Add(briskPreset);
            bodyPanel.Controls.Add(skimPreset);
            bodyPanel.Controls.Add(onTopCheck);
            bodyPanel.Controls.Add(smoothCheck);
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
                Font = new Font("Segoe UI", 8f),
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
            scrollTimer.Interval = 25; // 40 FPS continuous high-cadence refresh
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
                // When cursor is resting on the tool itself, don't scroll the tool
                statusBadge.Text = "ACTIVE (Move mouse over your browser window)";
                statusBadge.ForeColor = Color.FromArgb(253, 224, 71); // Yellow
                statusBadge.BackColor = Color.FromArgb(45, 36, 18);
                return;
            }
            else
            {
                string dirStr = scrollDown ? "DOWN" : "UP";
                statusBadge.Text = string.Format("SCROLLING  ({0} @ {1} px/s)", dirStr, speed);
                statusBadge.ForeColor = Color.FromArgb(52, 211, 153); // Emerald
                statusBadge.BackColor = Color.FromArgb(6, 78, 59);
            }

            int directionMultiplier = scrollDown ? -1 : 1;

            if (smoothMode)
            {
                // Liquid Continuous Glide:
                // Instead of sending 60 or 120 chunks (which cause discrete bursts),
                // we feed steady micro-steps at 40 FPS (every 25ms).
                // speed is 1 to 100:
                // At speed 4 (Very Slow): adds 0.16 units/tick (~6 units/sec = ~5 px/s). Emits 1 unit every ~150ms.
                // At speed 10 (Calm): adds 0.40 units/tick (~16 units/sec = ~14 px/s). Emits 1 unit every ~60ms.
                // At speed 20 (Reader): adds 0.80 units/tick (~32 units/sec = ~28 px/s). Emits 1 unit every ~30ms.
                // At speed 35 (Brisk): adds 1.40 units/tick (~56 units/sec = ~48 px/s).
                // The browser's scroll animation NEVER stops or pauses. It glides steadily like a gentle teleprompter.
                double deltaPerTick = (speed * 0.04);
                fractionalAccumulator += deltaPerTick;

                int unitsToSend = (int)fractionalAccumulator;
                if (unitsToSend >= 1)
                {
                    fractionalAccumulator -= unitsToSend;
                    SendWheelEvent(unitsToSend * directionMultiplier);
                }
            }
            else
            {
                // Stepped Notch Mode:
                // For legacy apps that require standard 120-unit detents
                double deltaPerTick = (speed * 0.15);
                fractionalAccumulator += deltaPerTick;

                if (fractionalAccumulator >= 120.0)
                {
                    fractionalAccumulator -= 120.0;
                    SendWheelEvent(120 * directionMultiplier);
                }
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
            UpdateStatusUI();
        }

        private void ToggleDirection()
        {
            scrollDown = !scrollDown;
            dirBtn.Text = scrollDown ? "Down v" : "Up ^";
            UpdateStatusUI();
        }

        private void SetSpeed(int newSpeed)
        {
            speed = Math.Max(1, Math.Min(100, newSpeed));
            if (speedSlider != null) speedSlider.Value = speed;
            if (speedLabel != null) speedLabel.Text = speed + " px/s";

            int estWpm = (int)(speed * 8);
            string mood;
            if (speed <= 5) mood = "Very Slow / Study";
            else if (speed <= 12) mood = "Calm Reading Pace";
            else if (speed <= 25) mood = "Natural Book Pace";
            else if (speed <= 45) mood = "Brisk Reading";
            else mood = "Fast Skimming";

            if (wpmLabel != null)
                wpmLabel.Text = string.Format("~{0} WPM ({1})", estWpm, mood);

            UpdateStatusUI();
        }

        private void AdjustSpeed(int delta)
        {
            SetSpeed(speed + delta);
        }

        private void UpdateStatusUI()
        {
            string dirStr = scrollDown ? "DOWN" : "UP";

            if (isScrolling)
            {
                scrollTimer.Start();
                toggleBtn.Text = isCompact ? "PAUSE" : "PAUSE SCROLLING (F8)";
                toggleBtn.BackColor = Color.FromArgb(239, 68, 68); // Red / Rose
                statusBadge.Text = string.Format("SCROLLING  ({0} @ {1} px/s)", dirStr, speed);
                statusBadge.ForeColor = Color.FromArgb(52, 211, 153); // Emerald text
                statusBadge.BackColor = Color.FromArgb(6, 78, 59); // Dark emerald bg
            }
            else
            {
                scrollTimer.Stop();
                toggleBtn.Text = isCompact ? "START" : "START SCROLLING (F8)";
                toggleBtn.BackColor = Color.FromArgb(16, 185, 129); // Emerald 500
                statusBadge.Text = "PAUSED  (Press F8 to Scroll)";
                statusBadge.ForeColor = Color.FromArgb(251, 191, 36); // Amber text
                statusBadge.BackColor = Color.FromArgb(45, 36, 18); // Dark amber bg
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

        private void RegisterAppHotkeys()
        {
            RegisterHotKey(this.Handle, HOTKEY_TOGGLE_F8, MOD_NONE, VK_F8);
            RegisterHotKey(this.Handle, HOTKEY_TOGGLE_CTRL_SPACE, MOD_CONTROL | MOD_ALT, VK_SPACE);
            RegisterHotKey(this.Handle, HOTKEY_SLOWER, MOD_NONE, VK_OEM_4);
            RegisterHotKey(this.Handle, HOTKEY_FASTER, MOD_NONE, VK_OEM_6);
            RegisterHotKey(this.Handle, HOTKEY_REVERSE, MOD_NONE, VK_F7);
        }

        private void UnregisterAppHotkeys()
        {
            UnregisterHotKey(this.Handle, HOTKEY_TOGGLE_F8);
            UnregisterHotKey(this.Handle, HOTKEY_TOGGLE_CTRL_SPACE);
            UnregisterHotKey(this.Handle, HOTKEY_SLOWER);
            UnregisterHotKey(this.Handle, HOTKEY_FASTER);
            UnregisterHotKey(this.Handle, HOTKEY_REVERSE);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_TOGGLE_F8:
                    case HOTKEY_TOGGLE_CTRL_SPACE:
                        ToggleScroll();
                        break;
                    case HOTKEY_SLOWER:
                        AdjustSpeed(-3);
                        break;
                    case HOTKEY_FASTER:
                        AdjustSpeed(3);
                        break;
                    case HOTKEY_REVERSE:
                        ToggleDirection();
                        break;
                }
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
            UnregisterAppHotkeys();
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
