# ⚡ ReadFlow - Smooth Auto-Scroller for Screen Sharing & Reading Sessions

A lightweight (37 KB), standalone Windows utility specifically designed for **Google Meet screen shares, book clubs, and collaborative reading sessions**. 

It provides gradual, steady auto-scrolling for web articles, PDFs, Google Docs, and eBooks without abrupt trackpad or mouse-wheel jerking, preventing words and sentences from getting cut off while people read together.

---

## 🚀 Instant Setup (Zero Dependencies)

- **Single Portable File**: [`SmoothScroller.exe`](file:///c:/Users/garys/OneDrive%20-%20The%20Ohio%20State%20University/My%20Books/AI%20Tools/smooth-scroller/SmoothScroller.exe)
- **No Installation Required**: Works immediately on any standard Windows 10 or Windows 11 PC (uses built-in Windows .NET Framework 4.8 runtime).
- **Easy Sharing**: You can email `SmoothScroller.exe`, put it in Google Drive / OneDrive, or send it over Slack/Discord. Anyone can double-click and run it instantly.

---

## 📖 How to Use During Google Meet

1. **Share your screen or browser window** on Google Meet as you normally do.
2. **Launch `SmoothScroller.exe`** (it floats on top of your screen so you never lose track of it).
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
| **`[`** | **Slower (-5 px/s)** | Decrease scrolling speed |
| **`]`** | **Faster (+5 px/s)** | Increase scrolling speed |
| **`F7`** | **Reverse Direction** | Switch between scrolling Down and Up |
| **`Ctrl + Alt + Space`** | **Alternative Toggle** | Backup shortcut to pause/resume |

---

## ✨ Features Built for Reading Groups

- **Silky Browser Engine**: Emits high-frequency micro-step wheel pulses (40 FPS) that modern browsers (Chrome, Edge, Firefox, Brave) and PDF viewers render smoothly, avoiding standard 3-line jumpiness.
- **Reading Pace Presets**:
  - **Gentle (12 px/s)**: ~90 WPM (for complex technical articles, dense study material, or language learners)
  - **Reader (25 px/s)**: ~190 WPM (standard book club reading pace)
  - **Brisk (45 px/s)**: ~340 WPM (for faster readers and narrative content)
  - **Skim (75 px/s)**: ~560 WPM (for quickly browsing through sections)
- **Mini Pill Toolbar (`↕ Mini`)**: Shrinks the scroller into a tiny compact floating strip so it takes up minimal space on your shared screen.
- **Hover-Aware**: Scrolls whatever application is right under your mouse pointer (standard Windows behavior). It will never scroll while your cursor is hovering over the ReadFlow control window itself.
- **Always On Top Toggle**: Keeps the control toolbar visible while you present.
