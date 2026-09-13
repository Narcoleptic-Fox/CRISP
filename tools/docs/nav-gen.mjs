// nav-gen: derive docmd's `navigation` from the docs/ tree plus page frontmatter, replacing the
// hand-maintained array. Runs in the onBeforeBuild hook and sets config.navigation.
//
// It reads docmd's own parsed `pages` and their `outputPath`, so the generated routes ARE docmd's
// routes: there is no independent route derivation and therefore no seam to guard. Ordering honours
// frontmatter nav_order (ascending) first, then the group's nav_sort (date-desc | alpha), then title.
// A group directory with an index.md becomes a linked group node; one without becomes a pathless
// expander (a warning, not an error). Bad input fails the build loudly: a non-lowercase path segment
// (case-sensitive on Linux, not on Windows), a duplicate nav_order within a group, a dateless entry in
// a date-desc group. After building, it asserts every page is reachable in the nav it produced, then
// writes .nav-generated.json at the repo root for tools/docs/check-nav.mjs to validate against the built site.
import { writeFileSync } from 'node:fs';
import { join } from 'node:path';

export const plugin = { name: 'nav-gen', version: '1.0.0', capabilities: ['build'] };

function routeOf(op) {
  let s = op.replace(/index\.html$/, '');
  if (!s.startsWith('/')) s = '/' + s;
  if (!s.endsWith('/')) s += '/';
  return s;
}

const fmOf = (node) => (node.page && node.page.frontmatter) || {};
const titleOf = (node, seg) => fmOf(node).nav_title || fmOf(node).title || seg;
const orderOf = (node) => { const o = fmOf(node).nav_order; return typeof o === 'number' ? o : null; };
const dateOf = (node) => fmOf(node).date || '';

export async function onBeforeBuild({ config, pages, tui }) {
  const warn = (m) => (tui && tui.warn ? tui.warn('nav-gen: ' + m) : console.log('nav-gen warn: ' + m));
  const fail = (m) => { throw new Error('nav-gen: ' + m); };

  const byRoute = {};
  for (const pg of pages) {
    const r = routeOf(pg.outputPath);
    byRoute[r] = pg;
    for (const seg of r.split('/').filter(Boolean)) {
      if (seg !== seg.toLowerCase()) fail("non-lowercase path segment '" + seg + "' in " + r + " (case-sensitive on Linux, case-insensitive on Windows: it passes locally and 404s in production)");
    }
  }

  const root = { route: '/', page: byRoute['/'] || null, children: {} };
  for (const [route, page] of Object.entries(byRoute)) {
    if (route === '/') continue;
    let node = root, acc = '';
    for (const seg of route.split('/').filter(Boolean)) {
      acc += '/' + seg;
      if (!node.children[seg]) node.children[seg] = { route: acc + '/', page: null, children: {} };
      node = node.children[seg];
    }
    node.page = page;
  }

  function orderChildren(entries, inheritedSort) {
    return entries.slice().sort((a, b) => {
      const ao = orderOf(a.node), bo = orderOf(b.node);
      if (ao != null && bo != null && ao !== bo) return ao - bo;
      if (ao != null && bo == null) return -1;
      if (ao == null && bo != null) return 1;
      if (inheritedSort === 'date-desc') {
        const ad = dateOf(a.node), bd = dateOf(b.node);
        if (ad !== bd) return ad < bd ? 1 : -1;
      }
      return titleOf(a.node, a.seg).localeCompare(titleOf(b.node, b.seg));
    });
  }

  function toNav(node, seg, inheritedSort) {
    const kidEntries = Object.entries(node.children).map(([s, k]) => ({ seg: s, node: k }));
    const fm = fmOf(node);
    const title = titleOf(node, seg);
    if (!kidEntries.length) {
      if (!fm.nav_title && !fm.title) warn('page has no title, nav label falls back to its slug: ' + node.route);
      return { title, path: node.route, icon: fm.nav_icon || 'file-text' };
    }
    const seen = {};
    for (const e of kidEntries) {
      const o = orderOf(e.node);
      if (o != null) { if (seen[o]) fail('duplicate nav_order ' + o + ' under ' + node.route + ' (' + seen[o] + ' and ' + e.seg + ')'); seen[o] = e.seg; }
    }
    if (!node.page && node.route !== '/') warn('missing index.md (pathless group, ' + kidEntries.length + ' children): ' + node.route);
    const sort = fm.nav_sort || inheritedSort;
    if (sort === 'date-desc') {
      for (const e of kidEntries) {
        const isLeaf = !Object.keys(e.node.children).length;
        if (isLeaf && !dateOf(e.node)) fail("date-desc group " + node.route + " has a dateless leaf '" + e.seg + "' (a date-sorted group needs a date on every entry, or it sorts to the bottom silently)");
      }
    }
    const ordered = orderChildren(kidEntries, sort);
    const out = { title, icon: fm.nav_icon || 'folder', collapsible: true, children: ordered.map((e) => toNav(e.node, e.seg, sort)) };
    if (node.page) out.path = node.route;
    if (fm.nav_collapsed === true) out.defaultCollapsed = true;
    return out;
  }

  const nav = [];
  if (root.page) nav.push({ title: fmOf(root).nav_title || 'Overview', path: '/', icon: fmOf(root).nav_icon || 'home' });
  const topEntries = Object.entries(root.children).map(([s, k]) => ({ seg: s, node: k }));
  for (const e of orderChildren(topEntries, null)) nav.push(toNav(e.node, e.seg, null));

  // Completeness: every built page must be reachable in the nav we produced.
  const inNav = new Set();
  (function collect(a) { for (const n of a) { if (n.path) inNav.add(n.path); if (n.children) collect(n.children); } })(nav);
  const missing = Object.keys(byRoute).filter((r) => !inNav.has(r));
  if (missing.length) fail('generated nav omits ' + missing.length + ' page(s): ' + missing.slice(0, 5).join(', '));

  config.navigation = nav;
  writeFileSync(join(process.cwd(), '.nav-generated.json'), JSON.stringify(nav, null, 2));
  (tui && tui.info ? (m) => tui.info(m) : (m) => console.log(m))('nav-gen: ' + nav.length + ' top-level entries, ' + inNav.size + ' pages all reachable');
}
