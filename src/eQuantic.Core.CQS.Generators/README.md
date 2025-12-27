# eQuantic.Core.CQS.Generators

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Generators.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Generators/)

Source generators for eQuantic.Core.CQS - Auto-registration of handlers and compile-time validation.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Generators
```

## Features

### Auto Handler Registration (Coming Soon)

Automatically generates handler registration code at compile time.

### Compile-Time Validation (Coming Soon)

Validates that all commands and queries have corresponding handlers.

## Current Status

🚧 **Under Development**

This package is being actively developed. Current features:

- Basic source generator infrastructure
- Roslyn analyzer framework

## Roadmap

- [ ] Auto-generate `AddCQS()` registration
- [ ] Validate handler existence at compile time
- [ ] Generate handler stubs from commands/queries
- [ ] Performance optimizations with compiled expressions

## License

MIT License
