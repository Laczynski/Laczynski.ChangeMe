#!/usr/bin/env node
import { spawnSync } from "node:child_process";

if (
  process.env.CI === "true" ||
  process.env.CI === "1" ||
  process.env.LEFTHOOK === "0"
) {
  process.exit(0);
}

const result =
  process.platform === "win32"
    ? spawnSync("cmd.exe", ["/d", "/s", "/c", "lefthook install"], {
        stdio: "inherit",
        shell: false,
      })
    : spawnSync("lefthook", ["install"], {
        stdio: "inherit",
        shell: false,
      });

process.exit(result.status ?? 0);
