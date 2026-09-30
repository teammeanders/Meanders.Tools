# ME Unit Converter

## Overview

`ME Unit Converter` converts numerical length values between supported measurement units.

The component currently supports:

- Millimeter
- Centimeter
- Meter
- Inch
- Foot

The source unit, target unit, and conversion direction are configured from the component context menu.

---

## Grasshopper Location

```text
Category: Meanders
Subcategory: Utilities
```

---

## Component Information

| Property      | Value             |
| ------------- | ----------------- |
| Name          | ME Unit Converter |
| Nickname      | ME Units          |
| Metadata ID   | me-unit-converter |
| Status        | Development       |
| Introduced In | 0.1.0             |

---

## Input

### Value

**Nickname:** `X`

**Type:** Number

**Access:** Item

**Optional:** No

Numeric length value to convert.

---

## Output

### Result

**Nickname:** `Y`

**Type:** Number

**Access:** Item

Converted numerical value in the selected target unit.

---

## Context Menu Settings

Right-click the component to access its settings.

### From Unit

Defines the source measurement unit.

Available values:

- Millimeter
- Centimeter
- Meter
- Inch
- Foot

### To Unit

Defines the target measurement unit.

Available values:

- Millimeter
- Centimeter
- Meter
- Inch
- Foot

### Invert

Reverses the conversion direction without changing the selected source and target units.

For example:

```text
From Unit: Millimeter
To Unit: Meter
Invert: Off
```

produces:

```text
Millimeter → Meter
```

When `Invert` is enabled:

```text
Meter → Millimeter
```

---

## Component Message

The component displays its active conversion directly on the Grasshopper canvas.

Example:

```text
Length
mm → cm
```

When inverted:

```text
Length
mm ← cm
```

This provides immediate visual feedback without reopening the context menu.

---

## Persistent Settings

The following settings are stored inside the Grasshopper document:

- From Unit
- To Unit
- Invert

When the `.gh` file is saved and reopened, the component restores these settings automatically.

---

## Conversion Logic

All supported length conversions use millimeters as the internal base unit.

The conversion process is:

```text
Source Unit
    ↓
Millimeters
    ↓
Target Unit
```

This keeps conversion behavior consistent across all supported units.

---

## Supported Conversion Factors

| Unit       | Millimeter Factor |
| ---------- | ----------------: |
| Millimeter |                 1 |
| Centimeter |                10 |
| Meter      |              1000 |
| Inch       |              25.4 |
| Foot       |             304.8 |

---

## Example

Convert:

```text
12 ft
```

to millimeters.

Settings:

```text
From Unit: Foot
To Unit: Millimeter
Invert: Off
```

Input:

```text
12
```

Output:

```text
3657.6
```

---

## Notes

- The component currently supports length units only.
- Unit selection is controlled through the component context menu rather than additional Grasshopper inputs.
- The selected settings are persistent.
- The component does not require an active Rhino document.

---

## Related Components

No related unit components are currently available.

---

## Developer Reference

```text
Class:
ME_Unit_Converter_Component

Namespace:
Meanders.Tools.Grasshopper.Components

Source:
Meanders.Tools/Grasshopper/Components/ME_Unit_Converter_Component.cs

Core Utility:
Meanders.Tools/Core/ME_UnitConverter.cs
```

---

## Component GUID

```text
5A4E9E1D-4B3D-4D0B-8E4B-1C9F9F0D3A71
```
