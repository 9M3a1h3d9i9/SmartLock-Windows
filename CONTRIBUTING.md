# Contributing to SmartLock-Windows

## Development principles

1. Keep security boundaries explicit.
2. Prefer small, reviewable commits.
3. Add or update tests for behavior changes.
4. Keep Core independent from WPF and Windows-specific infrastructure.
5. Do not commit credentials, tokens, private keys, or personal data.

## Commit style

Use conventional, imperative commit messages such as:

- `feat: add session policy contract`
- `fix: handle closed session state`
- `test: cover security event validation`
- `docs: document authentication boundary`

## Pull requests

Every pull request should explain the behavior changed, test coverage, and any security or privacy implications.
