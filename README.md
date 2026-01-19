# XR2IND - Industrial VR Training Platform

A comprehensive VR-based industrial training platform that combines immersive virtual reality environments with AI-powered language model assistance for enhanced training experiences.

## Project Structure

```
├── XR2IND-VR/          # Unity VR Application
│   ├── Assets/         # Unity assets, scripts, and scenes
│   ├── Packages/       # Unity package dependencies
│   └── ProjectSettings/# Unity project configuration
│
└── llm-chat-engine/    # AI Backend Service
    ├── app/            # FastAPI application
    ├── Dockerfile      # Container configuration
    └── docker-compose.yml
```

## Components

### XR2IND-VR (Unity Application)
A Unity-based VR application for industrial training featuring:
- **Tutorial Room**: VR controls familiarization for first-time users
- **Assembly Room**: Hands-on router assembly training
- **Troubleshooting Room**: Virtual routing room maintenance scenarios

Interactive 3D models include:
- Cisco ME4924
- Juniper EX9204
- Juniper MX480

### LLM Chat Engine (AI Backend)
A FastAPI-based chatbot service providing:
- Natural language understanding for maintenance queries
- Document upload and RAG-based retrieval
- Real-time streaming responses
- Context-aware conversations

## Requirements

### VR Application
- Unity 2022.3.19f1
- Meta Quest 3 headset
- Meta Quest Link

### AI Backend
- Docker & Docker Compose
- OpenAI API key
- Python 3.11+

## Quick Start

### 1. Deploy the AI Backend
```bash
cd llm-chat-engine
# Set your OpenAI API key in docker-compose.yml
docker-compose up
```

### 2. Run the VR Application
1. Open the project in Unity Hub
2. Connect Meta Quest 3 via USB 3.0
3. Enable Quest Link on the headset
4. Open `Assets/Scenes/ScenarioSelect` scene
5. Press Play in Unity Editor

## Documentation

- [VR Application Details](XR2IND-VR/README.md)
- [AI Backend API](llm-chat-engine/README.md)

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](XR2IND-VR/LICENSE) file for details.

## Status

🚧 **Work in Progress** - This project is currently under active development.
