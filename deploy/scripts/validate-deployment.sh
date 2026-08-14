#!/usr/bin/env bash

# Validate inventory, generated GitLab jobs, playbooks, lint, and the optional package contract.

set -Eeuo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd "${script_dir}/../.." && pwd)"
venv_path="${repository_root}/.venv-deploy"
inventory_path="deploy/ansible/inventory/hosts.yml"
skip_package=false

usage() {
  echo "Usage: bash deploy/scripts/validate-deployment.sh [--skip-package]"
}

for argument in "$@"; do
  case "$argument" in
    --skip-package)
      skip_package=true
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      usage >&2
      echo "Unknown argument: $argument" >&2
      exit 2
      ;;
  esac
done

if [[ "$(uname -s)" != "Linux" ]]; then
  echo "Deployment validation must run on Linux or WSL." >&2
  exit 1
fi

required_executables=(python ansible-inventory ansible-playbook ansible-lint)
for executable in "${required_executables[@]}"; do
  if [[ ! -x "${venv_path}/bin/${executable}" ]]; then
    echo "Missing ${venv_path}/bin/${executable}. Run npm run setup:deployment first." >&2
    exit 1
  fi
done

temporary_root="$(mktemp -d "${TMPDIR:-/tmp}/deployment-validation.XXXXXX")"
cleanup() {
  case "$temporary_root" in
    "${TMPDIR:-/tmp}"/deployment-validation.*)
      rm -rf -- "$temporary_root"
      ;;
    *)
      echo "Refusing to clean unexpected validation path: ${temporary_root}" >&2
      ;;
  esac
}
trap cleanup EXIT

cd "$repository_root"
export ANSIBLE_CONFIG="${repository_root}/deploy/ansible/ansible.cfg"
export PATH="${venv_path}/bin:${PATH}"
export PYTHONPYCACHEPREFIX="${temporary_root}/pycache"

echo "Validating resolved Ansible inventory..."
"${venv_path}/bin/ansible-inventory" --inventory "$inventory_path" --list > "${temporary_root}/inventory.json"

echo "Validating deployment pipeline generator and GitLab YAML..."
"${venv_path}/bin/python" deploy/scripts/generate-deployment-pipeline.py \
  --inventory "$inventory_path" \
  --application-version v0.0.0 \
  --configuration-commit 0000000000000000000000000000000000000000 \
  --output "${temporary_root}/deployment-pipeline.yml"
"${venv_path}/bin/python" -m py_compile deploy/scripts/generate-deployment-pipeline.py
"${venv_path}/bin/python" deploy/scripts/validate-gitlab-ci.py
"${venv_path}/bin/python" -m unittest discover --start-directory deploy/scripts/tests --verbose

echo "Checking Ansible playbooks..."
for playbook in deploy/ansible/playbooks/*.yml; do
  "${venv_path}/bin/ansible-playbook" --inventory "$inventory_path" --syntax-check "$playbook"
done
"${venv_path}/bin/ansible-lint" deploy/ansible/playbooks/*.yml

if [[ "$skip_package" == false ]]; then
  echo "Checking deterministic package contract..."
  bash deploy/scripts/test-package.sh
fi

echo "Deployment validation passed."
