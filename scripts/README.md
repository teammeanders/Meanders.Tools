# Meanders.Tools Scripts

This directory contains small development utilities used to keep project metadata and documentation consistent.

## Available Scripts

### `validate-metadata.py`

Validates component and plugin metadata against the current project structure.

It checks:

- Duplicate component IDs
- Duplicate component GUIDs
- Source file existence
- Component class name
- Component name and nickname
- Category and subcategory
- Component GUID consistency
- Documentation file existence
- Input/output index order
- Duplicate port IDs
- Plugin version consistency between `plugin.json` and `Meanders.Tools.csproj`

Run:

```bash
python scripts/validate-metadata.py
```

---

### `generate-docs-index.py`

Generates:

```text
docs/components/README.md
```

from component metadata files located in:

```text
metadata/components/
```

The generated index groups components by subcategory and includes:

- Component name
- Documentation link
- Short description

Run:

```bash
python scripts/generate-docs-index.py
```

Do not manually maintain component entries in `docs/components/README.md`, because this file can be regenerated from metadata.

---

### `check-project.py`

Runs the main project checks in sequence:

1. Metadata validation
2. Component documentation index generation

Run:

```bash
python scripts/check-project.py
```

This is the recommended command to run before important commits, pull requests, or releases.

A successful run should end with:

```text
Project checks completed successfully.
```

## Requirements

The scripts currently use only the Python standard library.

No additional packages are required.

Recommended:

```text
Python 3.10+
```

## Source of Truth

Component metadata:

```text
metadata/components/
```

Plugin metadata:

```text
metadata/plugin.json
```

Component documentation:

```text
docs/components/
```

When adding or modifying a component, update the metadata first, then run:

```bash
python scripts/check-project.py
```
