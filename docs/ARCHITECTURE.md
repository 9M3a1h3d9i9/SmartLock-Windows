# SmartLock Architecture

## V0.1 goal

V0.1 establishes a buildable native Windows foundation without attempting to replace Windows authentication.

```text
SmartLock.App
    |
    +--> SmartLock.Core
    |
    +--> SmartLock.Infrastructure (future)
    |
    +--> SmartLock.Security (future)
    |
    +--> SmartLock.UI (future)
```

### Boundaries

- **App** owns composition and the desktop entry point.
- **Core** contains domain models and contracts and must remain UI-independent.
- **Infrastructure** will contain OS/file/network adapters.
- **Security** will contain authentication and security policies using supported Windows APIs.
- **UI** will contain reusable presentation components and view models.

## Security boundary

SmartLock must never collect, store, transmit, or attempt to bypass Windows credentials. Future authentication work must use documented Windows mechanisms and explicit user consent where applicable.

## V0.1 acceptance criteria

- Solution contains a native WPF application and isolated domain project.
- Domain contracts are unit tested.
- CI restores, builds, and tests the solution on Windows.
- Development exit shortcut is explicit and remains available while the application is not a real Windows lock-screen replacement.
- No secrets or credentials are committed to source control.
