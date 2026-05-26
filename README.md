# StampVer

[![Build and Test StampVer](https://github.com/craigtp/StampVer/actions/workflows/main.yml/badge.svg)](https://github.com/craigtp/StampVer/actions/workflows/main.yml)
[![License: MIT](https://img.shields.io/github/license/craigtp/StampVer)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

A small command-line utility for updating .NET assembly version metadata in bulk. StampVer walks the current directory tree, finds matching files, and rewrites every assembly version it finds - incrementing or decrementing a [Semantic Versioning](https://semver.org/) part, or replacing the version outright. Out of the box it handles both:

- Legacy `AssemblyInfo.cs` files - `[assembly: AssemblyVersion(...)]` and `[assembly: AssemblyFileVersion(...)]` attributes.
- Modern SDK-style `.csproj` files - `<AssemblyVersion>`, `<FileVersion>`, `<Version>`, and `<VersionPrefix>` MSBuild properties.

## Table of contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Examples](#examples)
- [How it works](#how-it-works)
- [Building from source](#building-from-source)
- [Acknowledgements](#acknowledgements)
- [License](#license)

## Features

- **Two source formats supported** out of the box - legacy `AssemblyInfo.cs` attributes and modern SDK-style `.csproj` MSBuild properties.
- **Increment, decrement, or explicitly set** the `MAJOR`, `MINOR`, `PATCH` (or `BUILD`) component of a version number.
- **Semantic cascade** on increment: bumping `MAJOR` resets `MINOR` and `PATCH` to `0`; bumping `MINOR` resets `PATCH` to `0`.
- **Preserves wildcard tokens** like `*` in `AssemblyVersion("1.0.*")` (`AssemblyInfo.cs` only - csproj doesn't support wildcards).
- **Honours conditional element attributes** such as `<Version Condition="...">1.0.0</Version>`.
- **Clamps every part to `[0, 65535]`** so generated versions remain valid .NET assembly metadata.
- **Dry-run mode** previews the changes without writing to disk.
- **Custom starting directory** via `--dir` so you can stamp another repository without `cd`-ing into it.
- **Custom file patterns** for projects that don't fit either default.
- **Self-contained single-file binary** when published - no .NET runtime required on the target machine.
- **Zero external dependencies** at runtime.

## Installation

### Download a pre-built binary

A single-file Windows x64 executable can be produced from source with:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

The output `stampver.exe` is fully self-contained and can be dropped anywhere on `PATH`. The `publish-win-x64.bat` helper script in the repository root invokes the same command.

### Build from source

See [Building from source](#building-from-source) below.

## Usage

```
stampver <command> <version-part-or-number> [--dir <path>] [filepattern] [options]
```

### Commands

| Flag | Meaning |
| --- | --- |
| `-i <part>` | Increment the specified version part by 1. |
| `-d <part>` | Decrement the specified version part by 1. |
| `-e <x.y.z>` | Replace the entire version string with the specified version. |
| `--help` | Display help text and exit. |

The `-i`, `-d`, and `-e` commands are mutually exclusive.

### Version parts

| Value | Meaning |
| --- | --- |
| `MAJOR` | Operate on the major version part. |
| `MINOR` | Operate on the minor version part. |
| `PATCH` | Operate on the patch version part. |
| `BUILD` | Synonym for `PATCH`. |
| `x.y.z` | A literal version (for use with `-e` only). Each part must be an integer in `[0, 65535]`. |

### Options

| Flag | Meaning |
| --- | --- |
| `--dir <path>` (or `--directory <path>`) | Directory to start the recursive search from. Defaults to the current working directory. |
| `--quiet` | Suppress all console output. |
| `--verbose` | Log every file inspected and every change made. |
| `--dryrun` | Show what *would* change without modifying any files. Implies `--verbose`. |

`--quiet` and `--verbose` are mutually exclusive.

### Starting directory

By default StampVer walks the current working directory. Pass `--dir <path>` (alias `--directory`) to start the recursive walk from somewhere else, so you can stamp another repository without `cd`-ing into it:

```bash
stampver -i MINOR --dir "C:\MySourceCode\MyRepo"
```

The search is always recursive through every subdirectory of the starting directory. If the directory doesn't exist, StampVer reports a usage error and makes no changes. `--dir` composes with the file pattern below.

### File pattern

An optional final positional argument specifies which files to scan. Any pattern accepted by [`Directory.EnumerateFiles`](https://learn.microsoft.com/dotnet/api/system.io.directory.enumeratefiles) works (e.g. `*.cs`, `AssemblyInfo.*`, `Directory.Build.props`).

When no positional argument is given, StampVer scans **both** `AssemblyInfo.cs` and `*.csproj` files by default, so a typical SDK-style project works with no configuration. An explicit pattern suppresses the defaults and uses only that pattern.

The file format is detected from the extension: `.csproj` files are treated as MSBuild XML, and everything else is treated as a C# attribute source file. A file is only modified if it contains at least one recognised version entry:

| File extension | Recognised forms |
| --- | --- |
| `.csproj` | `<AssemblyVersion>x.y.z</AssemblyVersion>`, `<FileVersion>x.y.z</FileVersion>`, `<Version>x.y.z</Version>`, `<VersionPrefix>x.y.z</VersionPrefix>` (with optional attributes on the element) |
| Everything else | `[assembly: AssemblyVersion("x.y.z")]`, `[assembly: AssemblyFileVersion("x.y.z")]` |

Comment lines (`//` for C#, `<!--` for XML) are skipped. Multi-line XML block comments are matched line-by-line, so a version element inside a multi-line `<!-- ... -->` block would still be rewritten - in practice nobody comments out version elements, but worth knowing.

## Examples

Increment the minor version in every `AssemblyInfo.cs` under the current directory:

```bash
stampver -i MINOR
```

For files at `1.1.3`, this rewrites them to `1.2.0` (note the patch cascade).

Set every assembly to an explicit version, with detailed logging:

```bash
stampver -e 2.0.0 --verbose
```

Preview a patch bump without touching disk:

```bash
stampver -i PATCH --dryrun
```

Bump the patch number across a non-default file pattern:

```bash
stampver -i PATCH "Version.cs"
```

Stamp a different repository (recursively) without changing directory, optionally narrowing to a pattern:

```bash
stampver -i MINOR --dir "C:\MySourceCode\MyRepo"
stampver -i MINOR --dir "C:\MySourceCode\MyRepo" "*.txt"
```

## How it works

The pipeline is intentionally small:

1. **Parse arguments** into a `VersionArgs` aggregate; reject mutually-exclusive combinations early.
2. **Enumerate files** matching the pattern under the starting directory (the current working directory unless `--dir` is given), recursing through all subdirectories. When no pattern is given, the `AssemblyInfo.cs` and `*.csproj` defaults are scanned in turn.
3. **Pick a file format** per file based on extension - the MSBuild XML matcher for `.csproj`, the C# attribute matcher otherwise.
4. **Per line**, run the format's compiled regex; skip the format's comment marker (`//` or `<!--`).
5. **Transform the version** through `AssemblyVersion`, which preserves non-numeric tokens (e.g. `*`) and clamps numeric parts to `[0, UInt16.MaxValue]`.
6. **Write back** the updated lines - unless `--dryrun` is set.

Default output groups results by new version, e.g.:

```
1.2.0 (3 occurrences in 2 files)
1.0.1 (1 occurrence in 1 file)
```

## Building from source

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

Clone and build:

```bash
git clone https://github.com/craigtp/StampVer.git
cd StampVer
dotnet restore ./src/stampver.sln
dotnet build ./src/stampver.sln --configuration Release --no-restore
dotnet test ./src/stampver.sln
```

The solution lives in `src/`:

- `src/stampver/` - the CLI utility (no external runtime dependencies).
- `src/stampver.tests/` - the unit-test suite, using [NUnit 4](https://nunit.org/).
- `testfile/AssemblyTest.cs` - a representative sample file used for manual end-to-end runs of the published binary.

CI runs `restore`/`build`/`test` on Ubuntu against the .NET 10 SDK for every push to `master` and every pull request - see [`.github/workflows/main.yml`](.github/workflows/main.yml).

## Acknowledgements

StampVer's command-line argument parser was originally based upon Jonathan Pryor's [NDesk.Options](https://github.com/mono/mono/blob/main/mcs/class/Mono.Options/Mono.Options/Options.cs) (Novell, 2008, MIT-licensed), but has since been rewritten from scratch in modern C# while preserving the same prototype syntax and parsing semantics. The rewrite drops the legacy serialization plumbing, removes the locale-coupled type conversion, fixes a latent bug in the alias-removal path, and uses source-generated regex and composition-over-inheritance throughout.

## License

StampVer is released under the [MIT License](LICENSE). Copyright © 2020 Craig Phillips.
