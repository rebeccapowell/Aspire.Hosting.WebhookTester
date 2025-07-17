# .codex/setup.md

## Install .NET 9

Codex and other AI agents should always use the following to install .NET 9:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
```

Do not use apt-get, brew, or SDK installers directly.

## Add new projects

When creating a new project, use the `dotnet new` command and then add it to the
solution with `dotnet sln`:

```bash
dotnet new <template> -n <ProjectName> -o <OutputDirectory>
dotnet sln <YourSolution>.sln add <OutputDirectory>/<ProjectName>.csproj
```

## NuGet

- Prefer command line additions to manual editing of csproj files.
- Avoid pre-release versions if possible
- Keep versions consistent across projects (solution level)

## Coding

- .NET9 preferred
- Use global namespaces
- prefer `var`
- File scoped namespace declarations

## Testing
- Do not run integration tests in Aspire.Hosting.WebhooksTester.Tests (temp ignore)
- The integration tests for the AppHost require docker support, which Codex does not support
- Builder related tests can be run

## Notes
- This is currently targeting Aspire 9.3.0.
- Keep readme file up to date
