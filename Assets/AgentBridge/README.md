# AgentBridge for Unity

AgentBridge is a provider-agnostic AI integration layer for Unity that utilizes the Model Context Protocol (MCP) to connect Unity with AI agents (such as Antigravity, OpenAI, or local LLMs). 

The system does NOT depend on any specific AI provider, ensuring no vendor lock-in. It relies on strongly typed systems, safe execution (validation, undo support), and focuses on a native Unity Editor UX rather than a chatbot-first design constraints.

## Features

- **AI-agnostic & Modular**: No vendor lock-in. Switch between different LLMs or even local models.
- **MCP-first Communication**: Uses structured MCP tool calls rather than free-text prompts for better reliability.
- **Context Detection**: Automatically detects selected GameObjects, Textures, Audio, or Scripts and adapts available actions dynamically.
- **Safe Execution**: All AI-generated commands run inside Unity with built-in validation, execution whitelists, and full Undo support.
- **Native Editor UX**: Clean, familiar Unity Editor Window and Context Menu integrations.
- **Project-wide Analysis**: AI Analyze and Improve mode with diff/previews before applying changes.

## Installation via Git URL

The package can be installed via Unity Package Manager (UPM). 

1. Open your Unity Project.
2. Go to **Window > Package Manager**.
3. Click the **+** (plus) button in the upper left corner and select **"Add package from git URL..."**
4. Paste the repository URL (e.g., `https://github.com/soueuroya/AgentBridge.git?path=/Assets/AgentBridge`) and click **Add**.

*Note: Adjust the path variable if the package is moved to the repository root.*

## Basic Usage

Detailed usage will be added as features are merged. For now, the framework provides core architecture interfaces:
- Implement `IAgentProvider` for your chosen LLM.
- Use `IMcpClient` and `IMcpServer` for protocol communication.
- Define context with `IActionContext`.

*Look out for the right-click "AI Actions" contextual menu in upcoming releases!*

## Contribution Guidelines

1. **Fork & Branch**: Create your feature branch from `main`.
2. **Follow the Architecture**: Ensure changes use interfaces and abstraction layers natively. Do not introduce hard dependencies on specific AI vendors or Unity AI packages.
3. **Commit & PR**: Provide clear, descriptive commit messages and open a Pull Request.

## License

This project is licensed under the MIT License - see the `LICENSE.md` file for details.
