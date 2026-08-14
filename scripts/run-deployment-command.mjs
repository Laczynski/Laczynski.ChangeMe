#!/usr/bin/env node

/** Routes deployment setup and validation to Linux Bash, using the default WSL distribution on Windows. */

import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = join(dirname(fileURLToPath(import.meta.url)), "..");
const action = process.argv[2];
const forwardedArguments = process.argv.slice(3);
const scripts = {
  setup: "deploy/scripts/setup-deployment.sh",
  validate: "deploy/scripts/validate-deployment.sh",
};

if (!(action in scripts)) {
  console.error(
    "Usage: node scripts/run-deployment-command.mjs <setup|validate> [arguments]",
  );
  process.exit(2);
}

function finish(result, missingMessage) {
  if (result.error?.code === "ENOENT") {
    console.error(missingMessage);
    process.exit(1);
  }

  if (result.error) {
    console.error(result.error.message);
    process.exit(1);
  }

  process.exit(result.status ?? 1);
}

if (process.platform === "win32") {
  const translatedPath = spawnSync(
    "wsl.exe",
    ["--exec", "wslpath", "-a", repositoryRoot],
    { encoding: "utf8", shell: false },
  );

  if (translatedPath.error?.code === "ENOENT" || translatedPath.status !== 0) {
    console.error(
      "A working default WSL distribution is required. Install WSL with `wsl --install`, launch the distribution once, and rerun this command.",
    );
    process.exit(1);
  }

  const bashCheck = spawnSync("wsl.exe", ["--exec", "bash", "-lc", "exit 0"], {
    encoding: "utf8",
    shell: false,
  });
  if (bashCheck.error?.code === "ENOENT" || bashCheck.status !== 0) {
    console.error(
      "A working default WSL distribution is required. Install WSL with `wsl --install`, launch the distribution once, and rerun this command.",
    );
    process.exit(1);
  }

  const wslRepositoryRoot = translatedPath.stdout.trim();
  if (!wslRepositoryRoot.startsWith("/")) {
    console.error("WSL did not return a valid Linux repository path.");
    process.exit(1);
  }

  console.log(
    `Running deployment ${action} inside the default WSL distribution...`,
  );
  const result = spawnSync(
    "wsl.exe",
    [
      "--exec",
      "bash",
      "-lc",
      'cd -- "$1" && shift && exec bash "$@"',
      "deployment-command",
      wslRepositoryRoot,
      scripts[action],
      ...forwardedArguments,
    ],
    { cwd: repositoryRoot, stdio: "inherit", shell: false },
  );
  finish(
    result,
    "WSL is not available. Install and initialize a Linux distribution, then rerun this command.",
  );
}

if (process.platform !== "linux") {
  console.error(
    "Local deployment tooling is supported on Linux or Windows through WSL.",
  );
  process.exit(1);
}

const result = spawnSync("bash", [scripts[action], ...forwardedArguments], {
  cwd: repositoryRoot,
  stdio: "inherit",
  shell: false,
});
finish(result, "Bash is required to run the deployment tooling.");
