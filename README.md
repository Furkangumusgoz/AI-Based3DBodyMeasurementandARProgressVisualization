# Holo-Fit: AI-Based 3D Body Measurement & AR Progress Visualization 🧍‍♂️📏

Holo-Fit is a privacy-first, offline AI system built with Unity and C# that extracts highly accurate 3D body measurements from static images and visualizes physical progress through 3D Avatars and Augmented Reality (AR). 

Unlike standard medical-grade anthropometric tools, Holo-Fit is designed as a secure, AI-assisted body change visualization and progress tracking system. Its core purpose is to provide consistent relative change detection and motivating visual feedback across fitness tracking sessions, supported by approximate body measurements.

---

## ✨ Key Features

* 📷 **Static Image Processing:** Utilizes MediaPipe Pose Landmark Detection on 4 static images (Front, Right, Back, Left) to eliminate the "temporal jitter" often found in live-feed AI trackers.
* 🧮 **Custom Mathematical Engine:** Replaces standard cylindrical body approximations with advanced C# algorithms that calculate depth and width using elliptical perimeters for true anatomical accuracy.
* 🔒 **100% Offline & Privacy-First:** Operates entirely on-device (On-Device Processing). User data and images are never uploaded to a cloud server.
* 💾 **Local JSON Database:** Features a custom local storage system to save, track, and compare historical session data natively on the device.
* 🧍‍♂️ **3D Avatar Visualization:** Serializes accurate measurement data to generate and modify 3D holograms, allowing users to visually compare their physical progress over time.

---

## 🛠️ Tech Stack

* **Game Engine:** Unity 3D
* **Language:** C#
* **AI / Vision:** MediaPipe Unity Plugin (Homuler)
* **Data Management:** Custom Local JSON Serialization

---

## ⚙️ How It Works

1.  **Capture:** The user uploads or captures 4 static profile photos.
2.  **Analyze:** The MediaPipe runner detects skeletal landmarks.
3.  **Calculate:** The C# mathematical engine processes the coordinates and scales them using the user's height and custom calibration thresholds.
4.  **Save:** The data is serialized into a local JSON file stamped with the session date.
5.  **Visualize:** The `MeasurementComparisonSystem` compares historical data and morphs the 3D Avatar/Hologram to reflect body changes (e.g., increased chest size, decreased waist).

---

## 🚀 Architecture Highlights

* **MeasurementLoadSystem / MeasurementSaveSystem:** Robust, conflict-free local database managers that handle unique session IDs and daily overwrite rules.
* **Zero Cloud Dependency:** The entire pose estimation and mathematical calculation pipeline relies solely on the host device's CPU/GPU.
