#!/usr/bin/env node
import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");

function spawnCommand(command, args, options = {}) {
  if (
    process.platform === "win32" &&
    (command === "npm" || command === "npx")
  ) {
    return spawnSync(
      "cmd.exe",
      ["/d", "/s", "/c", [command, ...args].join(" ")],
      {
        ...options,
        shell: false,
      },
    );
  }

  return spawnSync(command, args, {
    ...options,
    shell: false,
  });
}

function run(command, args) {
  const result = spawnCommand(command, args, {
    cwd: root,
    stdio: "inherit",
  });

  if (result.status !== 0) {
    process.exit(result.status ?? 1);
  }
}

function checkPrerequisite(command, args, label) {
  const result = spawnCommand(command, args, {
    cwd: root,
    encoding: "utf8",
  });

  if (result.status !== 0) {
    console.error(`Missing prerequisite: ${label}`);
    process.exit(1);
  }

  const version = (result.stdout ?? result.stderr ?? "").trim().split("\n")[0];
  console.log(`✓ ${label}${version ? ` (${version})` : ""}`);
}

console.log("ChangeMe setup\n");

checkPrerequisite("node", ["--version"], "Node.js");
checkPrerequisite("dotnet", ["--version"], ".NET SDK");

const dockerCheck = spawnCommand(
  "docker",
  ["version", "--format", "{{.Server.Version}}"],
  {
    cwd: root,
    encoding: "utf8",
  },
);

if (dockerCheck.status === 0) {
  console.log(`✓ Docker (${dockerCheck.stdout.trim()})`);
} else {
  console.warn(
    "! Docker not detected — integration tests and E2E need a running Docker engine.",
  );
}

console.log("\nInstalling dependencies...");
run("npm", ["install"]);
run("npm", ["run", "install:frontend"]);
run("dotnet", ["restore", "src/ChangeMe.Backend/ChangeMe.Backend.slnx"]);

console.log("\nSetup complete.");
console.log("Next steps:");
console.log("  - PostgreSQL: docker compose up postgres mailhog -d");
console.log("  - Dev servers: npm run start:all");
console.log("  - Full stack:  npm run docker:up");
