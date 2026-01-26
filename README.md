# XR2IND - Industrial VR Training Platform

**XR2IND** is a cutting-edge industrial training platform that merges immersive Virtual Reality (VR) environments with advanced AI assistance. It provides a realistic, safe, and interactive space for trainees to master complex industrial hardware and maintenance procedures.

## 🏗️ System Architecture

The platform consists of two main components working in tandem:

1.  **XR2IND-VR (Unity Application)**: The frontend VR experience running on Meta Quest 3, featuring realistic physics and interactions.
2.  **LLM Chat Engine (AI Backend)**: A Python/FastAPI service powered by OpenAI and RAG (Retrieval-Augmented Generation) that acts as an intelligent virtual instructor.

## 🥽 XR2IND-VR (The Virtual Environment)

The VR application offers three distinct training environments designed to build competence progressively:

*   **Tutorial Room**: A safe space for new users to acclimate to VR controls and interactions.
*   **Assembly Room**: Focused on the intricate assembly of routers and switches from individual components.
*   **Troubleshooting Room**: A realistic server room scenario where trainees diagnose and fix issues on live equipment.

### Interactive Hardware Models
Trainees interact with high-fidelity 3D models of industry-standard networking gear:
*   **Cisco ME4924**
*   **Juniper EX9204**
*   **Juniper MX480**

### Key Features
*   **Interactive Whiteboard**: Displays real-time task objectives and status updates.
*   **Full Component Interaction**: Manipulate line cards, routing engines, PSUs, fans, and cables.
*   **Tool Usage**: precise control with electric screwdrivers and various cable types.

[👉 Learn more in the VR Application README](XR2IND-VR/README.md)

## 🧠 LLM Chat Engine (The AI Instructor)

The intelligent backend serves as an always-available mentor. It understands natural language queries and provides accurate, context-aware answers based on official maintenance manuals.

*   **Retrieval-Augmented Generation (RAG)**: Indexes technical manuals to ensure high accuracy.
*   **Voice Interaction**: Seamless speech-to-text integration allows hands-free questioning during tasks.
*   **Contextual Awareness**: Remembers conversation history for natural dialogue.

### Hosted Instance
A live instance is available for the VR app to connect to immediately:
*   **Base URL**: `https://llm-chat-engine.onrender.com`
*   **Chat Endpoint**: `https://llm-chat-engine.onrender.com/api/chat`

[👉 Learn more in the Chat Engine README](llm-chat-engine/README.md)

## 🚀 Getting Started

### Prerequisites
*   **Hardware**: Meta Quest 3 Headset, VR-ready PC (for Link), USB 3.0 Cable.
*   **Software**: Unity Hub, Unity 2022.3.19f1, Meta Quest Link App.

### Quick Start Guide

1.  **Deploy the Backend** (Optional if using the hosted instance):
    *   Follow instructions in [llm-chat-engine/README.md](llm-chat-engine/README.md) to run locally using Docker.
2.  **Setup the VR App**:
    *   Open `XR2IND-VR` in Unity 2022.3.19f1.
    *   Open the `Assets/Scenes/ScenarioSelect` scene.
3.  **Connect & Play**:
    *   Connect your Quest 3 via Air Link or Cable.
    *   Press **Play** in the Unity Editor.

## 📂 Resources & Downloads

Explore the project further with these resources:

### Click on play button see Demo:

<video src="DemoPrismVIdeo.mp4" controls="controls" style="max-width: 100%;">
  Your browser does not support the video tag.
</video>

[![Presentation](https://img.shields.io/badge/📄_View-Presentation_PDF-00599E?style=for-the-badge)](iMMERSIVIEW.pdf)
[![Prism Report](https://img.shields.io/badge/📄_View-Project_Report-FF8C00?style=for-the-badge)](PrismReport.pdf)
[![AI Disclosure](https://img.shields.io/badge/📄_View-AI_Disclosure-800080?style=for-the-badge)](AI_Disclosure_Final.pdf)
[![APK](https://img.shields.io/badge/📱_Download-Installable_APK-3DDC84?style=for-the-badge)](SamsungPrismFInal.apk)

> Click the tags above to view or download the files.

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](XR2IND-VR/LICENSE) file for details.
