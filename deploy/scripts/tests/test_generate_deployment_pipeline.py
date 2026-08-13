from __future__ import annotations

import importlib.util
import unittest
from pathlib import Path


SCRIPT_PATH = Path(__file__).resolve().parents[1] / "generate-deployment-pipeline.py"
SPEC = importlib.util.spec_from_file_location("generate_deployment_pipeline", SCRIPT_PATH)
assert SPEC is not None and SPEC.loader is not None
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


class GenerateDeploymentPipelineTests(unittest.TestCase):
    def test_enabled_instance_generates_manual_environment_job(self) -> None:
        inventory = self.create_inventory(enabled=True)

        pipeline = MODULE.generate_pipeline(
            inventory,
            "v1.2.3",
            "0123456789abcdef0123456789abcdef01234567",
        )

        job = pipeline["deploy:development"]
        self.assertEqual(".deploy-application", job["extends"])
        self.assertEqual("development", job["variables"]["TARGET_GITLAB_ENVIRONMENT"])
        self.assertEqual("deploy", job["variables"]["TARGET_DEPLOY_USER"])
        self.assertEqual("v1.2.3", job["variables"]["APPLICATION_VERSION"])

    def test_no_enabled_instance_generates_non_deploying_information_job(self) -> None:
        inventory = self.create_inventory(enabled=False)

        pipeline = MODULE.generate_pipeline(
            inventory,
            "v1.2.3",
            "0123456789abcdef0123456789abcdef01234567",
        )

        self.assertIn("deploy:no-environments", pipeline)
        self.assertNotIn("deploy:development", pipeline)

    def test_secret_key_in_versioned_configuration_is_rejected(self) -> None:
        inventory = self.create_inventory(enabled=True)
        variables = inventory["_meta"]["hostvars"]["development"]
        variables["backend_config"]["AuthOptions__Jwt__SigningKey"] = "must-not-be-versioned"

        with self.assertRaisesRegex(ValueError, "secret keys in backend_config"):
            MODULE.generate_pipeline(
                inventory,
                "v1.2.3",
                "0123456789abcdef0123456789abcdef01234567",
            )

    def test_managed_path_override_is_rejected(self) -> None:
        inventory = self.create_inventory(enabled=True)
        variables = inventory["_meta"]["hostvars"]["development"]
        variables["application_current_path"] = "/opt/unmanaged/current"

        with self.assertRaisesRegex(ValueError, "application_current_path is outside"):
            MODULE.generate_pipeline(
                inventory,
                "v1.2.3",
                "0123456789abcdef0123456789abcdef01234567",
            )

    @staticmethod
    def create_inventory(enabled: bool) -> dict:
        application_name = "example"
        variables = {
            "allowed_backend_config_keys": ["AuthOptions__Jwt__Issuer"],
            "ansible_host": "development.example.test",
            "ansible_port": 22,
            "application_backup_root": f"/var/backups/{application_name}",
            "application_backend_executable": "Example.Backend.Web",
            "application_backend_url": "http://127.0.0.1:5000",
            "application_config_revisions_root": f"/etc/{application_name}/config-revisions",
            "application_config_root": f"/etc/{application_name}",
            "application_current_config_path": f"/etc/{application_name}/current-config",
            "application_current_path": f"/opt/{application_name}/current",
            "application_data_root": f"/var/lib/{application_name}",
            "application_group": "appsvc",
            "application_log_root": f"/var/log/{application_name}",
            "application_name": application_name,
            "application_previous_config_path": f"/etc/{application_name}/previous-config",
            "application_previous_path": f"/opt/{application_name}/previous",
            "application_releases_root": f"/opt/{application_name}/releases",
            "application_root": f"/opt/{application_name}",
            "application_secrets_path": f"/etc/{application_name}/secrets.env",
            "application_service_name": f"{application_name}-backend",
            "application_storage_root": f"/var/lib/{application_name}/storage",
            "application_user": "appsvc",
            "backend_config": {"AuthOptions__Jwt__Issuer": "Example"},
            "deployment_enabled": enabled,
            "deployment_public_keys": ["ssh-ed25519 test-only-key"] if enabled else [],
            "deployment_tier": "development",
            "deployment_user": "deploy",
            "forbidden_backend_config_keys": ["AuthOptions__Jwt__SigningKey"],
            "frontend_api_url": "/api/v1",
            "gitlab_environment": "development",
            "health_path": "/health",
            "public_url": "https://development.example.test",
            "server_name": "development.example.test",
        }
        return {
            "application_instances": {"hosts": ["development"]},
            "_meta": {"hostvars": {"development": variables}},
        }


if __name__ == "__main__":
    unittest.main()
