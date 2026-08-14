from __future__ import annotations

import unittest
from pathlib import Path

import yaml


REPOSITORY_ROOT = Path(__file__).resolve().parents[3]


def load_yaml(path: str) -> dict:
    document = yaml.safe_load((REPOSITORY_ROOT / path).read_text(encoding="utf-8"))
    assert isinstance(document, dict)
    return document


class GitLabCiContractTests(unittest.TestCase):
    def test_deployment_job_uses_repository_setup_and_validation_scripts(self) -> None:
        verification = load_yaml(".gitlab/ci/verify.yml")
        deployment_job = verification["deployment:verify"]

        self.assertEqual(
            ["bash deploy/scripts/setup-deployment.sh --ansible-only"],
            deployment_job["before_script"],
        )
        self.assertEqual(
            ["bash deploy/scripts/validate-deployment.sh --skip-package"],
            deployment_job["script"],
        )

        github_ci = (REPOSITORY_ROOT / ".github/workflows/ci.yml").read_text(
            encoding="utf-8"
        )
        self.assertIn("run: npm run setup:deployment", github_ci)
        self.assertIn("run: npm run validate:deployment", github_ci)

    def test_e2e_is_not_a_ci_or_release_gate(self) -> None:
        verification = load_yaml(".gitlab/ci/verify.yml")
        release = load_yaml(".gitlab/ci/release.yml")

        self.assertNotIn("e2e:verify", verification)
        self.assertNotIn("e2e:verify", str(release))

        github_ci = (REPOSITORY_ROOT / ".github/workflows/ci.yml").read_text(
            encoding="utf-8"
        )
        self.assertNotIn("\n  e2e:\n", github_ci)

    def test_stable_tag_creates_release_from_versioned_notes(self) -> None:
        release = load_yaml(".gitlab/ci/release.yml")

        package_job = release["release:package"]
        self.assertTrue(
            any("extract-changelog.mjs" in command for command in package_job["script"])
        )

        release_job = release["release:create"]
        self.assertEqual(".stable-tag-rules", release_job["extends"])
        self.assertEqual("$CI_COMMIT_TAG", release_job["release"]["tag_name"])
        self.assertEqual(
            "artifacts/package/release-notes.md",
            release_job["release"]["description"],
        )
        self.assertEqual(3, len(release_job["release"]["assets"]["links"]))

        required_jobs = {
            dependency["job"] for dependency in release_job["needs"]
        }
        self.assertEqual({"release:package", "release:publish"}, required_jobs)


if __name__ == "__main__":
    unittest.main()
