#!/usr/bin/env python3

import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent

COMPONENTS_JSON = ROOT / "data" / "components.json"
PLUGIN_JSON = ROOT / "data" / "plugin.json"

COMPONENTS_SOURCE_DIR = (
    ROOT / "Meanders.Tools" / "Grasshopper" / "Components"
)

CSPROJ = ROOT / "Meanders.Tools" / "Meanders.Tools.csproj"
ICONS_DIR = ROOT / "assets" / "icons"


def load_json(path):
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except Exception as exc:
        raise RuntimeError(
            f"Could not read JSON file: {path}\n{exc}"
        ) from exc


def normalize_guid(value):
    return str(value).strip().upper()


def read_project_version():
    if not CSPROJ.exists():
        return None

    text = CSPROJ.read_text(encoding="utf-8-sig")

    match = re.search(
        r"<Version>\s*([^<]+?)\s*</Version>",
        text
    )

    if not match:
        return None

    return match.group(1).strip()


def parse_component_file(path):
    text = path.read_text(encoding="utf-8-sig")

    class_match = re.search(
        r"public\s+class\s+([A-Za-z0-9_]+)\s*:\s*GH_Component",
        text,
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

    if not class_match or not base_match or not guid_match:
        return None

    return {
        "file": path,
        "class": class_match.group(1),
        "name": base_match.group(1),
        "nickname": base_match.group(2),
        "description": base_match.group(3),
        "category": base_match.group(4),
        "subcategory": base_match.group(5),
        "guid": normalize_guid(guid_match.group(1)),
    }


def collect_code_components():
    result = {}

    if not COMPONENTS_SOURCE_DIR.exists():
        return result

    for path in sorted(COMPONENTS_SOURCE_DIR.glob("*.cs")):
        parsed = parse_component_file(path)

        if not parsed:
            continue

        result[parsed["guid"]] = parsed

    return result


def check_components(errors, warnings):
    if not COMPONENTS_JSON.exists():
        errors.append("Missing data/components.json")
        return

    data = load_json(COMPONENTS_JSON)
    components = data.get("components")

    if not isinstance(components, list):
        errors.append(
            "data/components.json must contain a 'components' array."
        )
        return

    code_components = collect_code_components()

    seen_ids = set()
    seen_guids = set()

    for component in components:
        name = component.get("name", "<unnamed>")
        prefix = f"[{name}]"

        required = [
            "id",
            "guid",
            "name",
            "nickname",
            "category",
            "subcategory",
            "status",
            "introducedIn",
            "description",
            "inputs",
            "outputs",
        ]

        for field in required:
            if field not in component:
                errors.append(
                    f"{prefix} Missing field: {field}"
                )

        component_id = component.get("id")
        guid = component.get("guid")

        if component_id:
            if component_id in seen_ids:
                errors.append(
                    f"{prefix} Duplicate component id: {component_id}"
                )
            seen_ids.add(component_id)

        if guid:
            normalized_guid = normalize_guid(guid)

            if normalized_guid in seen_guids:
                errors.append(
                    f"{prefix} Duplicate component GUID: {guid}"
                )

            seen_guids.add(normalized_guid)

            code = code_components.get(normalized_guid)

            if not code:
                errors.append(
                    f"{prefix} No matching GH_Component found "
                    f"for GUID {guid}"
                )
            else:
                comparisons = [
                    ("name", component.get("name"), code["name"]),
                    ("nickname", component.get("nickname"), code["nickname"]),
                    ("category", component.get("category"), code["category"]),
                    ("subcategory", component.get("subcategory"), code["subcategory"]),
                ]

                for label, json_value, code_value in comparisons:
                    if json_value != code_value:
                        errors.append(
                            f"{prefix} {label} mismatch: "
                            f"JSON='{json_value}' "
                            f"code='{code_value}'"
                        )

        for group_name in ("inputs", "outputs"):
            ports = component.get(group_name, [])

            if not isinstance(ports, list):
                errors.append(
                    f"{prefix} '{group_name}' must be an array."
                )
                continue

            for index, port in enumerate(ports):
                for field in (
                    "name",
                    "nickname",
                    "type",
                    "access",
                    "description",
                ):
                    if field not in port:
                        errors.append(
                            f"{prefix} {group_name}[{index}] "
                            f"missing field: {field}"
                        )

        icon = component.get("icon")

        if icon:
            icon_path = ICONS_DIR / icon

            if not icon_path.exists():
                warnings.append(
                    f"{prefix} Icon not found yet: "
                    f"assets/icons/{icon}"
                )

    json_guids = {
        normalize_guid(c["guid"])
        for c in components
        if c.get("guid")
    }

    for guid, code in code_components.items():
        if guid not in json_guids:
            errors.append(
                "[Code] Component has no entry in components.json: "
                f"{code['name']} ({code['file'].name})"
            )


def check_plugin(errors, warnings):
    if not PLUGIN_JSON.exists():
        errors.append("Missing data/plugin.json")
        return

    plugin = load_json(PLUGIN_JSON)

    json_version = plugin.get("version")
    project_version = read_project_version()

    if not json_version:
        errors.append(
            "[Plugin] data/plugin.json is missing 'version'."
        )

    if not project_version:
        warnings.append(
            "[Plugin] Could not read <Version> from "
            "Meanders.Tools.csproj."
        )

    if (
        json_version
        and project_version
        and json_version != project_version
    ):
        errors.append(
            "[Plugin] Version mismatch: "
            f"data/plugin.json='{json_version}' "
            f"csproj='{project_version}'"
        )

    release = plugin.get("release")

    if isinstance(release, dict):
        current = release.get("current")

        if current and json_version and current != json_version:
            errors.append(
                "[Plugin] release.current does not match version: "
                f"{current} != {json_version}"
            )


def main():
    errors = []
    warnings = []

    try:
        check_components(errors, warnings)
        check_plugin(errors, warnings)
    except RuntimeError as exc:
        errors.append(str(exc))

    print()
    print("Meanders.Tools project check")
    print("=" * 28)

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
        print(
            f"FAILED: {len(errors)} error(s), "
            f"{len(warnings)} warning(s)."
        )
        return 1

    print()
    print(
        f"PASSED: 0 errors, {len(warnings)} warning(s)."
    )

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
