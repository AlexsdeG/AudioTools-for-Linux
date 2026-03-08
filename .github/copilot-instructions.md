# Copilot Routing Instructions

## Repository Stack
- Language: C#
- Runtime/SDK: .NET 8 (`net8.0`)
- App Type: Linux desktop GUI (GtkSharp)
- Primary entry/source file: `Audio Tools.cs`
- Project file: `AudioTools.csproj`
- Shell automation: `Run This To Build.sh`, `setup/*.sh`
- Package manager: NuGet via `dotnet` CLI (`dotnet restore`, `dotnet add package`)

## Working Rules
- Keep the app compatible with Ubuntu/Debian-based Linux workflows documented in `README.md`.
- Preserve `Nullable` context and avoid introducing nullable warnings.
- Use clear command construction when shelling out through `ProcessStartInfo`.
- Do not hardcode distro-specific paths beyond existing project conventions unless explicitly required.
- Keep UI logic and process execution logic separated when adding new features.

## Coding Standards
- Follow standard C#/.NET style conventions:
  - PascalCase for types/methods/properties.
  - camelCase for local variables and parameters.
  - Prefer `var` only when the type is obvious.
  - Keep methods focused and small.
- Treat warnings as defects when possible; do not add code that introduces new warnings.
- Prefer explicit error handling for external command execution.
- Keep scripts POSIX/Bash-safe and idempotent where practical.

## Verification Commands
Run these commands from repository root after changes:

```bash
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
bash "setup/requirements test.sh"
```

## Test and Lint Status
- Current repository has no dedicated unit test project.
- Current repository has no dedicated lint configuration.
- Minimum verification is successful build/publish plus requirement script checks.
