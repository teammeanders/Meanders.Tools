import json
from datetime import datetime, timezone
from pathlib import Path
from urllib.request import urlopen

ROOT = Path(__file__).resolve().parents[1]

COMPONENTS_DIR = ROOT / "data" / "components"
PARAMETERS_DIR = ROOT / "data" / "parameters"

COMPONENT_SCHEMA = ROOT / "data" / "components.schema.json"
PARAMETER_SCHEMA = ROOT / "data" / "parameters.schema.json"

OUTPUT_FILE = ROOT / "data" / "components.json"


def load_json(path: Path):
    with path.open("r", encoding="utf-8") as file:
        return json.load(file)


def load_json_files(directory: Path):
    items = []

    if not directory.exists():
        return items

    for path in sorted(directory.glob("*.json")):
        items.append(
            {
                "path": path,
                "data": load_json(path),
            }
        )

    return items


def validate_required_fields(data, required, path):
    for field in required:
        if field not in data:
            raise ValueError(
                f"{path}: missing required field '{field}'."
            )


def validate_component(data, path):
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
        "icon",
    ]

    validate_required_fields(
        data,
        required,
        path,
    )

    if not isinstance(data["description"], dict):
        raise ValueError(
            f"{path}: description must be an object."
        )

    validate_required_fields(
        data["description"],
        ["short", "long"],
        path,
    )

    for collection in [
        "inputs",
        "outputs",
        "settings",
        "examples",
        "notes",
        "errors",
        "related",
    ]:
        if collection in data and not isinstance(
            data[collection],
            list,
        ):
            raise ValueError(
                f"{path}: '{collection}' must be an array."
            )

    for index, port in enumerate(
        data.get("inputs", [])
    ):
        validate_port(
            port,
            f"{path}: inputs[{index}]",
        )

    for index, port in enumerate(
        data.get("outputs", [])
    ):
        validate_port(
            port,
            f"{path}: outputs[{index}]",
        )


def validate_port(data, path):
    required = [
        "name",
        "nickname",
        "type",
        "access",
        "description",
    ]

    validate_required_fields(
        data,
        required,
        path,
    )

    if "optional" in data and not isinstance(
        data["optional"],
        bool,
    ):
        raise ValueError(
            f"{path}: optional must be boolean."
        )


def validate_parameter(data, path):
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
        "icon",
        "access",
        "type",
    ]

    validate_required_fields(
        data,
        required,
        path,
    )

    if not isinstance(data["description"], dict):
        raise ValueError(
            f"{path}: description must be an object."
        )

    validate_required_fields(
        data["description"],
        ["short", "long"],
        path,
    )

    for collection in [
        "settings",
        "examples",
        "notes",
        "related",
    ]:
        if collection in data and not isinstance(
            data[collection],
            list,
        ):
            raise ValueError(
                f"{path}: '{collection}' must be an array."
            )


def validate_unique_ids(items, label):
    ids = [
        item["data"].get("id")
        for item in items
    ]

    missing = [
        index
        for index, item_id in enumerate(ids)
        if not item_id
    ]

    if missing:
        raise ValueError(
            f"{label}: one or more entries are missing an id."
        )

    duplicates = {
        item_id
        for item_id in ids
        if ids.count(item_id) > 1
    }

    if duplicates:
        raise ValueError(
            f"{label}: duplicate ids found: "
            f"{', '.join(sorted(duplicates))}"
        )


def validate_related_references(
    components,
    parameters,
):
    component_ids = {
        item["data"]["id"]
        for item in components
    }

    parameter_ids = {
        item["data"]["id"]
        for item in parameters
    }

    all_ids = component_ids | parameter_ids

    for item in components:
        path = item["path"]
        related = item["data"].get(
            "related",
            [],
        )

        for related_id in related:
            if related_id not in all_ids:
                raise ValueError(
                    f"{path}: related reference "
                    f"'{related_id}' does not exist."
                )

    for item in parameters:
        path = item["path"]
        related = item["data"].get(
            "related",
            [],
        )

        for related_id in related:
            if related_id not in all_ids:
                raise ValueError(
                    f"{path}: related reference "
                    f"'{related_id}' does not exist."
                )


def main():
    component_files = load_json_files(
        COMPONENTS_DIR
    )

    parameter_files = load_json_files(
        PARAMETERS_DIR
    )

    for item in component_files:
        validate_component(
            item["data"],
            item["path"],
        )

    for item in parameter_files:
        validate_parameter(
            item["data"],
            item["path"],
        )

    validate_unique_ids(
        component_files,
        "components",
    )

    validate_unique_ids(
        parameter_files,
        "parameters",
    )

    validate_related_references(
        component_files,
        parameter_files,
    )

    components = [
        item["data"]
        for item in component_files
    ]

    parameters = [
        item["data"]
        for item in parameter_files
    ]

    components.sort(
        key=lambda item: (
            item.get("subcategory", ""),
            item.get("name", ""),
        )
    )

    parameters.sort(
        key=lambda item: (
            item.get("subcategory", ""),
            item.get("name", ""),
        )
    )

    output = {
        "$schema": "./components.schema.json",
        "_generated": True,
        "_generatedAt": datetime.now(
            timezone.utc
        ).isoformat(),
        "components": components,
        "parameters": parameters,
    }

    OUTPUT_FILE.parent.mkdir(
        parents=True,
        exist_ok=True,
    )

    with OUTPUT_FILE.open(
        "w",
        encoding="utf-8",
        newline="\n",
    ) as file:
        json.dump(
            output,
            file,
            ensure_ascii=False,
            indent=2,
        )

        file.write("\n")

    print(
        f"Generated {OUTPUT_FILE}"
    )

    print(
        f"Components: {len(components)}"
    )

    print(
        f"Parameters: {len(parameters)}"
    )


if __name__ == "__main__":
    main()