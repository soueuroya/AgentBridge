# AgentBridge for Unity

**AgentBridge** is a highly modular, provider-agnostic native AI integration layer built exclusively for Unity 2021+. It leverages the official **Model Context Protocol (MCP)** to allow secure, type-safe interactions between complex AI models (Anthropic, OpenAI, Local LLMs) and the internal Unity Editor structures.

By entirely avoiding free-text prompt injections and chatbot constraints, AgentBridge introduces native **AI-Action Hubs**, dynamic **Context Resolvers**, structured **Capability Registries**, and rigidly protected **Undo Pipeline Executions**, ensuring that AI functions dynamically yet safely directly as a native Unity subsystem.

## Core Feature Overview

- **Provider-Agnostic Extensibility**: Connect strictly to any web API or proxy utilizing the `IAiProvider` base contract. Hard vendor lock-in is completely eliminated.
- **Model Context Protocol (MCP)**: Communicates exactly by employing rigid nested JSON dictionaries, forcing providers into strict tool-driven executions to prevent formatting hallucinations.
- **Dynamic Context Parsing**: When you click a GameObject, AgentBridge inherently detects its components, transforming structural metadata seamlessly into AI readable context automatically.
- **Safe Command Executions**: Incorporates visual 'Before/After' visual diffing and forces all external payloads through explicit internal Unity tracking (`Undo.RecordObject`).
- **Multithreading Batches**: Multi-select hundreds of unique assets—the builder handles grouping payloads intelligently decreasing token thresholds severely.

## Installation via UPM

The package can be installed easily via Unity's Package Manager (UPM).

1. Open your Unity Project.
2. Go to **Window > Package Manager**.
3. Click the **+** (plus) button in the upper left corner and select **"Add package from git URL..."**
4. Paste the repository URL: `https://github.com/soueuroya/AgentBridge.git?path=/Assets/AgentBridge`
5. Press **Add** and let Unity compile assemblies.

## Basic Developer Usage

1. Open **Window > AgentBridge > AI Action Hub**.
2. Right click the Project window -> **Create > AgentBridge** to make your `CommandExecutionEngine` and `CapabilityRegistry`.
3. Simply start clicking GameObjects or Textures to watch context bindings trigger in real-time. Action buttons will be filtered natively allowing specific executions per object type.
4. Try highlighting massive groups of arrays, and run **Window > AgentBridge > Batch Processor** for holistic operations!

## Comprehensive Documentation

For dedicated system onboarding, read our formal architecture mappings:
- [Core Architecture Manual](Documentation~/ARCHITECTURE.md)
- [MCP Format & Data Specifics](Documentation~/MCP_SPEC.md)

## Contribution Guidelines

1. **Fork & Branch**: Create branch layouts exclusively mapped starting from `main`.
2. **Follow Structural Integrities**: All mutations MUST flow through the strict interfaces inside. Do not introduce hard dependencies on specific web models. If an execution mutates the engine, force it to declare `IsDangerous => true` in the codebase.
3. **Commit PRs**: Map discrete changes to testable scenes and PR.
