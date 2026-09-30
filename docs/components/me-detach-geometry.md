# ME Detach Geometry

## Overview

`ME Detach Geometry` separates `ME Object` instances into their stored geometry and Meanders attributes.

The component preserves the original Grasshopper data tree paths while exposing the two parts of each object as separate outputs.

---

## Grasshopper Location

```text
Category: Meanders
Subcategory: Objects
```

---

## Component Information

| Property        | Value              |
| --------------- | ------------------ |
| Name            | ME Detach Geometry |
| Nickname        | ME Detach          |
| Metadata ID     | me-detach-geometry |
| Status          | Development        |
| Introduced In   | 0.1.0              |
| Last Updated In | 0.1.0              |

---

## Inputs

### ME Objects

**Nickname:** `O`

**Type:** ME_Object

**Access:** Tree

**Optional:** No

**Default:** None

ME Object instances to separate into geometry and attributes.

The component expects `ME_Object_Goo` values, typically created by `ME Attach Attribute`.

---

## Outputs

### Geometry

**Nickname:** `G`

**Type:** Generic

**Access:** Tree

Geometry stored inside each input ME Object.

The geometry is wrapped as generic Grasshopper data in the output tree.

---

### Attributes

**Nickname:** `A`

**Type:** ME_Attribute

**Access:** Tree

Meanders attributes stored inside each input ME Object.

Each output attribute is returned as `ME_Attribute_Goo`.

---

## Behavior

The component reads each `ME Object` and extracts:

```text
ME Object
├── Geometry
└── Attributes
```

The two parts are then written to separate output trees.

The original tree path of every input item is preserved.

---

## Data Tree Preservation

Example input:

```text
ME Objects

{0;0}
O0
O1

{0;1}
O2
```

produces:

```text
Geometry

{0;0}
G0
G1

{0;1}
G2
```

and:

```text
Attributes

{0;0}
A0
A1

{0;1}
A2
```

The component does not flatten or simplify the tree.

---

## Null Handling

If an input `ME_Object_Goo` is null, or its internal value is null, that item is skipped.

No output item is generated for the skipped value.

---

## Attribute Handling

The component creates a new `ME_Attribute` wrapper from the `ObjectAttributes` stored in each `ME Object`.

This keeps the output compatible with the Meanders attribute system.

---

## Example

### Goal

Recover geometry and attributes from objects previously created with `ME Attach Attribute`.

### Input

```text
ME Object 0
    Geometry = Curve A
    Layer = Fabrication::Cut
    Name = Profile A

ME Object 1
    Geometry = Curve B
    Layer = Fabrication::Cut
    Name = Profile B
```

### Outputs

Geometry:

```text
Curve A
Curve B
```

Attributes:

```text
Attribute 0
    Layer = Fabrication::Cut
    Name = Profile A

Attribute 1
    Layer = Fabrication::Cut
    Name = Profile B
```

---

## Typical Workflow

```text
Geometry
   +
ME Attribute
   ↓
ME Attach Attribute
   ↓
ME Object
   ↓
ME Detach Geometry
   ├── Geometry
   └── Attributes
```

This allows geometry and metadata to move together through a Grasshopper definition and be separated again when needed.

---

## Notes

- The component accepts ME Objects as a data tree.
- Geometry and attributes are returned as separate trees.
- Original Grasshopper paths are preserved.
- Null ME Objects are skipped.
- The component does not modify the input objects.
- The component does not require an active Rhino document.

---

## Related Components

- `ME Attach Attribute` — Creates ME Objects from geometry and attributes.
- `ME Attribute` — Creates or modifies the attribute data stored inside ME Objects.

---

## Compatibility

| Environment | Supported |
| ----------- | --------- |
| Rhino       | 7         |
| Grasshopper | 7         |
| Platform    | Windows   |

Compatibility reflects the current tested project target.

---

## Developer Reference

```text
Component Class:
ME_Detach_Geometry_Component

Component Namespace:
Meanders.Tools.Grasshopper.Components

Component Source:
Meanders.Tools/Grasshopper/Components/ME_Detach_Geometry_Component.cs

Core Object:
ME_Object

Core Attribute:
ME_Attribute

Grasshopper Goo:
ME_Object_Goo
ME_Attribute_Goo

Grasshopper Parameter:
ME_Object_Param
```

---

## Component GUID

```text
B7A8C3D2-8A7B-4B4A-9F25-6D2D7E8A9F11
```

---

## Documentation Status

```text
Status: Draft
Last Reviewed: 2026-09-30
```
