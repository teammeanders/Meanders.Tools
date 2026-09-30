#!/usr/bin/env python3

import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
COMPONENTS_DIR = ROOT / "metadata" / "components"
PLUGIN_METADATA = ROOT / "metadata" / "plugin.json"
DOCS_DIR = ROOT / "docs" / "components"
CSPROJ = ROOT / "Meanders.Tools" / "Meanders.Tools.csproj"


def load_json(path):
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except Exception as exc:
        raise RuntimeError(f"Could not read JSON: {path}\n{exc}") from exc


def normalize_guid(value):
    return value.strip().upper()


def extract_component_info(source_path):
    text = source_path.read_text(encoding="utf-8-sig")

    class_match = re.search(
        r"public\s+class\s+([A-Za-z0-9_]+)\s*:\s*GH_Component",
        text,
        re.MULTILINE,
    )

    base_match = re.search(
        r":\s*base\s*\(\s*"
        r'"([^"]*)"\s*,\s*'
        r'"([^"]*)"\s*,\s*'
        r'"([^"]*)"\s*,\s*'
        r'"([^"]*)"\s*,\s*'
        r'"([^"]*)"\s*\)',
        text,
        re.DOTALL,
    )

    guid_match = re.search(
        r'new\s+Guid\s*\(\s*"([0-9A-Fa-f-]{36})"\s*\)',
        text,
        re.DOTALL,
    )

    result = {}

    if class_match:
        result["class"] = class_match.group(1)

    if base_match:
        result["name"] = base_match.group(1)
        result["nickname"] = base_match.group(2)
        result["description"] = base_match.group(3)
        result["category"] = base_match.group(4)
        result["subcategory"] = base_match.group(5)

    if guid_match:
        result["guid"] = normalize_guid(guid_match.group(1))

    return result


def get_csproj_version():
    if not CSPROJ.exists():
        return None

    text = CSPROJ.read_text(encoding="utf-8-sig")
    match = re.search(r"<Version>\s*([^<]+)\s*</Version>", text)

    return match.group(1).strip() if match else None


def main():
    errors = []
    warnings = []

    if not COMPONENTS_DIR.exists():
        errors.append(f"Missing directory: {COMPONENTS_DIR}")
        print_results(errors, warnings)
        return 1

    metadata_files = sorted(COMPONENTS_DIR.glob("*.json"))

    if not metadata_files:
        errors.append("No component metadata files found.")

    seen_ids = {}
    seen_guids = {}

    for metadata_path in metadata_files:
        try:
            data = load_json(metadata_path)
        except RuntimeError as exc:
            errors.append(str(exc))
            continue

        component_id = data.get("id")
        guid = data.get("guid")
        name = data.get("name", metadata_path.stem)

        prefix = f"[{name}]"

        # ---------------------------------------------------------
        # Required high-level fields
        # ---------------------------------------------------------

        required = [
            "id",
            "guid",
            "name",
            "nickname",
            "category",
            "subcategory",
            "status",
            "introducedIn",
            "lastUpdatedIn",
            "deprecatedIn",
            "description",
            "inputs",
            "outputs",
            "source",
            "documentation",
        ]

        for field in required:
            if field not in data:
                errors.append(f"{prefix} Missing metadata field: {field}")

        # ---------------------------------------------------------
        # Unique IDs and GUIDs
        # ---------------------------------------------------------

        if component_id:
            if component_id in seen_ids:
                errors.append(
                    f"{prefix} Duplicate component id '{component_id}' "
                    f"(also used by {seen_ids[component_id]})"
                )
            else:
                seen_ids[component_id] = metadata_path.name

            if metadata_path.stem != component_id:
                errors.append(
                    f"{prefix} Metadata filename should be '{component_id}.json'"
                )

        if guid:
            normalized = normalize_guid(guid)

            if normalized in seen_guids:
                errors.append(
                    f"{prefix} Duplicate GUID '{guid}' "
                    f"(also used by {seen_guids[normalized]})"
                )
            else:
                seen_guids[normalized] = metadata_path.name

        # ---------------------------------------------------------
        # Source file
        # ---------------------------------------------------------

        source = data.get("source") or {}
        source_file = source.get("file")

        if not source_file:
            errors.append(f"{prefix} Missing source.file")
            continue

        source_path = ROOT / source_file

        if not source_path.exists():
            errors.append(f"{prefix} Source file does not exist: {source_file}")
            continue

        code = extract_component_info(source_path)

        comparisons = [
            ("class", source.get("class"), code.get("class")),
            ("name", data.get("name"), code.get("name")),
            ("nickname", data.get("nickname"), code.get("nickname")),
            ("category", data.get("category"), code.get("category")),
            ("subcategory", data.get("subcategory"), code.get("subcategory")),
        ]

        for label, metadata_value, code_value in comparisons:
            if code_value is None:
                warnings.append(
                    f"{prefix} Could not detect {label} in source code."
                )
            elif metadata_value != code_value:
                errors.append(
                    f"{prefix} {label} mismatch: "
                    f"metadata='{metadata_value}' code='{code_value}'"
                )

        if guid and code.get("guid"):
            if normalize_guid(guid) != normalize_guid(code["guid"]):
                errors.append(
                    f"{prefix} GUID mismatch: "
                    f"metadata='{guid}' code='{code['guid']}'"
                )
        elif not code.get("guid"):
            warnings.append(f"{prefix} Could not detect ComponentGuid in source.")

        # ---------------------------------------------------------
        # Documentation
        # ---------------------------------------------------------

        documentation = data.get("documentation") or {}
        slug = documentation.get("slug")

        if slug:
            expected_doc = DOCS_DIR / f"{slug}.md"

            if not expected_doc.exists():
                errors.append(
                    f"{prefix} Documentation file missing: "
                    f"docs/components/{slug}.md"
                )
        else:
            errors.append(f"{prefix} Missing documentation.slug")

        # ---------------------------------------------------------
        # Port indexes
        # ---------------------------------------------------------

        for port_group in ("inputs", "outputs"):
            ports = data.get(port_group, [])

            indexes = [port.get("index") for port in ports]
            expected = list(range(len(ports)))

            if indexes != expected:
                errors.append(
                    f"{prefix} {port_group} indexes should be {expected}, "
                    f"found {indexes}"
                )

            ids = [port.get("id") for port in ports]

            if len(ids) != len(set(ids)):
                errors.append(f"{prefix} Duplicate {port_group} port id detected.")

    # -------------------------------------------------------------
    # Plugin version consistency
    # -------------------------------------------------------------

    if PLUGIN_METADATA.exists():
        try:
            plugin = load_json(PLUGIN_METADATA)
            metadata_version = plugin.get("version")
            release_current = (plugin.get("release") or {}).get("current")
            project_version = get_csproj_version()

            if metadata_version and release_current:
                if metadata_version != release_current:
                    errors.append(
                        "[Plugin] plugin.version does not match release.current: "
                        f"{metadata_version} != {release_current}"
                    )

            if metadata_version and project_version:
                if metadata_version != project_version:
                    errors.append(
                        "[Plugin] plugin.json version does not match csproj version: "
                        f"{metadata_version} != {project_version}"
                    )

        except RuntimeError as exc:
            errors.append(str(exc))
    else:
        warnings.append("metadata/plugin.json was not found.")

    print_results(errors, warnings)
    return 1 if errors else 0


def print_results(errors, warnings):
    print()
    print("Meanders.Tools metadata validation")
    print("=" * 36)

    if warnings:
        print()
        print("Warnings:")
        for warning in warnings:
            print(f"  - {warning}")

    if errors:
        print()
        print("Errors:")
        for error in errors:
            print(f"  - {error}")

        print()
        print(f"FAILED: {len(errors)} error(s), {len(warnings)} warning(s).")
    else:
        print()
        print(f"PASSED: 0 errors, {len(warnings)} warning(s).")


if __name__ == "__main__":
    sys.exit(main())
