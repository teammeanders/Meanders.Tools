#!/usr/bin/env python3

import json
from collections import defaultdict
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
COMPONENTS_DIR = ROOT / "metadata" / "components"
OUTPUT_FILE = ROOT / "docs" / "components" / "README.md"


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def main():
    metadata_files = sorted(COMPONENTS_DIR.glob("*.json"))

    if not metadata_files:
        raise SystemExit("No component metadata files found.")

    groups = defaultdict(list)

    for metadata_path in metadata_files:
        data = load_json(metadata_path)

        subcategory = data["subcategory"]
        name = data["name"]
        short_description = data["description"]["short"]
        slug = data["documentation"]["slug"]

        groups[subcategory].append(
            {
                "name": name,
                "description": short_description,
                "slug": slug,
            }
        )

    lines = []

    lines.append("# Meanders.Tools Components")
    lines.append("")
    lines.append(
        "This directory contains user-facing documentation for "
        "Meanders.Tools Grasshopper components."
    )
    lines.append("")
    lines.append(
        "> This file is generated from `metadata/components/*.json`. "
        "Do not edit component entries manually."
    )
    lines.append("")
    lines.append("## Components")
    lines.append("")

    for subcategory in sorted(groups):
        lines.append(f"### {subcategory}")
        lines.append("")

        for component in sorted(groups[subcategory], key=lambda item: item["name"]):
            lines.append(
                f"- [{component['name']}](./{component['slug']}.md)  "
            )
            lines.append(f"  {component['description']}")
            lines.append("")

    lines.append("---")
    lines.append("")
    lines.append("## Documentation Status")
    lines.append("")
    lines.append(
        "Component documentation status is tracked in each component metadata file."
    )
    lines.append("")
    lines.append("## Source of Truth")
    lines.append("")
    lines.append("Component definitions are maintained in:")
    lines.append("")
    lines.append("```text")
    lines.append("metadata/components/")
    lines.append("```")
    lines.append("")

    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_FILE.write_text("\n".join(lines), encoding="utf-8")

    print(f"Generated: {OUTPUT_FILE.relative_to(ROOT)}")
    print(f"Components: {sum(len(items) for items in groups.values())}")
    print(f"Subcategories: {len(groups)}")


if __name__ == "__main__":
    main()
