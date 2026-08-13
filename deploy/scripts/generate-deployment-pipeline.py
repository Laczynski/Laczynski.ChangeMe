#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
import re
import subprocess
from pathlib import Path
from typing import Any
from urllib.parse import urlparse


STABLE_VERSION = re.compile(r"^v\d+\.\d+\.\d+$")
COMMIT_SHA = re.compile(r"^[0-9a-f]{40}$")
INSTANCE_ID = re.compile(r"^[a-z0-9][a-z0-9-]*$")
GITLAB_ENVIRONMENT = re.compile(r"^[a-z0-9][a-z0-9._/-]*$")
SERVER_NAME = re.compile(r"^[A-Za-z0-9.-]+$")


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Validate Ansible inventory and generate manual GitLab deployment jobs."
    )
    parser.add_argument("--inventory", required=True, type=Path)
    parser.add_argument("--application-version", required=True)
    parser.add_argument("--configuration-commit", required=True)
    parser.add_argument("--output", required=True, type=Path)
    return parser.parse_args()


def load_inventory(path: Path) -> dict[str, Any]:
    completed = subprocess.run(
        ["ansible-inventory", "--inventory", str(path), "--list"],
        check=True,
        capture_output=True,
        text=True,
    )
    return json.loads(completed.stdout)


def require(condition: bool, message: str, errors: list[str]) -> None:
    if not condition:
        errors.append(message)


def validate_url(value: Any) -> bool:
    if not isinstance(value, str) or "\n" in value or "\r" in value:
        return False
    parsed = urlparse(value)
    try:
        port = parsed.port
    except ValueError:
        return False
    return (
        parsed.scheme in {"http", "https"}
        and bool(parsed.hostname)
        and bool(re.fullmatch(r"[A-Za-z0-9.-]+", parsed.hostname or ""))
        and parsed.username is None
        and parsed.password is None
        and parsed.query == ""
        and parsed.fragment == ""
        and (port is None or 1 <= port <= 65535)
    )


def validate_instance(instance: str, variables: dict[str, Any]) -> list[str]:
    errors: list[str] = []
    prefix = f"{instance}: "
    enabled = variables.get("deployment_enabled")
    require(isinstance(enabled, bool), prefix + "deployment_enabled must be a boolean", errors)
    require(
        variables.get("deployment_tier") in {"development", "production", "customer"},
        prefix + "deployment_tier is invalid",
        errors,
    )

    environment = variables.get("gitlab_environment")
    require(
        isinstance(environment, str) and bool(GITLAB_ENVIRONMENT.fullmatch(environment)),
        prefix + "gitlab_environment is invalid",
        errors,
    )
    require(validate_url(variables.get("public_url")), prefix + "public_url is invalid", errors)

    application_name = variables.get("application_name")
    require(
        isinstance(application_name, str)
        and bool(re.fullmatch(r"[a-z0-9][a-z0-9-]*", application_name)),
        prefix + "application_name is invalid",
        errors,
    )
    if isinstance(application_name, str):
        expected_paths = {
            "application_root": {f"/opt/{application_name}"},
            "application_releases_root": {
                f"/opt/{application_name}/releases",
                "{{ application_root }}/releases",
            },
            "application_current_path": {
                f"/opt/{application_name}/current",
                "{{ application_root }}/current",
            },
            "application_previous_path": {
                f"/opt/{application_name}/previous",
                "{{ application_root }}/previous",
            },
            "application_config_root": {f"/etc/{application_name}"},
            "application_config_revisions_root": {
                f"/etc/{application_name}/config-revisions",
                "{{ application_config_root }}/config-revisions",
            },
            "application_current_config_path": {
                f"/etc/{application_name}/current-config",
                "{{ application_config_root }}/current-config",
            },
            "application_previous_config_path": {
                f"/etc/{application_name}/previous-config",
                "{{ application_config_root }}/previous-config",
            },
            "application_secrets_path": {
                f"/etc/{application_name}/secrets.env",
                "{{ application_config_root }}/secrets.env",
            },
            "application_data_root": {f"/var/lib/{application_name}"},
            "application_storage_root": {
                f"/var/lib/{application_name}/storage",
                "{{ application_data_root }}/storage",
            },
            "application_log_root": {f"/var/log/{application_name}"},
            "application_backup_root": {f"/var/backups/{application_name}"},
        }
        for key, expected in expected_paths.items():
            require(
                variables.get(key) in expected,
                prefix + f"{key} is outside the managed application layout",
                errors,
            )

    server_name = variables.get("server_name")
    require(
        isinstance(server_name, str) and bool(SERVER_NAME.fullmatch(server_name)),
        prefix + "server_name is invalid",
        errors,
    )

    ansible_host = variables.get("ansible_host")
    require(
        isinstance(ansible_host, str)
        and bool(ansible_host)
        and not any(character.isspace() for character in ansible_host),
        prefix + "ansible_host is invalid",
        errors,
    )
    ansible_port = variables.get("ansible_port")
    require(
        isinstance(ansible_port, int) and 1 <= ansible_port <= 65535,
        prefix + "ansible_port must be an integer from 1 to 65535",
        errors,
    )
    if enabled:
        require(
            not str(ansible_host).endswith(".invalid"),
            prefix + "enabled instance still uses an example .invalid host",
            errors,
        )
        public_keys = variables.get("deployment_public_keys")
        require(
            isinstance(public_keys, list)
            and bool(public_keys)
            and all(isinstance(key, str) and key.startswith("ssh-") for key in public_keys),
            prefix + "enabled instance requires at least one versioned SSH public key",
            errors,
        )

    deployment_user = variables.get("deployment_user")
    require(
        isinstance(deployment_user, str)
        and bool(re.fullmatch(r"[a-z_][a-z0-9_-]*", deployment_user)),
        prefix + "deployment_user is invalid",
        errors,
    )
    for key in ("application_user", "application_group"):
        value = variables.get(key)
        require(
            isinstance(value, str) and bool(re.fullmatch(r"[a-z_][a-z0-9_-]*", value)),
            prefix + f"{key} is invalid",
            errors,
        )
    service_name = variables.get("application_service_name")
    require(
        isinstance(service_name, str)
        and bool(re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._@-]*", service_name)),
        prefix + "application_service_name is invalid",
        errors,
    )
    backend_url = variables.get("application_backend_url")
    require(
        backend_url == "http://127.0.0.1:5000",
        prefix + "application_backend_url must use the managed loopback endpoint",
        errors,
    )
    backend_executable = variables.get("application_backend_executable")
    require(
        isinstance(backend_executable, str)
        and bool(re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]*", backend_executable)),
        prefix + "application_backend_executable is invalid",
        errors,
    )

    backend_config = variables.get("backend_config")
    require(isinstance(backend_config, dict), prefix + "backend_config must be a mapping", errors)
    if isinstance(backend_config, dict):
        allowed = set(variables.get("allowed_backend_config_keys", []))
        forbidden = set(variables.get("forbidden_backend_config_keys", []))
        actual = set(backend_config)
        secret_keys = sorted(actual & forbidden)
        unknown_keys = sorted(actual - allowed)
        require(not secret_keys, prefix + f"secret keys in backend_config: {secret_keys}", errors)
        require(not unknown_keys, prefix + f"unknown backend_config keys: {unknown_keys}", errors)
        for key, value in backend_config.items():
            require(
                isinstance(value, (str, int, float, bool))
                and "\n" not in str(value)
                and "\r" not in str(value),
                prefix + f"backend_config.{key} must be a single-line scalar",
                errors,
            )

    frontend_api_url = variables.get("frontend_api_url")
    require(
        isinstance(frontend_api_url, str)
        and "\n" not in frontend_api_url
        and "\r" not in frontend_api_url
        and (
            bool(re.fullmatch(r"/[A-Za-z0-9._~!$&'()*+,;=:@%/-]*", frontend_api_url))
            or validate_url(frontend_api_url)
        ),
        prefix + "frontend_api_url is invalid",
        errors,
    )
    health_path = variables.get("health_path")
    require(
        isinstance(health_path, str)
        and bool(re.fullmatch(r"/[A-Za-z0-9/_-]*", health_path)),
        prefix + "health_path must start with /",
        errors,
    )
    return errors


def generate_pipeline(
    inventory: dict[str, Any], application_version: str, configuration_commit: str
) -> dict[str, Any]:
    group = inventory.get("application_instances", {})
    instances = group.get("hosts", [])
    host_variables = inventory.get("_meta", {}).get("hostvars", {})
    errors: list[str] = []

    require(bool(instances), "Inventory group application_instances has no hosts", errors)
    enabled_environments: set[str] = set()
    jobs: dict[str, Any] = {}

    for instance in sorted(instances):
        variables = host_variables.get(instance, {})
        require(bool(INSTANCE_ID.fullmatch(instance)), f"Invalid instance ID: {instance}", errors)
        errors.extend(validate_instance(instance, variables))
        if variables.get("deployment_enabled") is not True:
            continue

        environment = variables["gitlab_environment"]
        require(
            environment not in enabled_environments,
            f"Duplicate enabled GitLab environment: {environment}",
            errors,
        )
        enabled_environments.add(environment)
        jobs[f"deploy:{instance}"] = {
            "extends": ".deploy-application",
            "variables": {
                "APPLICATION_VERSION": application_version,
                "CONFIGURATION_COMMIT": configuration_commit,
                "TARGET_GITLAB_ENVIRONMENT": environment,
                "TARGET_INSTANCE": instance,
                "TARGET_PUBLIC_URL": variables["public_url"],
                "TARGET_DEPLOY_USER": variables["deployment_user"],
            },
        }

    if errors:
        raise ValueError("\n".join(f"- {error}" for error in errors))

    if not jobs:
        jobs["deploy:no-environments"] = {
            "stage": "deploy",
            "image": "alpine:3.23",
            "script": [
                "echo 'No deployment environment is enabled in the Ansible inventory.'"
            ],
        }

    return {
        "include": [{"local": ".gitlab/ci/deploy-job.yml"}],
        "stages": ["deploy"],
        **jobs,
    }


def main() -> None:
    arguments = parse_arguments()
    if not STABLE_VERSION.fullmatch(arguments.application_version):
        raise ValueError("application-version must be a stable tag such as v1.2.3")
    if not COMMIT_SHA.fullmatch(arguments.configuration_commit):
        raise ValueError("configuration-commit must be a full lowercase Git commit SHA")

    inventory = load_inventory(arguments.inventory)
    pipeline = generate_pipeline(
        inventory, arguments.application_version, arguments.configuration_commit
    )
    arguments.output.parent.mkdir(parents=True, exist_ok=True)
    arguments.output.write_text(json.dumps(pipeline, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
