# Contributing to AspNetConventions

Thank you for your interest in contributing to **AspNetConventions**!
This project is focused on providing a unified conventions engine for ASP.NET Core routing, parameters, Razor Pages, Minimal APIs, and response normalization.
Contributions of all kinds are welcome — bug reports, code improvements, features, documentation, and samples.

Please read the following guidelines to ensure a smooth and productive contribution process.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Workflow](#development-workflow)
- [Repository Structure](#repository-structure)
- [Branching Model](#branching-model)
- [Coding Guidelines](#coding-guidelines)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Reporting Issues](#reporting-issues)
- [Feature Requests](#feature-requests)
- [Style & Formatting](#style--formatting)
- [Commit Message Guidelines](#commit-message-guidelines)
- [License](#license)

---

## Code of Conduct

This project follows the **Contributor Covenant Code of Conduct**.
By participating, you agree to maintain a respectful, inclusive environment.

---

## How Can I Contribute?

There are several ways to contribute:

- Fix bugs or improve existing code
- Implement new features (routing conventions, casing strategies, formatters, etc.)
- Improve documentation or examples
- Create sample applications
- Write tests
- Optimize performance
- Help triage issues
- Suggest enhancements

All contributions are valuable!

---

## Development Workflow

### 1. Fork the repository
Click **Fork** at the top-right of the GitHub page.

### 2. Clone your fork

```bash
git clone https://github.com/keizrivas/AspNetConventions.git
cd AspNetConventions
```

### 3. Create a feature branch

```bash
git checkout -b feature/my-improvement
```

### 4. Build the solution

```bash
dotnet build
```

### 5. Run tests

```bash
dotnet test
```

### 6. Commit changes & push

```bash
git push origin feature/my-improvement
```

### 7. Open a Pull Request
Go to your fork and click **New Pull Request**.

---

## Repository Structure

```
AspNetConventions/
├── src/                   # Library source code
│   └── AspNetConventions/
├── tests/                 # Unit tests (coming soon)
├── samples/               # Sample apps (MVC, Razor Pages, Minimal APIs)
├── docs/                  # Documentation and guides
├── LICENSE
├── README.md
└── CONTRIBUTING.md
```

---

## Branching Model

We use a simplified flow:

- `main` – stable code, releases only
- `dev` – active development
- `feature/*` – new functionality
- `fix/*` – bug fixes
- `docs/*` – documentation changes

PRs should target **dev**, not main.

---

## Coding Guidelines

Before contributing code, please follow these:

### ✔ Target .NET 8+
ASP.NET Core conventions evolve quickly; stick to LTS versions (8, 9, 10, etc.).

### ✔ Follow the library architecture
The project is organized into:

- **Conventions** (ApplicationModel, PageModel, EndpointModel)
- **Transformers** (route tokens, kebab-case, casing engine)
- **Serialization** (STJ converters)
- **Models** (Response, Pagination, InternalError, etc.)
- **Extensions** (AddAspNetConventions, builder extensions)
- **Utils** (Regex parsers, helpers)

Keep new code consistent.

### ✔ Add XML comments to public APIs
This ensures NuGet packages remain well-documented.

### ✔ Add tests when possible
Especially for:
- Route transformations
- Parameter binding behavior
- Regex edge cases
- Razor Pages template normalization

---

## Submitting a Pull Request

A good PR includes:

1. Clear description of the change
2. Link to related issue (if any)
3. Passing build + tests
4. Updated documentation if needed
5. No breaking changes unless discussed beforehand

PRs are reviewed for:

- Consistency with project conventions
- Naming clarity
- Performance impact
- Maintainability

---

## Reporting Issues

If you find a bug, please include:

- Expected behavior
- Actual behavior
- Reproduction steps
- Sample code or project
- ASP.NET Core version
- Operating system
- Screenshots or stack traces (if relevant)

Good issues get fixed faster!

---

## Feature Requests

Before submitting a feature request, think about:

- Does it align with the project’s goal?
- Does it increase complexity unnecessarily?
- Does ASP.NET Core already support it?

Open an issue with:

- Description
- Use case
- Example
- Why it belongs in AspNetConventions

---

## Style & Formatting

- Use **C# 12 / latest** where appropriate
- Prefer expression-bodied members where clean
- Keep files small and focused
- Use **PascalCase** for public APIs
- Use **camelCase** for locals and parameters
- Avoid unnecessary abstractions
- Follow `.editorconfig` rules (coming soon)

---

## Commit Message Guidelines

Use conventional commits:

```
feat: add kebab-case converter for Razor Pages
fix: correct route parameter regex for optional segments
docs: update README with MVC sample
test: add tests for controller route normalization
refactor: simplify parameter transformation logic
```

---

## License

By contributing, you agree that your contributions will be licensed
under the **MIT License**, the same license as the project.

---

Thank you for contributing! ❤️
Your help makes AspNetConventions better for the .NET community.
