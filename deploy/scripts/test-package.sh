#!/usr/bin/env bash

# Build test packages twice and verify their deterministic archive, checksum, and content contract.

set -Eeuo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
package_script="${script_dir}/package.sh"
test_root="$(mktemp -d "${TMPDIR:-/tmp}/deployment-package-test.XXXXXX")"

cleanup() {
  case "$test_root" in
    "${TMPDIR:-/tmp}"/deployment-package-test.*)
      rm -rf -- "$test_root"
      ;;
    *)
      echo "Refusing to clean unexpected package-test path: $test_root" >&2
      ;;
  esac
}
trap cleanup EXIT

mkdir -p \
  "${test_root}/input/backend" \
  "${test_root}/input/frontend" \
  "${test_root}/package-one" \
  "${test_root}/package-two"

printf 'test backend\n' > "${test_root}/input/backend/Template.Backend.Web"
chmod 755 "${test_root}/input/backend/Template.Backend.Web"
printf 'must be removed\n' > "${test_root}/input/backend/appsettings.Development.json"
printf '<html></html>\n' > "${test_root}/input/frontend/index.html"

package_arguments=(
  example
  v1.2.3
  0123456789abcdef0123456789abcdef01234567
  https://gitlab.example/pipelines/1
  1700000000
  abcdef
  "${test_root}/input/backend"
  "${test_root}/input/frontend"
)

bash "$package_script" "${package_arguments[@]}" "${test_root}/package-one" >/dev/null
bash "$package_script" "${package_arguments[@]}" "${test_root}/package-two" >/dev/null

archive_name="example-v1.2.3-linux-x64.tar.gz"
cmp "${test_root}/package-one/${archive_name}" "${test_root}/package-two/${archive_name}"

(
  cd "${test_root}/package-one"
  sha256sum --check --strict "${archive_name}.sha256"
)

if tar -tzf "${test_root}/package-one/${archive_name}" | grep --quiet appsettings.Development.json; then
  echo "Development settings leaked into the deployment package." >&2
  exit 1
fi

node --input-type=module - "${test_root}/package-one/${archive_name}.manifest.json" <<'NODE'
import { readFileSync } from 'node:fs';

const manifest = JSON.parse(readFileSync(process.argv[2], 'utf8'));
if (manifest.version !== 'v1.2.3' || manifest.runtimeIdentifier !== 'linux-x64') {
  throw new Error('Deployment package manifest does not match the expected contract.');
}
NODE

mkdir -p "${test_root}/extracted"
tar -xzf "${test_root}/package-one/${archive_name}" -C "${test_root}/extracted"
(
  cd "${test_root}/extracted"
  sha256sum --check --strict checksums.sha256
  cmp manifest.json "${test_root}/package-one/${archive_name}.manifest.json"
)
