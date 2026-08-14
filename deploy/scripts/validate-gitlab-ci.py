#!/usr/bin/env python3
"""Validate GitLab YAML structure and local includes without calling a GitLab instance."""

from __future__ import annotations

from pathlib import Path
from typing import Any

import yaml


ROOT_CONFIG = Path(".gitlab-ci.yml")
INCLUDED_CONFIGS = sorted(Path(".gitlab/ci").glob("*.yml"))


def validate_job_scripts(path: Path, document: dict[str, Any]) -> None:
    for name, value in document.items():
        if not isinstance(value, dict) or "script" not in value:
            continue
        script = value["script"]
        if not isinstance(script, list) or not all(isinstance(line, str) for line in script):
            raise ValueError(f"{path}: job {name} must contain a list of string script commands")


def load_documents(path: Path) -> list[dict[str, Any]]:
    documents = list(yaml.safe_load_all(path.read_text(encoding="utf-8")))
    if not documents or any(document is not None and not isinstance(document, dict) for document in documents):
        raise ValueError(f"{path}: every YAML document must be a mapping")
    return [document for document in documents if document]


def main() -> None:
    paths = [ROOT_CONFIG, *INCLUDED_CONFIGS]
    if not ROOT_CONFIG.is_file() or not INCLUDED_CONFIGS:
        raise ValueError("GitLab pipeline entry point or local includes are missing")

    for path in paths:
        for document in load_documents(path):
            validate_job_scripts(path, document)

    root_documents = load_documents(ROOT_CONFIG)
    pipeline_document = root_documents[-1]
    includes = pipeline_document.get("include", [])
    local_includes = {
        Path(item["local"])
        for item in includes
        if isinstance(item, dict) and isinstance(item.get("local"), str)
    }
    missing = sorted(path for path in local_includes if not path.is_file())
    if missing:
        raise ValueError(f"Missing local GitLab includes: {missing}")

    print(f"GitLab YAML valid: {len(paths)} files")


if __name__ == "__main__":
    main()
