# Changelog

All notable changes to Meanders.Tools will be documented in this file.

The project follows Semantic Versioning.

## [0.1.0] - 2026-09-30

### Added

- Initial Meanders.Tools plugin architecture
- `ME Attribute` component
- `ME Attach Attribute` component
- `ME Detach Geometry` component
- `ME Unit Converter` component
- Custom `ME Object` and `ME Attribute` data types
- Grasshopper parameter types for ME Object and ME Attribute
- Component and parameter icons
- Persistent unit converter settings
- Unified component documentation data in `data/components.json`
- Plugin metadata in `data/plugin.json`
- Project validation script
- Development naming conventions
- Component definition-of-done checklist

### Changed

- Standardized project namespace to `Meanders.Tools`
- Reorganized project folders into Core, Grasshopper, Plugin, and Resources
- Standardized component names, categories, subcategories, and metadata
- Simplified documentation and metadata structure for easier long-term maintenance

### Fixed

- Grasshopper component ID conflict caused by duplicate plugin assemblies during development
- Plugin resource paths after project and namespace renaming
- Unit Converter state persistence and context-menu feedback
- Project version consistency between plugin metadata and project configuration

### Notes

- This release targets Rhino 7 / Grasshopper 7 on Windows.
- Distribution is currently internal.
- Yak package and installer are not included in this release.
