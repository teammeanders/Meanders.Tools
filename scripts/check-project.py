#!/usr/bin/env python3

import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent


def run_script(relative_path):
    script_path = ROOT / relative_path

    print()
    print(f"Running {relative_path}")
    print("-" * (8 + len(str(relative_path))))

    result = subprocess.run(
        [sys.executable, str(script_path)],
        cwd=ROOT,
    )

    return result.returncode


def main():
    steps = [
        Path("scripts") / "validate-metadata.py",
        Path("scripts") / "generate-docs-index.py",
    ]

    for step in steps:
        code = run_script(step)

        if code != 0:
            print()
            print(f"FAILED: {step}")
            return code

    print()
    print("Project checks completed successfully.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
