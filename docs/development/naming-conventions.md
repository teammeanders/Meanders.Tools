# Meanders.Tools Naming Conventions

This document defines the naming rules used across the Meanders.Tools codebase, component metadata, documentation, and future website.

The goal is to keep naming consistent, predictable, searchable, and stable throughout the lifetime of the project.

---

## 1. General Principles

Naming should be:

- Consistent
- Short but clear
- Human-readable
- Machine-readable where required
- Stable after release
- Easy to search in code, metadata, and documentation

Avoid unnecessary abbreviations unless they are already common in Rhino or Grasshopper.

Prefer clarity over clever naming.

---

## 2. Component Class Names

Grasshopper component classes use the following format:

```text
ME_<Name>_Component
```

Examples:

```text
ME_Attribute_Component
ME_Unit_Converter_Component
ME_Attach_Attribute_Component
ME_Detach_Geometry_Component
```

Rules:

- Prefix all Grasshopper component classes with `ME_`
- Use PascalCase words separated by underscores
- Always end component classes with `_Component`
- The file name should match the class name exactly

Example:

```text
Class:
ME_Unit_Converter_Component

File:
ME_Unit_Converter_Component.cs
```

---

## 3. Grasshopper Display Name

The Grasshopper display name should follow this format:

```text
ME <Readable Name>
```

Examples:

```text
ME Attribute
ME Unit Converter
ME Attach Attribute
ME Detach Geometry
```

Rules:

- Always begin with `ME`
- Use Title Case
- Keep the name descriptive but concise
- Avoid internal implementation terminology unless it is useful to the user
- Prefer terminology already familiar to Rhino and Grasshopper users

---

## 4. Grasshopper Nickname

Component nicknames should be short and easy to recognize on the Grasshopper canvas.

Examples:

```text
ME Attr
ME Units
ME Attach
ME Detach
```

Rules:

- Keep the `ME` prefix
- Prefer one to three short words
- Avoid ambiguous abbreviations
- Keep nicknames reasonably stable after release

---

## 5. Metadata IDs

Every component must have a stable machine-readable metadata ID.

Format:

```text
me-<component-name>
```

Examples:

```text
me-attribute
me-unit-converter
me-attach-attribute
me-detach-geometry
```

Rules:

- Lowercase only
- Use hyphens between words
- No spaces
- No underscores
- No uppercase letters
- Must remain stable after release

The metadata ID is used by:

- Component metadata files
- Documentation
- Website routes
- Validation tools
- Search
- Future APIs

---

## 6. Input and Output Names

Input and output display names should use Title Case.

Examples:

```text
Existing Attributes
Object Color
From Unit
Result
Converted Value
```

Rules:

- Use descriptive user-facing names
- Avoid overly short names
- Avoid implementation-specific terminology when a clearer user-facing term exists
- Reuse terminology consistently across components

---

## 7. Input and Output Nicknames

Port nicknames should be concise and readable.

Examples:

```text
Value        → V
Keys         → K
Values       → V
Name         → N
Layer        → L
Object Color → Oc
Geometry     → G
Tolerance    → T
```

Rules:

- Prefer one to three characters
- Use uppercase first letter
- Multi-word nicknames may use mixed case such as `Oc`
- Avoid duplicate nicknames inside the same component where they may create confusion
- Keep naming consistent across the plugin where possible

---

## 8. Input and Output Metadata IDs

Every input and output should have a stable machine-readable ID.

Use lowercase kebab-case.

Examples:

```text
existing-attributes
object-color
from-unit
converted-value
geometry
tolerance
```

Rules:

- Lowercase only
- Use hyphens between words
- No spaces
- No underscores
- Keep IDs stable after release

---

## 9. Port Access Types

Metadata should explicitly define Grasshopper access types using one of:

```text
item
list
tree
```

These values should always match the actual Grasshopper component definition.

---

## 10. Port Optional State

Every input should explicitly define whether it is optional.

Example:

```json
"optional": true
```

or:

```json
"optional": false
```

If an input has a meaningful default value, the metadata should also define it.

If no default exists:

```json
"default": null
```

---

## 11. Data Types

Input and output metadata should use simple, consistent type identifiers.

Examples:

```text
number
integer
boolean
text
color
point
vector
curve
surface
brep
mesh
subd
geometry
generic
ME_Object
ME_Attribute
```

Where a generic parameter accepts multiple concrete types, use:

```json
"type": "generic"
```

and optionally define:

```json
"acceptedTypes": [
  "ME_Attribute",
  "Rhino.DocObjects.ObjectAttributes"
]
```

---

## 12. Namespaces

The root namespace is:

```text
Meanders.Tools
```

Current namespace structure:

```text
Meanders.Tools.Core
Meanders.Tools.Grasshopper.Components
Meanders.Tools.Grasshopper.Goo
Meanders.Tools.Grasshopper.Parameters
Meanders.Tools.Plugin
```

Rules:

- Use `Meanders.Tools` as the root namespace
- Namespace structure should reflect architectural responsibility
- Folder structure should generally match namespace structure
- Avoid inconsistent casing such as `Meanders.tools`

---

## 13. Grasshopper Category

All plugin components currently use:

```text
Meanders
```

as the Grasshopper root category.

Do not introduce additional root categories without a project-level decision.

---

## 14. Grasshopper Subcategories

Subcategories should come from a controlled list.

Initial proposed subcategories:

```text
Attributes
Objects
Geometry
Data
Utilities
Analysis
Fabrication
IO
```

Rules:

- Reuse an existing subcategory whenever possible
- Do not create a new subcategory for a single component
- New subcategories should be discussed before introduction
- Category names should be short and broad enough to support multiple components

---

## 15. Component GUIDs

Every Grasshopper component must have a unique and stable `ComponentGuid`.

Rules:

- Every component must use a unique GUID
- The GUID must match the component metadata file
- Never reuse a GUID
- Never change the GUID of a released component
- Renaming a component does not justify changing its GUID

Changing a component GUID causes Grasshopper to treat it as a different component and can break existing Grasshopper definitions.

---

## 16. Versioning

Meanders.Tools follows Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

Examples:

```text
0.1.0
0.2.0
0.2.1
1.0.0
```

Individual components do not maintain independent versions.

Instead, each component metadata file should track:

```json
"introducedIn": "0.1.0",
"lastUpdatedIn": "0.1.0",
"deprecatedIn": null
```

---

## 17. Component Status

Component status should use one of the following controlled values:

```text
idea
planned
design
development
testing
experimental
stable
deprecated
```

Definitions:

- `idea` — Concept exists but is not yet planned
- `planned` — Approved for future development
- `design` — Behavior and interface are being defined
- `development` — Actively being implemented
- `testing` — Implementation exists and is being validated
- `experimental` — Available but behavior may change
- `stable` — Approved for normal use
- `deprecated` — Still available but should not be used in new work

---

## 18. File Naming

C# source files should match their primary class.

Examples:

```text
ME_Attribute_Component.cs
ME_Object_Goo.cs
ME_Attribute_Param.cs
ME_UnitConverter.cs
```

Component metadata files:

```text
me-attribute.json
me-unit-converter.json
```

Component documentation files:

```text
me-attribute.md
me-unit-converter.md
```

Icon source files:

```text
me-attribute.svg
me-unit-converter.svg
```

Generated Grasshopper icons:

```text
me-attribute.png
me-unit-converter.png
```

---

## 19. Component Metadata File Location

Component metadata files should be stored under:

```text
metadata/components/
```

Example:

```text
metadata/components/me-attribute.json
```

Each component should have exactly one metadata file.

The metadata ID and file name should match.

---

## 20. Documentation File Location

Component documentation should be stored under:

```text
docs/components/
```

Example:

```text
docs/components/me-attribute.md
```

The documentation slug should match the metadata ID.

---

## 21. Icons

Component icon naming should match the component metadata ID.

Example:

```text
me-attribute.svg
me-attribute.png
```

Recommended workflow:

```text
SVG source
    ↓
generated PNG
    ↓
Grasshopper
```

The SVG should be treated as the master icon source.

Recommended Grasshopper icon size:

```text
24 × 24 px
```

---

## 22. Descriptions

Every component and every port must have a meaningful description.

Avoid vague descriptions such as:

```text
Input value.
Output result.
Geometry input.
```

Prefer descriptions that explain purpose and expected data.

Example:

```text
Numeric value to convert between supported length units.
```

Descriptions should be understandable without reading the source code.

---

## 23. Short and Long Descriptions

Component metadata should support both a short and long description.

Example:

```json
"description": {
  "short": "Create or modify Meanders object attributes.",
  "long": "Creates a new Meanders attribute object or modifies an existing attribute definition, including name, layer, object color, and user text."
}
```

The short description is intended for:

- Grasshopper tooltips
- Search results
- Component lists
- Website cards

The long description is intended for:

- Documentation pages
- Detailed help
- Website component pages

---

## 24. Runtime Messages

Important runtime errors and warnings should be documented in component metadata when useful.

Example:

```json
"runtimeErrors": [
  "Keys and Values must have the same number of items."
]
```

---

## 25. Source References

Each component metadata file should reference its source code.

Example:

```json
"source": {
  "class": "ME_Attribute_Component",
  "namespace": "Meanders.Tools.Grasshopper.Components",
  "file": "Meanders.Tools/Grasshopper/Components/ME_Attribute_Component.cs"
}
```

---

## 26. Tags

Components may include tags for search and filtering.

Example:

```json
"tags": [
  "attributes",
  "metadata",
  "rhino",
  "user-text"
]
```

Rules:

- Use lowercase
- Prefer singular concepts where practical
- Avoid excessive tagging
- Reuse existing tags

---

## 27. Compatibility Metadata

Component metadata may define compatibility information.

Example:

```json
"compatibility": {
  "rhino": [
    "7"
  ],
  "grasshopper": [
    "7"
  ],
  "platforms": [
    "windows"
  ]
}
```

Compatibility information should reflect tested support rather than theoretical support.

---

## 28. Stability Rules

After a component is released, the following values should be treated as stable:

- Component GUID
- Metadata ID
- Port IDs
- Input/output order where possible
- Main component name where possible
- Data behavior where possible

Breaking changes should only be introduced intentionally and documented in `CHANGELOG.md`.

---

## 29. Input and Output Order

Input and output order should remain stable after a component is released.

Adding new ports should be done carefully to avoid breaking existing Grasshopper definitions or confusing existing users.

If a breaking port change is unavoidable, it should be documented in the changelog.

---

## 30. Final Rule

Consistency across the plugin is more important than optimizing the naming of a single component.

When uncertain about naming:

1. Check existing Meanders.Tools components
2. Check this document
3. Prefer established Rhino and Grasshopper terminology
4. Discuss new conventions before introducing them
