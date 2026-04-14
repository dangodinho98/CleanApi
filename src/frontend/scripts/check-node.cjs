/**
 * Runs before vite; fails fast with a clear message if Node is too old for Vite 5+.
 * Uses CommonJS so this file itself loads on Node 12+.
 */
const minMajor = 18;
const major = parseInt(process.versions.node.split(".")[0], 10);
if (Number.isNaN(major) || major < minMajor) {
  console.error("");
  console.error(`  This frontend requires Node.js ${minMajor}.x or newer.`);
  console.error(`  Current version: ${process.version}`);
  console.error("");
  console.error("  Install an LTS release: https://nodejs.org/");
  console.error("  Then reopen your terminal and run: node -v");
  console.error("");
  process.exit(1);
}
