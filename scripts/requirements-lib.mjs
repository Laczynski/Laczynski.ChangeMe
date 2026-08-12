import fs from "node:fs";
import path from "node:path";

export const ROOT = path.join(process.cwd(), "docs", "requirements");
export const FUNCTIONAL_DIR = path.join(ROOT, "functional");

export function parseFrontmatter(content) {
  if (!content.startsWith("---\n")) return null;
  const end = content.indexOf("\n---\n", 4);
  if (end === -1) return null;
  const block = content.slice(4, end);
  const meta = {};
  const lines = block.split("\n");

  for (let index = 0; index < lines.length; index += 1) {
    const m = lines[index].match(/^([\w_]+):\s*(.*)$/);
    if (!m) continue;
    const [, key] = m;
    let raw = m[2].trim();

    if (!raw && /^\s+\S/.test(lines[index + 1] ?? "")) {
      raw = lines[index + 1].trim();
      index += 1;
    }

    if (raw.startsWith("[") && raw.endsWith("]")) {
      meta[key] = raw
        .slice(1, -1)
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);
    } else {
      meta[key] = raw;
    }
  }
  return meta;
}

export function listMarkdownFiles(dir) {
  if (!fs.existsSync(dir)) return [];
  return fs
    .readdirSync(dir)
    .filter((name) => name.endsWith(".md"))
    .sort((a, b) => a.localeCompare(b));
}

export function listDomainDirs() {
  if (!fs.existsSync(FUNCTIONAL_DIR)) return [];
  return fs
    .readdirSync(FUNCTIONAL_DIR, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name)
    .sort((a, b) => a.localeCompare(b));
}

export function collectFunctionalFiles() {
  const files = [];
  for (const domain of listDomainDirs()) {
    const dir = path.join(FUNCTIONAL_DIR, domain);
    for (const name of listMarkdownFiles(dir)) {
      files.push({ domain, name, path: path.join(dir, name) });
    }
  }
  return files;
}

export function collectDomainDocs() {
  const dir = path.join(ROOT, "_shared", "domain");
  return listMarkdownFiles(dir).map((file) => ({
    file,
    relPath: `_shared/domain/${file}`,
    path: path.join(dir, file),
  }));
}

/** @deprecated use collectDomainDocs */
export function collectReferenceDocs() {
  return collectDomainDocs();
}

function collectSharedDocs(subdir, expectedType) {
  const dir = path.join(ROOT, "_shared", subdir);
  return listMarkdownFiles(dir)
    .filter((file) => file !== "README.md")
    .map((file) => {
      const filePath = path.join(dir, file);
      const content = fs.readFileSync(filePath, "utf8");
      const meta = parseFrontmatter(content) ?? {};
      return {
        id: meta.id ?? "",
        title: meta.title ?? "",
        type: meta.type ?? "",
        status: meta.status ?? "active",
        file,
        relPath: `_shared/${subdir}/${file}`,
        path: filePath,
        expectedType,
        content,
      };
    });
}

export function collectQualityDocs() {
  return collectSharedDocs("quality", "quality");
}

/** @deprecated use collectQualityDocs */
export function collectNfrDocs() {
  return collectQualityDocs();
}

export function collectConventionDocs() {
  return collectSharedDocs("conventions", "conventions");
}

export function collectStdIdsFromConventions() {
  const ids = new Set();
  const stdRe = /^## (STD-[A-Z]+-\d{3})\b/gm;
  for (const doc of collectConventionDocs()) {
    for (const m of doc.content.matchAll(stdRe)) {
      ids.add(m[1]);
    }
  }
  return ids;
}

export function formatDomainLabel(domain) {
  return domain
    .split(/[-_]/)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}
