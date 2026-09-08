# Working Rules for Swecial Unity SDK

These rules apply to both humans and AI agents working in this repository.

## Project Communication

- **English Only:** All communication within the project must be in English. This includes commit messages, branch names, pull requests, code comments, issue descriptions, and documentation.

## Agent Instructions Structure

- **`AGENTS.md`** is the single source of truth for all rules, conventions, and workflows.
- **`CLAUDE.md`** points to `@AGENTS.md` for Claude agents.
- **`GEMINI.md`** is a symbolic link pointing directly to `AGENTS.md` for Gemini agents.
- **Important:** Always and only edit `AGENTS.md` if you want to update the rules. Never delete or overwrite the symbolic links manually.

## Branches

- `master` is the production branch. It must always be stable and deployable.
- `develop` is the integration branch and the default branch for daily development.
- Do not make changes directly to `master` or `develop`. Always use a short-lived branch and a pull request.
- Create working branches from the latest `develop` and name them `feature/<short-name>`, `fix/<short-name>`, or `chore/<short-name>`.

## Unity Package Conventions

- **Package Name:** `com.swecial.unity`
- Runtime client C# code resides in `Runtime/`.
- Unity Editor extensions and build scripts reside in `Editor/`.
