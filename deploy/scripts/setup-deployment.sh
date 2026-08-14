#!/usr/bin/env bash

# Create the isolated deployment virtual environment and install the pinned Ansible toolchain.

set -Eeuo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd "${script_dir}/../.." && pwd)"
venv_path="${repository_root}/.venv-deploy"
requirements_path="${repository_root}/deploy/ansible/requirements-ci.txt"
ansible_only=false

usage() {
  echo "Usage: bash deploy/scripts/setup-deployment.sh [--ansible-only]"
}

for argument in "$@"; do
  case "$argument" in
    --ansible-only)
      ansible_only=true
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
  echo "Deployment tooling must run on Linux or WSL, not Git Bash or native Windows Python." >&2
  exit 1
fi

require_command() {
  local command_name="$1"
  local install_hint="$2"
  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Missing prerequisite: ${command_name}. ${install_hint}" >&2
    return 1
  fi
}

missing_prerequisite=false
require_command python3 "Install Python 3.12-3.14 and the matching python3-venv package." || missing_prerequisite=true

if [[ "$ansible_only" == false ]]; then
  require_command node "Install a Node.js version supported by package.json inside Linux/WSL." || missing_prerequisite=true
  require_command ssh "Install openssh-client." || missing_prerequisite=true
  require_command ssh-keygen "Install openssh-client." || missing_prerequisite=true
  require_command tar "Install tar." || missing_prerequisite=true
  require_command sha256sum "Install coreutils." || missing_prerequisite=true
fi

if [[ "$missing_prerequisite" == true ]]; then
  exit 1
fi

python_version="$(python3 -c 'import sys; print(".".join(map(str, sys.version_info[:3])))')"
if ! python3 -c 'import sys; raise SystemExit(0 if (3, 12) <= sys.version_info[:2] <= (3, 14) else 1)'; then
  echo "Unsupported control-node Python ${python_version}; pinned ansible-core requires Python 3.12-3.14." >&2
  exit 1
fi

if [[ -L "$venv_path" ]]; then
  echo "Refusing to use symlinked deployment environment: ${venv_path}" >&2
  exit 1
fi

if [[ ! -x "${venv_path}/bin/python" ]]; then
  echo "Creating ${venv_path} with Python ${python_version}..."
  if ! python3 -m venv "$venv_path"; then
    echo "Could not create the virtual environment. On Ubuntu/Debian install python3-venv and retry." >&2
    exit 1
  fi
else
  echo "Using existing ${venv_path}."
fi

echo "Installing pinned deployment tooling..."
PIP_DISABLE_PIP_VERSION_CHECK=1 "${venv_path}/bin/python" -m pip install --requirement "$requirements_path"

echo
"${venv_path}/bin/ansible-playbook" --version | head -n 1
"${venv_path}/bin/ansible-lint" --version
echo "Deployment tooling is ready."
echo "Run: npm run validate:deployment"
