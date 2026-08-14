/** Extracts a non-empty version section from `CHANGELOG.md` for GitLab release notes. */

import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const version = process.argv[2];
if (!version) {
  console.error("Usage: node scripts/extract-changelog.mjs <version>");
  process.exit(1);
}

const changelogPath = join(
  dirname(fileURLToPath(import.meta.url)),
  "..",
  "CHANGELOG.md",
);
const lines = readFileSync(changelogPath, "utf8").split(/\r?\n/);
const headers = [`## ${version}`, `## [${version}]`];
const start = lines.findIndex((line) =>
  headers.some((header) => line.startsWith(header)),
);

if (start === -1) {
  console.error(`No CHANGELOG section found for ${version}`);
  process.exit(1);
}

const body = [];
for (let i = start + 1; i < lines.length; i++) {
  if (lines[i].startsWith("## ")) {
    break;
  }

  body.push(lines[i]);
}

const notes = body.join("\n").trim();
if (!notes) {
  console.error(`CHANGELOG section for ${version} is empty`);
  process.exit(1);
}

process.stdout.write(notes);
