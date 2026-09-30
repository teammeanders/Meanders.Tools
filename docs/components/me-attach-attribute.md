# ME Attach Attribute

## Overview

`ME Attach Attribute` combines geometry or supported Grasshopper data with Meanders attributes to create `ME Object` instances.

The component preserves the incoming Grasshopper data tree structure and pairs attributes with geometry by branch and item position.

---

## Grasshopper Location

```text
Category: Meanders
Subcategory: Objects
```

---

## Component Information

| Property        | Value               |
| --------------- | ------------------- |
| Name            | ME Attach Attribute |
| Nickname        | ME Attach           |
| Metadata ID     | me-attach-attribute |
| Status          | Development         |
| Introduced In   | 0.1.0               |
| Last Updated In | 0.1.0               |

---

## Inputs

### Geometry

**Nickname:** `G`

**Type:** Generic

**Access:** Tree

**Optional:** No

**Default:** None

Geometry or supported Grasshopper data to wrap inside `ME Object` instances.

Supported data currently includes:

- ME Object geometry
- Point
- Curve
- Brep
- Surface
- SubD
- Geometry Group
- Mesh
- Rectangle
- Line
- Generic wrapped Grasshopper data

The original data tree paths are preserved in the output.

---

### Attributes

**Nickname:** `A`

**Type:** Generic

**Access:** Tree

**Optional:** Yes

**Default:** None

Meanders attributes or Rhino `ObjectAttributes` to attach to the supplied geometry.

Accepted attribute types include:

- `ME_Attribute`
- `ME_Attribute_Goo`
- `Rhino.DocObjects.ObjectAttributes`

If no attributes are supplied, the component creates each `ME Object` with a default empty Rhino `ObjectAttributes` instance.

---

## Outputs

### ME Objects

**Nickname:** `O`

**Type:** ME_Object

**Access:** Tree

ME Object instances containing the source geometry together with the matched attributes.

The output tree follows the geometry input tree structure.

---

## Behavior

The geometry tree controls the output structure.

For every geometry branch:

```text
Geometry Branch
      ↓
Each Geometry Item
      ↓
Matched Attribute
      ↓
New ME Object
```

Each output item stores:

```text
Geometry
+
Object Attributes
```

The component creates new `ME Object` instances and does not modify the original geometry or attribute data in place.

---

## Attribute Matching

Attributes are matched to geometry by branch and item index.

### Same Branch

If the attribute tree contains the same path as the geometry branch, that attribute branch is used.

Example:

```text
Geometry:
{0}  G0, G1, G2

Attributes:
{0}  A0, A1, A2
```

produces:

```text
G0 + A0
G1 + A1
G2 + A2
```

---

### Shorter Attribute Branch

If an attribute branch contains fewer items than the geometry branch, the last available attribute is reused for the remaining geometry items.

Example:

```text
Geometry:
{0}  G0, G1, G2, G3

Attributes:
{0}  A0, A1
```

produces:

```text
G0 + A0
G1 + A1
G2 + A1
G3 + A1
```

---

### Missing Matching Attribute Branch

If the exact geometry path does not exist in the attribute tree, the component attempts to use the first attribute branch as a fallback.

This allows a shared attribute branch to be applied to geometry from multiple branches.

---

### No Attributes

If no attribute input is supplied, each output `ME Object` receives a default empty attribute set.

---

## Data Tree Preservation

The geometry input determines the output paths.

Example:

```text
Geometry Input

{0;0}
G0
G1

{0;1}
G2
```

produces:

```text
ME Objects

{0;0}
O0
O1

{0;1}
O2
```

The component does not flatten or simplify the geometry tree.

---

## Supported Geometry Extraction

The component unwraps common Grasshopper data types before storing them inside an `ME Object`.

Supported wrappers currently include:

```text
GH_Point
GH_Curve
GH_Brep
GH_Surface
GH_SubD
GH_GeometryGroup
GH_Mesh
GH_Rectangle
GH_Line
GH_ObjectWrapper
```

If an incoming value is already an `ME_Object_Goo`, its internal geometry is extracted.

For unsupported Grasshopper goo types, the component falls back to storing the goo object itself.

---

## Example

### Goal

Attach one attribute definition to several pieces of geometry.

### Geometry

```text
{0}
Curve A
Curve B
Curve C
```

### Attributes

```text
{0}
ME Attribute:
    Layer = Fabrication::Cut
    Name = Profile
```

### Output

```text
{0}
ME Object 0
ME Object 1
ME Object 2
```

Each output object contains its original curve plus the supplied attributes.

Because only one attribute item is provided, that attribute is reused for all geometry items in the branch.

---

## Notes

- Geometry is required.
- Attributes are optional.
- Geometry tree structure is preserved.
- Attribute matching is branch-aware.
- The last attribute in a branch is reused when there are fewer attributes than geometry items.
- Existing `ME Object` inputs are unwrapped to their internal geometry before creating the new object.
- The component does not require an active Rhino document.
- The component creates new `ME Object` instances rather than modifying input data in place.

---

## Related Components

- `ME Attribute` — Creates or modifies the attributes used by this component.
- `ME Detach Geometry` — Separates an ME Object back into geometry and attributes.

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
ME_Attach_Attribute_Component

Component Namespace:
Meanders.Tools.Grasshopper.Components

Component Source:
Meanders.Tools/Grasshopper/Components/ME_Attach_Attribute_Component.cs

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
E42B8C71-93F6-4A05-A6D2-5C17F9B83426
```

---

## Documentation Status

```text
Status: Draft
Last Reviewed: 2026-09-30
```
