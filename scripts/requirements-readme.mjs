#!/usr/bin/env node

/** Regenerates the requirements index from the manifest produced by requirements validation. */

import fs from "node:fs";
import path from "node:path";
import { ROOT, formatDomainLabel } from "./requirements-lib.mjs";

const manifestPath = path.join(ROOT, ".requirements-manifest.json");
const manifest = fs.existsSync(manifestPath)
  ? JSON.parse(fs.readFileSync(manifestPath, "utf8"))
  : {
      functional: [],
      quality: [],
      conventions: [],
      domain: [],
    };

const byDomain = {};
for (const spec of manifest.functional ?? []) {
  (byDomain[spec.domain] ??= []).push(spec);
}

let md = `# Requirements index

> Five layers: **Domain** · **Conventions** · **Quality** · **Capabilities (FR-*)** · **Implementation (module and system docs)**.
> Layer index: \`_shared/README.md\`. Start: \`requirements-change-process.md\`. Authoring: \`requirements-authoring-guide.md\`. Validate: \`npm run docs:validate\`.

## L1 — Domain (\`_shared/domain/\`)

| Document | File |
| -------- | ---- |
`;

for (const doc of manifest.domain ?? []) {
  const fileName = doc.file.split("/").pop();
  if (fileName === "README.md") {
    md += `| Layer index | [README.md](${doc.file}) |\n`;
    continue;
  }
  md += `| ${fileName} | [${fileName}](${doc.file}) |\n`;
}

md += `
## L2 — Conventions (\`_shared/conventions/\`)

| ID | Title | File |
| -- | ----- | ---- |
`;

for (const s of manifest.conventions ?? []) {
  const fileName = s.file.split("/").pop();
  md += `| ${s.id} | ${s.title || fileName} | [${fileName}](${s.file}) |\n`;
}

md += `
| — | [STD index & checklist](_shared/conventions/README.md) | [README.md](_shared/conventions/README.md) |

## L3 — Quality (\`_shared/quality/\`)

| ID | Title | File |
| -- | ----- | ---- |
`;

for (const n of manifest.quality ?? []) {
  const fileName = n.file.split("/").pop();
  md += `| ${n.id} | ${n.title || fileName} | [${fileName}](${n.file}) |\n`;
}

md += `| — | Quality index | [README.md](_shared/quality/README.md) |\n`;

md += `
## Pending changes

See [changes/](changes/) for open requirement deltas.

`;

const domainOrder = Object.keys(byDomain).sort((a, b) => a.localeCompare(b));

for (const domain of domainOrder) {
  const specs = byDomain[domain];
  if (!specs?.length) continue;
  const label = formatDomainLabel(domain);
  md += `\n## L4 — ${label} (\`functional/${domain}/\`)\n\n| ID | Title | File |\n| -- | ----- | ---- |\n`;
  for (const s of specs.sort((a, b) => a.id.localeCompare(b.id))) {
    const fileName = s.file.split("/").pop();
    md += `| ${s.id} | ${s.title} | [${fileName}](${s.file}) |\n`;
  }
}

md += `\n---\n\n_Auto-generated from \`.requirements-manifest.json\` via the internal requirements validator (refreshed by \`npm run docs:validate\`)._\n`;

fs.writeFileSync(path.join(ROOT, "README.md"), md);
console.log("Wrote docs/requirements/README.md");
