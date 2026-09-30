# ME Attribute

## Overview

`ME Attribute` creates a new Meanders attribute object or modifies an existing attribute set for later use with Meanders objects.

The component can work with an existing `ME Attribute` or Rhino `ObjectAttributes` instance and supports object name, layer assignment, object color, and Rhino user text key-value pairs.

---

## Grasshopper Location

```text
Category: Meanders
Subcategory: Attributes
```

---

## Component Information

| Property        | Value        |
| --------------- | ------------ |
| Name            | ME Attribute |
| Nickname        | ME Attr      |
| Metadata ID     | me-attribute |
| Status          | Development  |
| Introduced In   | 0.1.0        |
| Last Updated In | 0.1.0        |

---

## Inputs

### Existing Attributes

**Nickname:** `A`

**Type:** Generic

**Access:** Item

**Optional:** Yes

**Default:** None

Existing Meanders attributes or Rhino `ObjectAttributes` to copy and modify.

Accepted values include:

- `ME_Attribute`
- `ME_Attribute_Goo`
- `Rhino.DocObjects.ObjectAttributes`

If no existing attributes are supplied, the component creates a new attribute object.

---

### Keys

**Nickname:** `K`

**Type:** Text

**Access:** List

**Optional:** Yes

**Default:** Empty list

User text keys to store in the Rhino object attributes.

Each key corresponds by index to a value supplied to the `Values` input.

---

### Values

**Nickname:** `V`

**Type:** Text

**Access:** List

**Optional:** Yes

**Default:** Empty list

User text values corresponding to the supplied `Keys`.

The number of values must match the number of keys.

---

### Name

**Nickname:** `N`

**Type:** Text

**Access:** Item

**Optional:** Yes

**Default:** None

Rhino object name to store in the resulting attributes.

If this input is empty, the existing object name is preserved when modifying existing attributes.

---

### Layer

**Nickname:** `L`

**Type:** Text

**Access:** Item

**Optional:** Yes

**Default:** None

Full Rhino layer path to assign to the resulting attributes.

If the specified layer does not exist, the current implementation attempts to create it in the active Rhino document.

Layer operations depend on an active Rhino document.

---

### Object Color

**Nickname:** `Oc`

**Type:** Color

**Access:** Item

**Optional:** Yes

**Default:** None

Object display color to store in the resulting attributes.

When a color is assigned, the Rhino color source is set to use the object color.

---

## Outputs

### Attributes

**Nickname:** `A`

**Type:** ME_Attribute

**Access:** Item

Resulting Meanders attribute object.

This output can be passed to components such as `ME Attach Attribute`.

---

### Keys

**Nickname:** `K`

**Type:** Text

**Access:** List

All user text keys stored in the resulting attributes.

---

### Values

**Nickname:** `V`

**Type:** Text

**Access:** List

All user text values stored in the resulting attributes.

---

### Name

**Nickname:** `N`

**Type:** Text

**Access:** Item

Object name stored in the resulting attributes.

---

### Layer

**Nickname:** `L`

**Type:** Text

**Access:** Item

Full Rhino layer path represented by the resulting attributes.

---

### Object Color

**Nickname:** `Oc`

**Type:** Color

**Access:** Item

Object color stored in the resulting attributes.

---

## Behavior

The component works on a copy of supplied attributes rather than directly mutating the original object.

Behavior depends on the supplied input:

```text
No Existing Attributes
        ↓
Create new ME Attribute
```

```text
Existing ME Attribute
        ↓
Duplicate
        ↓
Apply supplied changes
```

```text
Rhino ObjectAttributes
        ↓
Duplicate into ME Attribute
        ↓
Apply supplied changes
```

Only supplied optional values are applied.

---

## User Text Behavior

`Keys` and `Values` are interpreted as matching lists.

Example:

```text
Keys:
Material
PanelID

Values:
Oak
A-12
```

produces Rhino user text equivalent to:

```text
Material = Oak
PanelID = A-12
```

Keys and values must contain the same number of items.

If a user text value is empty, the underlying `ME_Attribute` implementation removes the user string associated with that key.

---

## Layer Behavior

Layer assignment uses the active Rhino document.

When a layer path is supplied:

1. The component searches the active Rhino document for the layer.
2. If the layer exists, its layer index is assigned.
3. If it does not exist, the current implementation attempts to create a new layer.
4. The resulting layer index is stored in the object attributes.

If there is no active Rhino document, layer assignment cannot be completed.

---

## Runtime Messages

### Errors

```text
Existing Attributes must be ME Attribute or Rhino ObjectAttributes.
```

This occurs when the `Existing Attributes` input receives an unsupported object type.

Use an `ME Attribute`, `ME_Attribute`, or Rhino `ObjectAttributes` instance.

---

```text
Keys and Values must have the same number of items.
```

This occurs when the `Keys` and `Values` lists contain different numbers of items.

Ensure that every key has one corresponding value.

---

## Example

### Goal

Create attributes for a Rhino object that should be placed on a specific layer and carry custom user text.

### Inputs

```text
Name:
Panel A12

Layer:
Fabrication::Panels

Object Color:
Custom selected color

Keys:
Material
PanelID

Values:
Oak
A-12
```

### Result

The output `ME Attribute` contains:

```text
Name = Panel A12
Layer = Fabrication::Panels
User Text:
    Material = Oak
    PanelID = A-12
Object Color = selected color
```

The resulting attributes can then be attached to geometry using `ME Attach Attribute`.

---

## Notes

- All inputs are optional.
- If no existing attributes are supplied, a new attribute object is created.
- Existing attributes are duplicated before modifications are applied.
- Layer lookup and creation require an active Rhino document.
- Keys and values must have matching list lengths.
- User text is stored using Rhino object user strings.
- Supplying an empty user text value removes that user string from the attribute set.

---

## Related Components

- `ME Attach Attribute` — Combines geometry with Meanders attributes.
- `ME Detach Geometry` — Extracts geometry and attributes from an ME Object.

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
ME_Attribute_Component

Component Namespace:
Meanders.Tools.Grasshopper.Components

Component Source:
Meanders.Tools/Grasshopper/Components/ME_Attribute_Component.cs

Core Class:
ME_Attribute

Core Namespace:
Meanders.Tools.Core

Core Source:
Meanders.Tools/Core/ME_Attribute.cs

Grasshopper Goo:
ME_Attribute_Goo
```

---

## Component GUID

```text
C91E7F42-5A63-4D88-B2E1-06F4A9C73D15
```

---

## Documentation Status

```text
Status: Draft
Last Reviewed: 2026-09-30
```
