# ✨ MixedRealityToolkit-MRTK3-WebView2-Hololens2
.
This Unity project demonstrates advanced hand gesture-based interactions for navigating webpages in **WebView2**, designed specifically for **HoloLens 2** using **MRTK3** and **XR Interaction Toolkit**. Also, refer to the officiali documentation on [Getting started with WebView2 in HoloLens 2 Unity apps](https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/hololens2) 

While the default WebView2 prefab supports basic browser controls (like back, forward, and URL input), this project enhances usability with **natural hand gestures** — making browsing in mixed reality more intuitive and immersive.

---

## 📌 Features

- 🔗 Full integration with **WebView2** in Unity
- 🖱️ Click webpage content using:
  - Far-ray pinch gesture
  - Near-poke (touch) interaction
- 📜 Scroll webpage using far-ray swipe gesture (hand motion up/down)
- 🧩 Plug-and-play Unity components (scripts)
- ✅ Compatible with MRTK3 and OpenXR-based hand tracking
- 🎮 Demo scene included: `WebviewDemo.unity`

---

## 🎬 Demo Videos

📽 **General Demo**  
Shows all interaction types while navigating a webpage in MR.

📽 **Click Interaction Showcase**  
Focuses specifically on the two available modes of clicking:

- Far-ray + pinch
- Near-poke (touch)

> _You can find these videos in the `DemoVideos/` folder or link them externally if hosted._

---

## 🛠️ How to Use

### Option 1: Try the Demo Scene

The easiest way to explore the features is to open:

> `Assets/Scenes/WebviewDemo.unity`

This scene includes all components wired up and ready to test.

---

### Option 2: Add to Your Own Scene

#### 1. Add WebView2 to the Scene

Follow the official WebView2 Unity integration guide to:

- Add the **WebView2 prefab** to your scene
- Attach `WebViewBrowser.cs` to the prefab
- Hook up:
  - Back button
  - Forward button
  - URL field (TextMeshPro)

#### 2. Add Gesture Interaction Scripts

To enable advanced interactions, add the following components to the WebView GameObject:

- `ClickInteractable.cs`  
  Enables clicking via:
  - Far-ray + pinch
  - Near-poke (touch)

- `FarRaySwipeInteractable.cs`  
  Enables scrolling by far-ray **hand swipe up/down** gesture

📁 Scripts are located at:

```
├── Assets
│   ├── Scripts
│   ├── ├── WebviewInteractables
│   ├── ├── ├── FarRaySwipeInteracatable.cs
│   ├── ├── ├── ClickInteracatable.cs

```