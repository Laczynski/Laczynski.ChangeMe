/**
 * Validate all repository documentation. Common rules are checked here;
 * requirements-validate.mjs remains the internal specialized validator.
 */

import { spawnSync } from "node:child_process";
import { existsSync, readFileSync, readdirSync } from "node:fs";
import path from "node:path";

const root = process.cwd();
const docsRoot = path.join(root, "docs");
const errors = [];

function walkMarkdown(directory) {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const absolute = path.join(directory, entry.name);
    if (entry.isDirectory()) return walkMarkdown(absolute);
    return entry.isFile() && entry.name.endsWith(".md") ? [absolute] : [];
  });
}

const docsFiles = walkMarkdown(docsRoot);
const linkedFiles = [
  path.join(root, "README.md"),
  path.join(root, "AGENTS.md"),
  ...(existsSync(path.join(root, "CONTRIBUTING.md"))
    ? [path.join(root, "CONTRIBUTING.md")]
    : []),
  path.join(
    root,
    "src",
    "ChangeMe.Backend",
    "tools",
    "ChangeMe.Backend.DataGenerator",
    "README.md",
  ),
  ...docsFiles,
];

const linkPattern = /\[[^\]]*\]\(([^)]+)\)/g;

for (const file of linkedFiles) {
  const content = readFileSync(file, "utf8");

  for (const match of content.matchAll(linkPattern)) {
    let target = match[1].trim();
    if (target.startsWith("<") && target.endsWith(">")) {
      target = target.slice(1, -1);
    }

    if (/^(https?:\/\/|mailto:|tel:|#)/.test(target)) continue;

    const [encodedPath] = target.split("#", 1);
    if (!encodedPath) continue;

    let decodedPath;
    try {
      decodedPath = decodeURIComponent(encodedPath);
    } catch {
      errors.push(
        `${path.relative(root, file)}: invalid link encoding: ${target}`,
      );
      continue;
    }

    const resolved = decodedPath.startsWith("/")
      ? path.join(root, decodedPath.slice(1))
      : path.resolve(path.dirname(file), decodedPath);

    if (!existsSync(resolved)) {
      errors.push(
        `${path.relative(root, file)}: missing link target: ${target}`,
      );
    }
  }
}

const index = readFileSync(path.join(docsRoot, "README.md"), "utf8");
const ownedDocs = docsFiles.filter((file) => {
  const relative = path.relative(docsRoot, file).replaceAll(path.sep, "/");
  return relative.startsWith("system/") || relative.startsWith("modules/");
});

const metadata = ["Type", "Scope", "Status", "Canonical for"];
const fileNamePattern = /^[a-z0-9]+(?:-[a-z0-9]+)*\.md$/;
const allowedTypes = new Set([
  "architecture",
  "design",
  "development",
  "operations",
  "reference",
]);
const allowedStatusesByType = new Map([
  ["architecture", new Set(["implemented", "superseded"])],
  ["design", new Set(["proposed", "implemented", "superseded"])],
  ["development", new Set(["implemented", "superseded"])],
  ["operations", new Set(["implemented", "superseded"])],
  ["reference", new Set(["implemented", "superseded"])],
]);

for (const file of ownedDocs) {
  const relative = path.relative(docsRoot, file).replaceAll(path.sep, "/");
  const content = readFileSync(file, "utf8");

  if (!index.includes(relative)) {
    errors.push(`${relative}: not reachable from docs/README.md`);
  }

  if (!content.startsWith("# ")) {
    errors.push(`${relative}: must start with one H1 title`);
  }

  for (const field of metadata) {
    if (!new RegExp(`^> ${field}: .+`, "m").test(content)) {
      errors.push(`${relative}: missing metadata field '${field}'`);
    }
  }

  if (
    path.basename(file) !== "README.md" &&
    !fileNamePattern.test(path.basename(file))
  ) {
    errors.push(`${relative}: file name must use lowercase kebab-case`);
  }

  const type = content.match(/^> Type: (.+)$/m)?.[1].trim();
  if (type && !allowedTypes.has(type)) {
    errors.push(`${relative}: unsupported Type '${type}'`);
  }

  const scope = content.match(/^> Scope: (.+)$/m)?.[1].trim();
  const pathParts = relative.split("/");
  const expectedScope = pathParts[0] === "system" ? "system" : pathParts[1];
  if (scope && scope !== expectedScope) {
    errors.push(
      `${relative}: Scope '${scope}' does not match owner '${expectedScope}'`,
    );
  }

  const status = content.match(/^> Status: (.+)$/m)?.[1].trim();
  if (type && status && !allowedStatusesByType.get(type)?.has(status)) {
    errors.push(
      `${relative}: Status '${status}' is not allowed for Type '${type}'`,
    );
  }

  if (type === "architecture") {
    for (const section of ["Summary", "Verification"]) {
      if (!new RegExp(`^## ${section}$`, "m").test(content)) {
        errors.push(`${relative}: architecture requires '## ${section}'`);
      }
    }
    if (!content.includes("```mermaid")) {
      errors.push(`${relative}: architecture requires a Mermaid diagram`);
    }
  }

  if (type === "design") {
    if (!/^## Goal$/m.test(content)) {
      errors.push(`${relative}: design requires '## Goal'`);
    }
    if (!/^## (Decision|Target|Plan|Phases)\b/m.test(content)) {
      errors.push(
        `${relative}: design requires a Decision, Target, Plan, or Phases section`,
      );
    }
  }
}

if (errors.length > 0) {
  console.error("Documentation validation failed:\n");
  for (const error of errors.sort()) console.error(`- ${error}`);
  process.exit(1);
}

console.log(
  `Documentation valid: ${docsFiles.length} files, ${ownedDocs.length} system/module documents indexed`,
);

const generatedRequirementFiles = [
  path.join(docsRoot, "requirements", ".requirements-manifest.json"),
  path.join(docsRoot, "requirements", "README.md"),
];
const generatedRequirementsBefore = new Map(
  generatedRequirementFiles.map((file) => [
    file,
    existsSync(file) ? readFileSync(file, "utf8") : null,
  ]),
);

const requirementsValidator = spawnSync(
  process.execPath,
  [path.join(root, "scripts", "requirements-validate.mjs")],
  { cwd: root, stdio: "inherit" },
);

if (requirementsValidator.error) {
  console.error(requirementsValidator.error.message);
  process.exit(1);
}

if (requirementsValidator.status !== 0) {
  process.exit(requirementsValidator.status ?? 1);
}

const regeneratedFiles = generatedRequirementFiles.filter(
  (file) =>
    generatedRequirementsBefore.get(file) !== readFileSync(file, "utf8"),
);

if (regeneratedFiles.length > 0) {
  console.error(
    "\nGenerated requirements files were stale and have been updated:",
  );
  for (const file of regeneratedFiles) {
    console.error(`- ${path.relative(root, file)}`);
  }
  console.error(
    "Review the generated changes, then run npm run docs:validate again.",
  );
  process.exit(1);
}
