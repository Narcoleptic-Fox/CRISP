#!/usr/bin/env node
// Fail the build if a page exists but nothing in the sidebar links to it, or if the
// sidebar links to a page that does not exist.
//
// The sidebar is now GENERATED: the nav-gen plugin derives `navigation` from the docs/
// tree at build time and writes the result to .nav-generated.json. This gate compares
// that generated nav against the pages docmd actually built. It is not tautological: the
// plugin builds the nav in onBeforeBuild, docmd renders the site afterwards, and this
// catches a divergence between the two (a page the plugin dropped, or a nav path with no
// built page). The plugin also asserts completeness itself; this is the independent check.
//
// It used to validate a hand-maintained `navigation` array in docmd.config.json. Three
// pages sat invisible in this KB for a day before that gate existed; the generator makes
// that failure impossible by construction, and this confirms it every build.

import { readFileSync } from 'node:fs';
import { readdir } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

// fileURLToPath, not URL.pathname: on Windows `.pathname` is `/D:/...`, which join() then
// turns into `D:\D:\Dev\...` — a doubled drive letter that crashes. This is a Linux-only path
// otherwise, i.e. the gate would run on only some hosts. See lessons/gate-on-one-host.
const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const config = JSON.parse(readFileSync(join(ROOT, 'docmd.config.json'), 'utf8'));
// Optional arg: validate an alternate output dir (e.g. a staging build), relative to cwd.
// No arg → the configured output dir (site/). Lets kb-build gate a staged build before swap.
const OUT = process.argv[2] ? join(process.cwd(), process.argv[2]) : join(ROOT, config.out ?? 'site');

async function builtPages(dir, base = '') {
  const found = [];
  for (const entry of await readdir(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      found.push(...(await builtPages(join(dir, entry.name), `${base}/${entry.name}`)));
    } else if (entry.name === 'index.html') {
      found.push(`${base}/`);
    }
  }
  return found;
}

let navigation;
try {
  navigation = JSON.parse(readFileSync(join(ROOT, '.nav-generated.json'), 'utf8'));
} catch {
  console.error('nav check FAILED — .nav-generated.json not found. Run `docmd build` (the nav-gen plugin writes it) before the nav check.');
  process.exit(1);
}

const linked = new Set();
(function walk(nodes) {
  for (const n of nodes ?? []) {
    if (n.path) linked.add(n.path);
    walk(n.children);
  }
})(navigation);

const pages = await builtPages(OUT);
const orphans = pages.filter((p) => !linked.has(p));
const dead = [...linked].filter((p) => !pages.includes(p));

for (const p of orphans) console.error(`  unreachable: ${p}  (built, but nothing in the sidebar links to it)`);
for (const p of dead) console.error(`  dead link:   ${p}  (in the sidebar, but no such page)`);

if (orphans.length || dead.length) {
  console.error(`\nnav check FAILED — the generated nav and the built site disagree.`);
  console.error(`An orphan means the nav-gen plugin dropped a page; a dead link means the nav points at an unbuilt route.`);
  process.exit(1);
}
console.log(`nav check ok — ${pages.length} pages, all reachable.`);
