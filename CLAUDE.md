# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 LTS (6000.0.42f1) number tracing game for toddlers using the Universal Render Pipeline (URP) and UIElements/UIToolkit for UI.

## Commands

### Unity Development
- **Open Project**: Launch Unity Hub and open the project with Unity 6000.0.42f1
- **Play Mode**: Press Play button in Unity Editor to test
- **Build**: File > Build Settings in Unity Editor
- **Run Tests**: Window > General > Test Runner (Unity Test Framework 1.4.6 is included)

### Unity Editor Integration
Since this is a Unity project, most operations are done through the Unity Editor rather than command line. The project includes UMCP (Unity MCP) integration for programmatic control.

## Architecture

### Technology Stack
- **Engine**: Unity 6 LTS with Universal Render Pipeline
- **UI Framework**: UIElements/UIToolkit (NOT IMGUI)
- **Input**: Unity Input System (new, not legacy)
- **Namespace**: PNTG (for all project scripts)
- **Database**: Chroma vector database for development logging

### Project Structure
```
Assets/PNTG/                    # Main project folder
├── scripts/runtime/            # Runtime scripts with assembly definition
├── scripts/editor/             # Editor-only scripts with assembly definition  
├── scenes/                     # Game scenes
├── prefabs/                    # Prefabs
├── materials/                  # Materials
├── textures/                   # Textures
├── UIElements/                 # UIToolkit assets (UXML, USS)
└── ScriptableObjects/          # Number/shape configuration data
```

### Core Architecture Components
1. **Stroke System**: 2D NURBS curves with wobble radius tolerance
2. **UI System**: UIElements Painter2D for stroke visualization
3. **Game Loop**: ScriptableObject-based number configurations
4. **Input Handling**: Touch/mouse tracking with tolerance for toddler input

## Development Guidelines

### Unity-Specific Rules
- Only modify files within Assets/ folder
- Let Unity create .meta files automatically
- Use UMCP ForceUpdateEditor after file changes
- Mark data classes with [Serializable] for persistence
- Use [SerializeReference] for polymorphic fields
- Create ScriptableObjects with [CreateAssetMenu] attribute

### Code Organization
- Apply functional programming: separate data structs from logic
- Create static utility classes for processing logic
- Write descriptive XML summaries for all public members
- Keep assembly definitions in script folders

### When Working on Tasks
1. Check Chroma database configuration is local to project
2. Create/use ProjectDevelopmentLog and issue-specific databases
3. Use UMCP tools to mark step progress
4. Run ForceUpdateEditor after changes
5. Check for compilation errors with RequestStepLogs
6. Run unit tests before completing work

## Key Implementation Details

### Stroke Representation
- Use UIElements Painter2D for rendering
- Implement 2D NURBS curves for smooth strokes
- Support runtime control point editing in play mode
- Visualize wobble radius as filled outline

### Number Data
Reference images in Claude_Prompts/Numbers/ show:
- Stroke count and order (numbered arrows)
- Stroke directionality (arrow direction)  
- Proper letter formation for numbers 0-3

### Game Loop
1. Display number background
2. Show stroke guidance overlay
3. Track finger/mouse with wobble tolerance
4. Validate stroke completion
5. Progress through multi-stroke numbers
6. Award points and load next number