# Changelog

All notable changes to Meanders.Tools will be documented in this file.

The project follows Semantic Versioning.

## [0.2.0] - 2026-10-13

### Added

- `ME Fab Text` component for creating fabrication-ready text geometry
- `ME Round Number` component for rounding numbers to a specified number of decimal places
- `ME Round to Odd/Even` component for rounding numbers to the nearest odd or even integer
- `ME Bake` component for baking geometry into Rhino with Meanders attributes
- New component icons for the added components
- Parameter documentation pages for `ME Attribute` and `ME Object`
- Component registry table for tracking component implementation and documentation status

### Changed

- Refined the canonical icon design system for Grasshopper-native Meanders component icons
- Moved the icon design system into GPT Knowledge so icon generation can use a stable visual source of truth
- Updated the icon-generation workflow to read component-specific metadata from GitHub
- Improved semantic icon generation rules so component examples are not treated as visual subjects
- Updated `ME Bake` behavior so baking is triggered only by a false-to-true transition of the Bake input
- Added optional branch grouping behavior to `ME Bake`, enabled by default
- Restored parameter documentation routing under `/parameters/[id]`

### Fixed

- Restored missing parameter documentation pages that were previously removed from the documentation site
- Prevented `ME Bake` from baking repeatedly when Grasshopper recomputes the component while Bake remains enabled

### Notes

- This release continues to target Rhino 7 / Grasshopper 7 on Windows.
- Distribution remains internal.
- GitHub Releases are used for the current downloadable build.

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
