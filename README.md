# StampVer

[![Build and Test StampVer](https://github.com/craigtp/StampVer/actions/workflows/main.yml/badge.svg)](https://github.com/craigtp/StampVer/actions/workflows/main.yml)
[![License: MIT](https://img.shields.io/github/license/craigtp/StampVer)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

A small command-line utility for updating .NET assembly version attributes in bulk. StampVer walks the current directory tree, finds files matching a pattern (default `AssemblyInfo.cs`), and rewrites every `[assembly: AssemblyVersion(...)]` and `[assembly: AssemblyFileVersion(...)]` attribute - incrementing or decrementing a [Semantic Versioning](https://semver.org/) part, or replacing the version outright.

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

- **Increment, decrement, or explicitly set** the `MAJOR`, `MINOR`, `PATCH` (or `BUILD`) component of a version number.
- **Semantic cascade** on increment: bumping `MAJOR` resets `MINOR` and `PATCH` to `0`; bumping `MINOR` resets `PATCH` to `0`.
- **Preserves wildcard tokens** like `*` in `AssemblyVersion("1.0.*")` - only the numeric parts are touched.
- **Clamps every part to `[0, 65535]`** so generated versions remain valid .NET assembly metadata.
- **Dry-run mode** previews the changes without writing to disk.
- **Custom file patterns** for projects that don't use `AssemblyInfo.cs`.
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
stampver <command> <version-part-or-number> [filepattern] [options]
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
| `--quiet` | Suppress all console output. |
| `--verbose` | Log every file inspected and every change made. |
| `--dryrun` | Show what *would* change without modifying any files. Implies `--verbose`. |

`--quiet` and `--verbose` are mutually exclusive.

### File pattern

An optional final positional argument specifies which files to scan. Any pattern accepted by [`Directory.EnumerateFiles`](https://learn.microsoft.com/dotnet/api/system.io.directory.enumeratefiles) works (e.g. `*.cs`, `AssemblyInfo.*`). Defaults to `AssemblyInfo.cs`.

A file is only modified if it contains at least one `[assembly: AssemblyVersion("x.y.z")]` or `[assembly: AssemblyFileVersion("x.y.z")]` attribute. Comment lines (`//`) are skipped.

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

## How it works

The pipeline is intentionally small:

1. **Parse arguments** into a `VersionArgs` aggregate; reject mutually-exclusive combinations early.
2. **Enumerate files** matching the pattern under the current working directory.
3. **Per line**, match `Assembly(File)?Version("...")` via a compiled regex; skip `//` comment lines.
4. **Transform the version** through `AssemblyVersion`, which preserves non-numeric tokens (e.g. `*`) and clamps numeric parts to `[0, UInt16.MaxValue]`.
5. **Write back** the updated lines - unless `--dryrun` is set.

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
