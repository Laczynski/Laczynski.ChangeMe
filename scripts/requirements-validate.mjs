#!/usr/bin/env node
/**
 * Validate docs/requirements structure: FR specs, quality docs, conventions, cross-references.
 * Internal validator invoked by `npm run docs:validate`.
 */

import { execSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import {
  ROOT,
  collectConventionDocs,
  collectDomainDocs,
  collectFunctionalFiles,
  collectQualityDocs,
  collectStdIdsFromConventions,
  parseFrontmatter,
} from "./requirements-lib.mjs";

const errors = [];
const warnings = [];

function error(msg) {
  errors.push(msg);
}

function warn(msg) {
  warnings.push(msg);
}

function validateSharedDoc(doc, idPattern) {
  const rel = doc.relPath;
  if (!doc.id) {
    error(`Missing id in frontmatter: ${rel}`);
    return false;
  }
  if (!idPattern.test(doc.id)) {
    error(`Invalid id "${doc.id}" in frontmatter: ${rel}`);
    return false;
  }
  if (doc.type && doc.expectedType && doc.type !== doc.expectedType) {
    warn(`Expected type: ${doc.expectedType} in ${rel}`);
  }
  if (!doc.title) {
    warn(`Missing title in frontmatter: ${rel}`);
  }
  return true;
}

function main() {
  if (!fs.existsSync(ROOT)) {
    error("Missing docs/requirements/");
    report();
    return;
  }

  const domainDocs = collectDomainDocs();
  if (domainDocs.length === 0) {
    warn("No domain docs found under docs/requirements/_shared/domain/");
  }

  const qualityDocs = collectQualityDocs();
  if (qualityDocs.length === 0) {
    warn("No quality docs found under docs/requirements/_shared/quality/");
  }

  const conventionDocs = collectConventionDocs();
  if (conventionDocs.length === 0) {
    warn(
      "No convention docs found under docs/requirements/_shared/conventions/",
    );
  }

  const stdIds = collectStdIdsFromConventions();
  if (stdIds.size === 0) {
    warn("No STD-* section ids found in conventions docs");
  }

  const qualityIds = new Set();
  for (const doc of qualityDocs) {
    if (!validateSharedDoc(doc, /^NFR-[A-Z0-9]+-\d{3}$/)) continue;
    if (qualityIds.has(doc.id)) {
      error(`Duplicate quality id ${doc.id}`);
    } else {
      qualityIds.add(doc.id);
    }
  }

  const conventionIds = new Map();
  for (const doc of conventionDocs) {
    if (!validateSharedDoc(doc, /^CONV-\d{3}$/)) continue;
    if (conventionIds.has(doc.id)) {
      error(`Duplicate conventions id ${doc.id}`);
    } else {
      conventionIds.set(doc.id, doc.relPath);
    }
  }

  if (!fs.existsSync(path.join(ROOT, "_change-record-skeleton.md"))) {
    error("Missing docs/requirements/_change-record-skeleton.md");
  }

  const frFiles = collectFunctionalFiles();
  if (frFiles.length === 0) {
    warn(
      "No functional specifications found under docs/requirements/functional/",
    );
  }

  const frIds = new Map();
  const allFrIds = new Set();

  const frFilenamePattern = /^fr-[a-z]+-\d{3}-.+\.md$/;

  for (const { domain, name, path: filePath } of frFiles) {
    if (!frFilenamePattern.test(name)) {
      warn(
        `Non-standard filename (expected fr-<area>-<nnn>-<slug>.md): functional/${domain}/${name}`,
      );
    }

    const content = fs.readFileSync(filePath, "utf8");
    const meta = parseFrontmatter(content);
    if (!meta) {
      error(`Missing YAML frontmatter: functional/${domain}/${name}`);
      continue;
    }

    if (!meta.id || !/^FR-[A-Z0-9]+-\d{3}$/.test(meta.id)) {
      error(
        `Invalid or missing id in frontmatter: functional/${domain}/${name}`,
      );
    }
    if (meta.type !== "functional") {
      warn(`Expected type: functional in functional/${domain}/${name}`);
    }
    if (meta.domain !== domain) {
      error(
        `Frontmatter domain "${meta.domain}" does not match folder "${domain}": ${name}`,
      );
    }
    if (!meta.title) {
      error(`Missing title in frontmatter: functional/${domain}/${name}`);
    }

    if (meta.inherits_fr) {
      warn(
        `Deprecated inherits_fr in functional/${domain}/${name}; use inherits_conventions with STD-* ids`,
      );
    }
    if (meta.inherits_nfr) {
      warn(
        `Deprecated inherits_nfr in functional/${domain}/${name}; use inherits_quality`,
      );
    }

    if (frIds.has(meta.id)) {
      error(
        `Duplicate FR id ${meta.id}: ${frIds.get(meta.id)} and functional/${domain}/${name}`,
      );
    } else {
      frIds.set(meta.id, `functional/${domain}/${name}`);
      allFrIds.add(meta.id);
    }

    const expectedPrefix = meta.id.toLowerCase().replaceAll("_", "-");
    if (!name.startsWith(expectedPrefix)) {
      warn(
        `Filename should start with ${expectedPrefix}: functional/${domain}/${name}`,
      );
    }

    const body = content.replace(/^---[\s\S]*?---\n/, "");
    if (!body.includes("## Functional requirements")) {
      error(`Missing ## Functional requirements: functional/${domain}/${name}`);
    }
    if (
      !body.includes("## Quality requirements") &&
      !body.includes("## Non-functional requirements")
    ) {
      error(`Missing ## Quality requirements: functional/${domain}/${name}`);
    }
  }

  for (const { domain, name, path: filePath } of frFiles) {
    const content = fs.readFileSync(filePath, "utf8");
    const body = content.replace(/^---[\s\S]*?---\n/, "");
    const frRefRe = /(?<![A-Z/])FR-[A-Z0-9]+-\d{3}/g;
    const refs = [...new Set(body.match(frRefRe) ?? [])];
    const meta = parseFrontmatter(content);
    for (const ref of refs) {
      if (!allFrIds.has(ref)) {
        error(`Broken FR reference ${ref} in functional/${domain}/${name}`);
      }
    }
    if (meta?.depends_on) {
      for (const dep of meta.depends_on) {
        if (!allFrIds.has(dep)) {
          error(`depends_on references missing FR ${dep} in ${domain}/${name}`);
        }
      }
    }
    const qualityInherits = meta?.inherits_quality ?? meta?.inherits_nfr ?? [];
    for (const dep of qualityInherits) {
      if (!qualityIds.has(dep)) {
        error(
          `inherits_quality references missing NFR ${dep} in ${domain}/${name}`,
        );
      }
    }
    if (meta?.inherits_conventions) {
      for (const dep of meta.inherits_conventions) {
        if (!stdIds.has(dep)) {
          error(
            `inherits_conventions references missing STD ${dep} in ${domain}/${name}`,
          );
        }
      }
    }
  }

  for (const { domain, name, path: filePath } of frFiles) {
    const content = fs.readFileSync(filePath, "utf8");
    const links = content.match(/`docs\/requirements\/[^`]+`/g) ?? [];
    for (const link of links) {
      const target = link.slice(1, -1);
      if (target.includes("*")) continue;
      const resolved = path.join(process.cwd(), target);
      if (!fs.existsSync(resolved)) {
        error(`Broken path reference ${link} in functional/${domain}/${name}`);
      }
    }
  }

  const changesDir = path.join(ROOT, "changes");
  if (fs.existsSync(changesDir)) {
    for (const name of fs.readdirSync(changesDir)) {
      if (!name.endsWith(".md") || name === "README.md") continue;
      const content = fs.readFileSync(path.join(changesDir, name), "utf8");
      if (!/\*\*Status:\*\*\s*(pending|done)/i.test(content)) {
        warn(`Change record missing Status (pending|done): changes/${name}`);
      }
      const touched = content.match(/(?<![A-Z/])FR-[A-Z0-9]+-\d{3}/g) ?? [];
      for (const ref of [...new Set(touched)]) {
        if (!allFrIds.has(ref)) {
          error(`Change record references missing FR ${ref}: changes/${name}`);
        }
      }
    }
  }

  const manifest = {
    functional: [...frIds.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([id, file]) => {
        const filePath = path.join(ROOT, file);
        const content = fs.readFileSync(filePath, "utf8");
        const meta = parseFrontmatter(content);
        return {
          id,
          title: meta?.title ?? "",
          domain: meta?.domain ?? "",
          type: "functional",
          status: meta?.status ?? "active",
          file,
          depends_on: meta?.depends_on ?? [],
          inherits_quality: meta?.inherits_quality ?? meta?.inherits_nfr ?? [],
          inherits_conventions: meta?.inherits_conventions ?? [],
        };
      }),
    quality: qualityDocs
      .filter((doc) => doc.id)
      .sort((a, b) => a.id.localeCompare(b.id))
      .map((doc) => ({
        id: doc.id,
        title: doc.title,
        file: doc.relPath,
        type: "quality",
        status: doc.status,
      })),
    conventions: conventionDocs
      .filter((doc) => doc.id)
      .sort((a, b) => a.id.localeCompare(b.id))
      .map((doc) => ({
        id: doc.id,
        title: doc.title,
        file: doc.relPath,
        type: "conventions",
        status: doc.status,
      })),
    domain: domainDocs.map((doc) => ({
      file: doc.relPath,
    })),
  };

  fs.writeFileSync(
    path.join(ROOT, ".requirements-manifest.json"),
    JSON.stringify(manifest, null, 2),
  );

  execSync("node scripts/requirements-readme.mjs", { stdio: "pipe" });

  if (!fs.existsSync(path.join(ROOT, "README.md"))) {
    error("Failed to generate docs/requirements/README.md");
  }

  report(
    frIds.size,
    qualityDocs.length,
    domainDocs.length,
    conventionDocs.length,
  );
}

function report(
  frCount = 0,
  qualityCount = 0,
  domainCount = 0,
  conventionCount = 0,
) {
  if (warnings.length) {
    console.warn(`\n${warnings.length} warning(s):`);
    for (const w of warnings) console.warn(`  ⚠ ${w}`);
  }
  if (errors.length) {
    console.error(`\n${errors.length} error(s):`);
    for (const e of errors) console.error(`  ✗ ${e}`);
    process.exit(1);
  }
  console.log(
    `✓ Requirements valid: ${frCount} functional specifications, ${qualityCount} quality documents, ${conventionCount} convention documents, ${domainCount} domain documents`,
  );
}

main();
