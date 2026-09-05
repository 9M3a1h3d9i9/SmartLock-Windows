# SmartLock-Windows

> **Smart Windows lock-screen experience — designed for intelligent security, privacy, and usability.**

## Project Identity

**صاحب اثر:** MahdiCS313  
**هدف:** توسعه هوشمند و امنیت فراگیر (Intelligent Development & Inclusive Security)

SmartLock-Windows is a portfolio-grade Windows desktop application project focused on building a polished, privacy-conscious lock-screen experience without bypassing or replacing Windows authentication mechanisms.

## Current Milestone — V0.2

V0.2 begins the professional UI layer:

- C# / .NET 10 / WPF application shell
- Dedicated `SmartLock.UI` presentation project
- Reusable WPF theme resources and design tokens
- Professional dark lock-screen layout with surface hierarchy and status indicator
- Explicit `DEVELOPMENT MODE` labeling
- `LockScreenViewModel` for testable presentation behavior
- Dedicated UI unit tests
- Security-event behavior remains isolated behind the Core service contract

### Run locally

Prerequisite: Windows with the .NET 10 SDK installed.

```powershell
dotnet restore SmartLock.sln
dotnet build SmartLock.sln --configuration Release
dotnet test SmartLock.sln --configuration Release
```

To run the development application:

```powershell
dotnet run --project src/SmartLock.App/SmartLock.App.csproj
```

The current development shell intentionally does **not** authenticate against Windows. `Ctrl+E` closes the development window. Real Windows integration is reserved for later milestones and will use documented operating-system APIs.

## Vision

SmartLock aims to combine:

- Modern Windows-native UX
- Secure authentication boundaries
- Intelligent session awareness
- Security-event visualization
- Privacy-first design
- Accessibility and localization
- Testable, maintainable architecture
- Professional packaging and release engineering

## Engineering Principles

1. **Security first** — never collect, expose, or bypass Windows credentials.
2. **Privacy by design** — camera, biometric, network, and telemetry features require explicit user consent.
3. **Native integration** — use supported Windows APIs rather than undocumented authentication hooks.
4. **Separation of concerns** — UI, domain logic, security, and infrastructure remain independently testable.
5. **Professional delivery** — CI, automated tests, documentation, versioning, and reproducible builds are first-class requirements.

## Architecture

```text
SmartLock-Windows/
├── src/
│   ├── SmartLock.App/              # WPF application/composition root
│   ├── SmartLock.Core/             # Domain models and contracts
│   ├── SmartLock.Security/         # Future security policies/adapters
│   ├── SmartLock.UI/               # WPF theme + view models
│   └── SmartLock.Infrastructure/   # Future OS/storage/network adapters
├── tests/
│   ├── SmartLock.Core.Tests/       # Core unit tests
│   └── SmartLock.UI.Tests/         # UI/view-model unit tests
├── docs/
│   ├── ARCHITECTURE.md
│   └── V0.2.md
├── assets/
├── installer/
└── .github/workflows/ci.yml
```

## Roadmap

- [x] V0.1 — Native application foundation
- [x] V0.2 — Professional lock-screen UX and authentication UI foundation
- [ ] V0.3 — Security event engine
- [ ] V0.4 — Intelligent session/context awareness
- [ ] V0.5 — Windows integration
- [ ] V0.6 — Accessibility and localization
- [ ] V0.7 — Security hardening and performance
- [ ] V0.8 — Installer and packaging
- [ ] V1.0 — Public release

## Security

See [`SECURITY.md`](SECURITY.md). The project explicitly avoids credential harvesting, covert camera capture, and undocumented authentication interception.

## Attribution

**Copyright © 2026 MahdiCS313. All rights reserved to the project owner, subject to the repository license.**

Developed by **MahdiCS313**, with AI engineering assistance from **ChatGPT**.

ChatGPT is credited as an AI engineering assistant and is not represented as a legal co-owner or rights holder.

## License

This project is released under the MIT License. See [`LICENSE`](LICENSE).
