# Workleap.Extensions.MediatR

Workleap.Extensions.MediatR is a .NET library that provides MediatR extensions, behaviors, and Roslyn analyzers for CQRS conventions. The library targets netstandard2.0 and net8.0 with additional ApplicationInsights integration.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

- **Install Required .NET SDK**: Download and install .NET 9.0.304 exactly as specified in global.json:
  - `wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && chmod +x /tmp/dotnet-install.sh`
  - `/tmp/dotnet-install.sh --version 9.0.304 --install-dir ~/.dotnet`
  - `export PATH="$HOME/.dotnet:$PATH"`
- **Install .NET 8.0 Runtime** (required for running tests):
  - `/tmp/dotnet-install.sh --version 8.0.8 --runtime dotnet --install-dir ~/.dotnet`
- **Build the project**:
  - Navigate to `src/` directory: `cd src`
  - First restore: `dotnet restore` -- takes 50 seconds initially, 2 seconds subsequently. NEVER CANCEL. Set timeout to 10+ minutes.
  - Debug build: `dotnet build -c Debug` -- takes 9 seconds after restore. NEVER CANCEL. Set timeout to 30+ minutes.
  - Release build may fail with GitVersion issues in non-CI environments. Use Debug build for development.
- **Run tests**:
  - `dotnet test -c Debug --no-build --verbosity normal` -- takes ~8 seconds if already built, ~17 seconds if a build is required. NEVER CANCEL. Set timeout to 30+ minutes.
  - All 86 tests should pass when environment is properly configured.
- **Alternative: Use PowerShell build script** (may fail with GitVersion in non-CI):
  - `pwsh ./Build.ps1` -- runs clean, build, test, pack sequence

## Validation

- **ALWAYS run through complete build and test cycle** after making changes to validate functionality.
- **ALWAYS run both main library tests and analyzer tests** to ensure Roslyn analyzers work correctly.
- **ALWAYS test MediatR extension functionality** by checking that the library properly registers with dependency injection.
- **NEVER make changes to PublicAPI.Shipped.txt** without understanding the breaking change implications.
- Add new public APIs to `PublicAPI.Unshipped.txt` files in the respective project directories.
- **Always check that all tests pass** - the test suite includes comprehensive validation of MediatR behaviors, analyzers, and integration scenarios.
- **Validation Scenarios**: After making changes, run these end-to-end validation steps:
  1. `cd src && dotnet clean && dotnet restore && dotnet build -c Debug`
  2. `dotnet test -c Debug --no-build --verbosity normal` 
  3. Verify all 86 tests pass
  4. Check that no new analyzer warnings are introduced
  5. Ensure PublicAPI files are updated if needed

## Common Tasks

The following are outputs from frequently run commands. Reference them instead of viewing, searching, or running bash commands to save time.

### Repository Structure
```
.
├── .github/                    # GitHub workflows and templates
├── src/                        # All source code
│   ├── Workleap.Extensions.MediatR/                    # Main library
│   ├── Workleap.Extensions.MediatR.ApplicationInsights/ # ApplicationInsights integration
│   ├── Workleap.Extensions.MediatR.Analyzers/          # Roslyn analyzers
│   ├── Workleap.Extensions.MediatR.Tests/              # Main library tests
│   ├── Workleap.Extensions.MediatR.Analyzers.Tests/    # Analyzer tests
│   └── Workleap.Extensions.MediatR.sln                 # Solution file
├── Build.ps1                   # Main build script
├── README.md                   # Project documentation
├── global.json                 # .NET SDK version requirement (9.0.304)
└── Directory.Build.props       # Shared MSBuild properties
```

### Key Project Components

1. **Main Library** (`Workleap.Extensions.MediatR`):
   - Multi-targets: netstandard2.0, net8.0
   - Provides MediatR extensions and behaviors
   - Includes activity-based OpenTelemetry instrumentation
   - High-performance logging with Debug level
   - Data annotations support for request validation

2. **ApplicationInsights Integration** (`Workleap.Extensions.MediatR.ApplicationInsights`):
   - Separate NuGet package for Application Insights instrumentation
   - Multi-targets: netstandard2.0, net8.0

3. **Roslyn Analyzers** (`Workleap.Extensions.MediatR.Analyzers`):
   - Enforces CQRS naming conventions (GMDTR01-GMDTR13 rules)
   - Embedded into main package during build
   - Validates handler design patterns

### Common Build Commands
```bash
# First restore (one-time setup - takes ~50 seconds)
cd src && dotnet restore

# Subsequent restores (takes ~2 seconds) 
cd src && dotnet restore

# Debug build (development recommended - takes ~9 seconds)
cd src && dotnet build -c Debug

# Run all tests (takes ~8 seconds if already built, ~17 seconds with build)
cd src && dotnet test -c Debug --no-build --verbosity normal

# Run tests with automatic build (if you want to skip separate build step)
cd src && dotnet test -c Debug --verbosity normal

# Clean and rebuild (for troubleshooting)
cd src && dotnet clean && dotnet build -c Debug

# Check .NET version
dotnet --version
```

### PublicAPI Files
- `PublicAPI.Shipped.txt` - Contains all shipped public APIs (DO NOT MODIFY)
- `PublicAPI.Unshipped.txt` - Add new public APIs here before shipping

### Analyzer Rules (GMDTR01-GMDTR13)
- GMDTR01-GMDTR06: Naming conventions for commands, queries, handlers, notifications
- GMDTR07-GMDTR13: Design patterns and best practices

## Important Notes

- **Release builds require GitVersion** which only works in CI environments with proper git context
- **Use Debug builds for local development** to avoid GitVersion issues  
- **PowerShell 7.4+ is available** for running Build.ps1 if needed
- **Assembly signing is enabled** with Workleap.Extensions.MediatR.snk
- **InternalsVisibleTo** is configured between projects for testing
- **CI/CD is fully automated** - preview packages on main branch commits, stable releases on tags

## Project Dependencies

Key NuGet packages:
- MediatR 12.5.0
- Microsoft.Extensions.Logging.Abstractions 8.0.0
- Microsoft.CodeAnalysis.PublicApiAnalyzers 3.3.4
- Workleap.DotNet.CodingStandards 1.1.3 (for style and analysis)

## Troubleshooting

- **"GitVersion failed"**: Use Debug build configuration instead of Release
- **"Framework not found"**: Install .NET 8.0 runtime for test execution
- **"Assembly not found"**: Run `dotnet restore` in src/ directory first
- **Tests failing**: Ensure both .NET 9.0.304 SDK and 8.0.8 runtime are installed
