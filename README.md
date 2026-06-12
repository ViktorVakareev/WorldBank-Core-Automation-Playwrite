📖 **Browse the internal engineering documentation at [worldbank.wiki.local/automation-framework**](https://www.google.com/search?q=%23)

**Next-generation, AI-augmented UI automation architecture.**
From immutable DevSecOps infrastructure to deterministic, component-driven execution.

---

## 🚀 Overview

The **WorldBank Core Automation** project is an enterprise-grade testing framework designed to validate the WorldBank Mock Web Application.

Traditional Page Object Models (POM) become brittle at scale. This framework abandons legacy OOP inheritance in favor of a modern **Component-Based UI Architecture** paired with **Stateless Action Extensions**. Engineered specifically for CI/CD, the framework includes an immutable Dockerized execution environment to prevent shared-memory deadlocks, alongside an integrated local Large Language Model (LLM) that automatically intercepts and triages pipeline failures.

## ✨ Core Features

* **Stateless Action Extensions:** Test scripts read like plain English. Heavy business logic is abstracted into C# extension methods mapped directly to Playwright's `IPage` interface.
* **Component-Based DOM Fragments:** UI elements are isolated into reusable, atomic components rather than monolithic page classes, completely eliminating locator rot cascades.
* **AI-Augmented Failure Triage:** Native integration with a local **Llama 3** instance. When a test fails, the framework automatically queries the AI with the stack trace, DOM state, and error message to categorize the failure (e.g., *Application Defect* vs. *Locator Rot*) and attaches the Markdown analysis directly to the test report.
* **Immutable DevSecOps Infrastructure:** A pre-baked `Dockerfile.agent` handles Chromium dependencies and OS-level fonts, ensuring zero environment drift between local execution and Jenkins pipeline runs.
* **Dynamic Data Fuzzing:** Integration with Bogus generates production-like, randomized financial data (IBANs, SWIFT codes, recipient names) to thoroughly fuzz validation gateways.
* **Enterprise Telemetry:** Deep, seamless integration with both **ReportPortal** and **Allure**, guaranteeing thread-safe attachment of screenshots, `.webm` execution videos, and AI diagnostics.

## 🛠️ Technology Stack

| Category | Technology |
| --- | --- |
| **Language & Runtime** | C# 12, .NET 10.0 |
| **Test Runner** | NUnit 4 |
| **Browser Engine** | Microsoft Playwright |
| **AI Intelligence** | Ollama (Llama 3 Model) |
| **Data Generation** | Bogus |
| **Reporting** | ReportPortal, Allure |
| **CI/CD** | Jenkins, Docker |

## 📂 Architecture Blueprint

The repository enforces strict separation of concerns. Tests never touch raw locators; configurations never touch business logic.

```text
WorldBank.Automation.Solution/
├── Jenkinsfile                      # CI/CD Pipeline Configuration
├── Dockerfile.agent                 # Immutable Jenkins Agent Definition
└── src/Automation.Framework/
    ├── local.runsettings            # Local execution configurations
    ├── playwright.runsettings       # Specific Playwright execution profiles
    ├── ReportPortal.config.json     # Telemetry routing
    ├── allureConfig.json            # Artifact formatting
    ├── 📁 Infrastructure/           # DEVSECOPS & ENGINE LAYER
    │   ├── AiTriage.cs              # Playwright base + LLM integration
    │   ├── AppConfig.cs             # Environment resolution
    │   └── GlobalSetup.cs           # Assembly-level hooks
    ├── 📁 Components/               # UI FRAGMENTS
    │   ├── BaseComponent.cs         # Core component locator logic
    │   ├── GlobalNavigationComponent.cs 
    │   ├── SandboxToggleComponent.cs # Environment toggling logic
    │   └── TransferStepperComponent.cs 
    ├── 📁 Actions/                  # STATELESS BUSINESS LOGIC
    │   ├── AuthActions.cs           
    │   └── TransferActions.cs       
    ├── 📁 Data/                     # TEST DATA MANAGEMENT
    │   ├── DataFactory.cs           
    │   └── StaticTestUsers.json     # Hardcoded fallback credentials/states
    └── 📁 Tests/                    # DECLARATIVE TEST SUITES
        ├── AccessibilityTests.cs    # A11y compliance verifications
        ├── LoginScenariosTests.cs   
        ├── NavigationTests.cs       
        ├── NetworkInterceptionTests.cs # API mocking and response validation
        ├── SecurityScenariosTests.cs 
        ├── VisualRegressionTests.cs # Pixel-perfect snapshot comparisons
        ├── WorldBankDataDrivenTests.cs 
        └── WorldBankFunctionalTests.cs

```
🔒 DevSecOps & Zero-Trust Integration
Security is shift-left and integrated directly into the testing lifecycle.

Zero-Trust Execution: The Playwright browser runs with aggressive memory and security flags (--disable-dev-shm-usage, --no-sandbox) in CI environments.

Secret Vaulting: Passwords, API Keys, and UUIDs are strictly segregated from the codebase. GitHub Repository Secrets (${{ secrets.RP_API_KEY }}) and Jenkins Credential Vaults inject sensitive data directly into the runner's ephemeral environment variables at runtime.

Trace Isolation: Playwright traces and video recordings are structurally isolated using WorkerId paths to prevent parallel threads from locking or corrupting each other's I/O streams.

📦 Execution Artifacts & Telemetry (The Black Box)
Because our GitHub Actions runners are ephemeral, the environment and its file system are completely destroyed the moment a pipeline finishes. To ensure zero loss of debugging context, the framework acts as a "black box flight recorder," aggressively capturing state upon any test failure.

Using recursive glob patterns (/TestResults//*.[ext]) in our CI pipeline, we extract a comprehensive suite of telemetry files. When a developer downloads the test-results-and-traces.zip artifact from a failed build, it contains the following DevSecOps payloads:

1. Developer Experience (DX) Tooling
*.playlist (Visual Studio Test Playlist)

What it is: A dynamically generated XML file containing the exact list of tests that failed in the cloud.

Why it matters: Ultimate developer velocity. Instead of manually reading logs and hunting for failed tests in the IDE, a developer simply double-clicks this file. Visual Studio automatically opens, filters the Test Explorer, and queues up only the broken tests for local debugging.

[📸 INSERT SCREENSHOT: Visual Studio Test Explorer filtered by the playlist file]

2. AI & Execution Logs
*.md (Llama 3 Root Cause Analysis)

What it is: A localized, AI-generated Markdown file created the millisecond the test fails.

Why it matters: Drastically reduces Mean Time To Resolution (MTTR). The local AI instance parses the Playwright exception and the C# stack trace, outputting a concise, 3-bullet-point summary of the failure (e.g., "Locator Rot," "Timeout," or "Application Defect") attached directly to the test report.

[📸 INSERT SCREENSHOT: An opened AI_Analysis...md file showing the 3 bullet points]

*.trx (VSTest Results File)

What it is: The official XML execution log generated by the .NET NUnit engine.

Why it matters: This is the authoritative record of the test run. Because parallel test runners suppress console outputs to prevent jumbled logs, the .trx file safely stores all hidden [WARNING] tags, AI connection telemetry, and exact execution durations.

3. Visual & State Telemetry
*.zip (Playwright DOM Trace)

What it is: A complete, portable archive of the browser's state during the test.

Why it matters: Time-travel debugging. By dragging this zip file into trace.playwright.dev, engineers can step through the test millisecond by millisecond. It captures the raw DOM, network requests, console logs, and action timings, completely eliminating the "it works on my machine" argument.

[📸 INSERT SCREENSHOT: The Playwright Trace Viewer showing a timeline and network requests]

*.png (Full-Page Screenshot)

What it is: A high-resolution capture of the entire web page layout at the exact moment the exception was thrown.

Why it matters: Provides immediate visual confirmation of the failure state. It instantly reveals if a loading spinner froze, if an obscuring modal blocked a click, or if a CSS update broke the layout.

*.webm (Execution Video)

What it is: A lightweight video recording of the browser session.

Why it matters: Contextualizes the steps leading up to the failure. While traces and screenshots show the final state, the video is critical for diagnosing dynamic timing issues, visual jitter, or unexpected animations that confused the automated waits.

[📸 INSERT SCREENSHOT: A frame of the .webm video playing or attached in Allure/ReportPortal]

## 💻 Getting Started

### Prerequisites

* [.NET 10.0 SDK](https://www.google.com/search?q=https://dotnet.microsoft.com/download/dotnet/10.0)
* [Ollama](https://www.google.com/search?q=https://ollama.com/) (For local AI Triage capabilities)
* Docker Desktop (For local infrastructure testing)

### Local Setup

1. **Clone the repository:**
```bash

```
git clone https://github.com/ViktorVakareev/WorldBank-Core-Automation.git
cd WorldBank-Core-Automation

```
2.  **Restore dependencies and install Playwright browsers:**
    ```bash
dotnet build src/
pwsh src/Automation.Framework/bin/Debug/net10.0/playwright.ps1 install

```

3. **Start the local AI engine (Optional but recommended):**
```bash

```



ollama run llama3

```
4.  **Execute the suite:**
    ```bash
dotnet test src/ --settings src/Automation.Framework/local.runsettings

```
## Immutable framework
I designed the framework configuration to be immutable in source control but highly elastic at runtime. A developer can pull the repo, create an appsettings.local.json, turn Headless to false, crank up the SlowMo to 1000ms, and test against a local Docker database. Meanwhile, the GitHub Actions pipeline ignores all of that, runs Headless: true at lightning speed, and can seamlessly pivot from testing the Sandbox environment to the Staging environment just by us passing Framework__BaseUrl=https://staging.worldbank... as a GitHub secret, without changing a single line of code.

## 🧠 Writing Modern Tests

Tests in this framework are completely declarative. By combining `IPage` extensions with isolated components, we achieve highly readable, highly maintainable tests that require zero object instantiation boilerplate.

```csharp
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class WorldBankTransferTests : AiTriage
{
    [Test]
    public async Task WireTransfer_ValidNavigation_ShouldLoadForm()
    {
        // 1. Arrange: Stateless authentication action
        await Page.LoginToWorldBankAsync("standard_user", "password123");
        
        // 2. Act: Modular component interaction
        var navigation = new GlobalNavigationComponent(Page);
        await navigation.NavigateToAsync("wire transfer");

        // 3. Assert: Native Playwright expectations
        await Expect(Page).ToHaveURLAsync(new Regex(".*transfer\\.html"));
    }
}

```

## 🛡️ DevSecOps & CI/CD Integration

This project is built to run in hostile CI environments without memory throttling.

The `Jenkinsfile` orchestrates execution by pulling the `Dockerfile.agent`, which pre-bakes all Linux Chromium dependencies. The `AiTriage.cs` base class actively overrides the Playwright context to bypass Docker's restrictive 64MB `/dev/shm` shared memory limit via the `--disable-dev-shm-usage` flag, allowing for high-concurrency parallel execution (`NUnit.NumberOfTestWorkers=4+`) without deadlocking the Jenkins CPU.

## 🤝 Contributing

1. Create a feature branch (`git checkout -b feature/amazing-new-action`)
2. Commit your changes (`git commit -m 'feat: added international wire transfer action'`)
3. Push to the branch (`git push origin feature/amazing-new-action`)
4. Open a Pull Request ensuring all tests pass on the PR Jenkins Gate.

---

*Architected for speed, built for scale, engineered for stability.*
