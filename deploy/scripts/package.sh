#!/usr/bin/env bash

set -Eeuo pipefail

if [[ $# -ne 9 ]]; then
  echo "Usage: $0 <application> <version> <commit-sha> <pipeline-url> <source-date-epoch> <migration-id> <backend-dir> <frontend-dir> <output-dir>" >&2
  exit 2
fi

application="$1"
version="$2"
commit_sha="$3"
pipeline_url="$4"
source_date_epoch="$5"
migration_id="$6"
backend_dir="$7"
frontend_dir="$8"
output_dir="$9"
runtime_identifier="linux-x64"

if [[ ! "$application" =~ ^[a-z0-9][a-z0-9._-]*$ ]]; then
  echo "Application package name must contain only lowercase letters, numbers, dots, underscores, and hyphens." >&2
  exit 2
fi

if [[ ! "$version" =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "Version must be a stable tag such as v1.2.3." >&2
  exit 2
fi

if [[ ! "$commit_sha" =~ ^[0-9a-fA-F]{40}$ ]]; then
  echo "Commit SHA must contain exactly 40 hexadecimal characters." >&2
  exit 2
fi

if [[ ! "$source_date_epoch" =~ ^[0-9]+$ ]]; then
  echo "Source date epoch must be an integer." >&2
  exit 2
fi

for required_command in node tar gzip sha256sum find sort xargs realpath; do
  if ! command -v "$required_command" >/dev/null 2>&1; then
    echo "Required command is unavailable: $required_command" >&2
    exit 1
  fi
done

if [[ ! -d "$backend_dir" || ! -d "$frontend_dir" ]]; then
  echo "Backend and frontend publish directories must exist." >&2
  exit 1
fi

backend_dir="$(realpath "$backend_dir")"
frontend_dir="$(realpath "$frontend_dir")"
mkdir -p "$output_dir"
output_dir="$(realpath "$output_dir")"

archive_name="${application}-${version}-${runtime_identifier}.tar.gz"
archive_path="${output_dir}/${archive_name}"
checksum_path="${archive_path}.sha256"
manifest_copy_path="${archive_path}.manifest.json"

for output_path in "$archive_path" "$checksum_path" "$manifest_copy_path"; do
  if [[ -e "$output_path" ]]; then
    echo "Refusing to overwrite immutable package output: $output_path" >&2
    exit 1
  fi
done

staging_dir="$(mktemp -d "${output_dir}/.package.XXXXXX")"

cleanup() {
  case "$staging_dir" in
    "${output_dir}"/.package.*)
      rm -rf -- "$staging_dir"
      ;;
    *)
      echo "Refusing to clean unexpected staging path: $staging_dir" >&2
      ;;
  esac
}
trap cleanup EXIT

mkdir -p "${staging_dir}/backend" "${staging_dir}/frontend"
cp -a "${backend_dir}/." "${staging_dir}/backend/"
cp -a "${frontend_dir}/." "${staging_dir}/frontend/"
rm -f -- "${staging_dir}/backend/appsettings.Development.json"

export PACKAGE_APPLICATION="$application"
export PACKAGE_VERSION="$version"
export PACKAGE_COMMIT_SHA="$commit_sha"
export PACKAGE_PIPELINE_URL="$pipeline_url"
export PACKAGE_SOURCE_DATE_EPOCH="$source_date_epoch"
export PACKAGE_MIGRATION_ID="$migration_id"
export PACKAGE_RUNTIME_IDENTIFIER="$runtime_identifier"
export PACKAGE_MANIFEST_PATH="${staging_dir}/manifest.json"

node --input-type=module <<'NODE'
import { writeFileSync } from 'node:fs';

const manifest = {
  schemaVersion: 1,
  application: process.env.PACKAGE_APPLICATION,
  version: process.env.PACKAGE_VERSION,
  commitSha: process.env.PACKAGE_COMMIT_SHA,
  runtimeIdentifier: process.env.PACKAGE_RUNTIME_IDENTIFIER,
  buildTimestamp: new Date(Number(process.env.PACKAGE_SOURCE_DATE_EPOCH) * 1000).toISOString(),
  pipelineUrl: process.env.PACKAGE_PIPELINE_URL,
  migrationId: process.env.PACKAGE_MIGRATION_ID
};

writeFileSync(process.env.PACKAGE_MANIFEST_PATH, `${JSON.stringify(manifest, null, 2)}\n`, 'utf8');
NODE

(
  cd "$staging_dir"
  find backend frontend -type f -print0 \
    | LC_ALL=C sort -z \
    | xargs -0 sha256sum > checksums.sha256
)

cp "${staging_dir}/manifest.json" "$manifest_copy_path"

(
  cd "$staging_dir"
  tar \
    --sort=name \
    --mtime="@${source_date_epoch}" \
    --owner=0 \
    --group=0 \
    --numeric-owner \
    -cf - \
    backend frontend manifest.json checksums.sha256
) | gzip -n > "$archive_path"

(
  cd "$output_dir"
  sha256sum "$archive_name" > "$(basename "$checksum_path")"
)

echo "$archive_path"
