# Security Policy

## Scope

SmartLock is being developed as a Windows desktop application. During development it must not replace, intercept, or bypass the operating system's real authentication boundary.

## Security principles

- Never store or transmit Windows passwords, PINs, tokens, or biometric templates.
- Never capture a webcam image without explicit, user-visible consent and a clear product purpose.
- Prefer documented Windows APIs over undocumented authentication hooks.
- Keep security-sensitive operations isolated behind explicit interfaces.
- Treat telemetry as opt-in and minimize collected data.

## Reporting a vulnerability

Please do not publish sensitive exploit details in a public issue. Use a private security disclosure channel associated with the repository owner when one is configured. Until then, open a minimal issue containing only enough information to establish that a security problem exists.
