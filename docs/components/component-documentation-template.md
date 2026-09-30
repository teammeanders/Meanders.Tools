# Component Documentation Template

> Use this template when creating documentation for a Meanders.Tools Grasshopper component.
> Replace all placeholder text before marking documentation as complete.

# <Component Display Name>

## Overview

`<Component Display Name>` <briefly explain what the component does in one or two sentences>.

<Add a slightly longer explanation of the intended use, important behavior, and where the component fits in the Meanders.Tools workflow.>

---

## Grasshopper Location

```text
Category: Meanders
Subcategory: <Subcategory>
```

---

## Component Information

| Property        | Value                                                        |
| --------------- | ------------------------------------------------------------ |
| Name            | <Component Display Name>                                     |
| Nickname        | <Component Nickname>                                         |
| Metadata ID     | <me-component-id>                                            |
| Status          | <Development / Testing / Experimental / Stable / Deprecated> |
| Introduced In   | <0.x.x>                                                      |
| Last Updated In | <0.x.x>                                                      |

---

## Inputs

### <Input Name>

**Nickname:** `<Nickname>`

**Type:** <Type>

**Access:** <Item / List / Tree>

**Optional:** <Yes / No>

**Default:** <Default value or None>

<Describe what the input expects, how it is interpreted, and any important constraints.>

---

<!-- Repeat the section above for each input. -->

## Outputs

### <Output Name>

**Nickname:** `<Nickname>`

**Type:** <Type>

**Access:** <Item / List / Tree>

<Describe what the output contains and any relevant behavior.>

---

<!-- Repeat the section above for each output. -->

## Behavior

<Explain important component behavior that is not obvious from the inputs and outputs.>

Examples may include:

- Data tree matching behavior
- Copy versus mutation behavior
- Rhino document dependencies
- Geometry handling
- State persistence
- Error handling
- Automatic layer creation
- Context menu behavior

---

## Context Menu Settings

<!-- Remove this section if the component has no custom context menu settings. -->

Right-click the component to access additional settings.

### <Setting Name>

<Describe the setting and its effect.>

---

## Component Message

<!-- Remove this section if the component does not display a message on the Grasshopper canvas. -->

The component displays:

```text
<Example component message>
```

<Explain what the message communicates.>

---

## Persistent Settings

<!-- Remove this section if the component has no persistent custom state. -->

The following settings are stored inside the Grasshopper document:

- <Setting>
- <Setting>

When the `.gh` file is saved and reopened, these settings are restored automatically.

---

## Runtime Messages

### Errors

<!-- Remove if none. -->

- `<Exact runtime error message>`

<Explain when this error occurs and how the user can resolve it.>

### Warnings

<!-- Remove if none. -->

- `<Exact runtime warning message>`

<Explain when this warning occurs and what it means.>

---

## Example

### Goal

<Describe a simple practical task.>

### Setup

```text
<Relevant component settings>
```

### Input

```text
<Example input>
```

### Output

```text
<Expected output>
```

### Explanation

<Explain what happened and why.>

---

## Notes

- <Important note>
- <Important limitation>
- <Implementation or usage detail that users should know>

---

## Related Components

- `<Related Component Name>` — <Why it is related>
- `<Related Component Name>` — <Why it is related>

If no related components currently exist, write:

> No directly related components are currently available.

---

## Compatibility

| Environment | Supported         |
| ----------- | ----------------- |
| Rhino       | <Version(s)>      |
| Grasshopper | <Version(s)>      |
| Platform    | <Windows / macOS> |

Only document tested compatibility.

---

## Developer Reference

```text
Class:
<Class Name>

Namespace:
<Namespace>

Source:
<Path to component source file>

Core Dependency:
<Path or class name, if applicable>
```

---

## Component GUID

```text
<Component GUID>
```

---

## Documentation Status

```text
Status: <Draft / Review / Complete>
Last Reviewed: <YYYY-MM-DD>
```

---

## Documentation Checklist

Before marking this page complete, verify:

- Component name matches the Grasshopper component
- Nickname matches the Grasshopper component
- Category and subcategory are correct
- Metadata ID matches the JSON metadata
- Component GUID matches the source code
- Every input is documented
- Every output is documented
- Input/output types match the implementation
- Item/list/tree access matches the implementation
- Optional inputs and defaults are documented
- Runtime errors and warnings are documented
- Context menu settings are documented
- Persistent state is documented where applicable
- At least one practical example is included
- Compatibility information reflects tested environments
- Related components are listed
- Developer source reference is correct
- Documentation metadata status is updated
