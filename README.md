# ⚡ ReadFlow - Smooth Auto-Scroller for Screen Sharing & Reading Sessions

A lightweight, standalone Windows utility specifically designed for **Google Meet screen shares, book clubs, and collaborative reading sessions**. 

It provides gradual, steady auto-scrolling for web articles, PDFs, Google Docs, and eBooks without abrupt trackpad or mouse-wheel jerking, preventing words and sentences from getting cut off while people read together.

## 📥 Direct Download

[![Download ReadFlow.exe](https://img.shields.io/badge/Download-ReadFlow.exe%20(45%20KB)-10b981?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/garysung0/readflow/raw/main/ReadFlow.exe)

> Click the button above to download `ReadFlow.exe` directly (no installer, no Python, zero dependencies).

---

## 🚀 Instant Setup (Zero Dependencies)

- **Direct Executable**: [`ReadFlow.exe`](https://github.com/garysung0/readflow/raw/main/ReadFlow.exe) (or [`SmoothScroller.exe`](https://github.com/garysung0/readflow/raw/main/SmoothScroller.exe))
- **No Installation Required**: Works immediately on any standard Windows 10 or Windows 11 PC (uses built-in Windows .NET Framework 4.8 runtime).
- **Easy Sharing**: You can share this link or send `ReadFlow.exe` via email, Google Drive, Slack, or Discord. Anyone can double-click and run it instantly.

---

## 📖 How to Use During Google Meet

1. **Share your screen or browser window** on Google Meet as you normally do.
2. **Launch `ReadFlow.exe`** (it floats on top of your screen so you never lose track of it).
3. **Move your mouse pointer anywhere over your article or document**.
4. **Press `F8`** (or click the green **START SCROLLING** button).
5. The page will begin scrolling downward at a steady, comfortable reading speed.
6. **Press `F8` again to pause** whenever someone stops to discuss a paragraph or ask a question.

---

## ⌨️ Global Hotkeys

You can control scrolling at any moment **without switching away from your browser or presentation window**:

| Hotkey | Action | Description |
| :--- | :--- | :--- |
| **`F8`** | **Play / Pause** | Toggle scrolling on/off instantly |
| **`-`** | **Slower (-1 px/s)** | Decrease scrolling speed |
| **`+`** | **Faster (+1 px/s)** | Increase scrolling speed |
| **`F7`** | **Reverse Direction** | Switch between scrolling Down and Up |
| **`Ctrl + Alt + Space`** | **Alternative Toggle** | Backup shortcut to pause/resume |

---

## ✨ Features Built for Reading Groups

- **Continuous 1ms Multimedia Engine**: Bypasses the erratic Windows message loop with a dedicated high-priority multimedia background thread (`timeBeginPeriod(1)`). Fires strictly isochronous single-unit micro-pulses without multi-unit bunching, completely eliminating the subtle stop-and-go choppiness at slow reading speeds (0 - 15 px/s).
- **Precision Slow Speed Focus (0 – 15 px/s)**:
  - **Crawl (1 px/s)**: Ultra-slow line study, language translation, complex math formulas
  - **Study (3 px/s)**: ~45 WPM (dense technical documentation and academic research)
  - **Gentle (6 px/s)**: ~90 WPM (comfortable collaborative reading on Google Meet)
  - **Reader (10 px/s)**: ~160 WPM (standard conversational book club reading pace)
  - **Flow (14 px/s)**: ~220 WPM (brisk narrative and article flow)
- **Mini Pill Toolbar (`↕ Mini`)**: Shrinks the scroller into a tiny compact floating strip so it takes up minimal space on your shared screen.
- **Hover-Aware**: Scrolls whatever application is right under your mouse pointer (standard Windows behavior). It will never scroll while your cursor is hovering over the ReadFlow control window itself.
- **Always On Top Toggle**: Keeps the control toolbar visible while you present.
