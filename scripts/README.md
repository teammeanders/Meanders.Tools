# Meanders.Tools Scripts

This directory contains small development utilities for checking project consistency.

## `check-project.py`

Runs the main project validation.

It currently checks:

- Duplicate component IDs
- Duplicate component GUIDs
- Component GUID consistency between C# and `data/components.json`
- Component name consistency
- Component nickname consistency
- Grasshopper category consistency
- Grasshopper subcategory consistency
- Basic input/output metadata structure
- Missing component entries in `data/components.json`
- Plugin version consistency between `data/plugin.json` and `Meanders.Tools.csproj`
- Component icon file existence

Run from the repository root:

```bash
python scripts/check-project.py
```

A successful result should look like:

```text
Meanders.Tools project check
============================

PASSED: 0 errors, 0 warning(s).
```

Warnings do not currently block validation.

Errors cause the check to fail.

## Project Data

Component documentation and metadata are stored in:

```text
data/components.json
```

Plugin-level information is stored in:

```text
data/plugin.json
```

Component icons are expected in:

```text
assets/icons/
```

## Recommended Workflow

After adding or modifying a component:

1. Update the C# component
2. Update `data/components.json`
3. Add or update the component icon
4. Test the component in Rhino / Grasshopper
5. Run:

```bash
python scripts/check-project.py
```

6. Fix any reported errors
7. Commit and push the changes
