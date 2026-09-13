---
title: Design notes
description: Point-in-time records of decisions as they were made, with the evidence in front of us at the time. History, not a living specification.
nav_order: 6
nav_icon: file-clock
nav_sort: date-desc
---

# Design notes

A design note records a decision at the moment it was made: the problem, the options, what was
chosen and why, and what was known at the time. It is never updated to match the code afterwards.
If the code moves on, a new note supersedes the old one and links back; the old one stays as the
record of why things were once otherwise.

Read the [architecture](/architecture/) pages for how things are. Read these for how they got that way.

## Writing one

```markdown title="docs/design/YYYY-MM-DD-short-slug.md"
---
title: Short imperative title
description: One sentence on what was decided.
date: YYYY-MM-DD
status: accepted        # proposed | accepted | superseded by YYYY-MM-DD-other
---
```

Sections: **Context** (what forced the decision), **Options** (the ones seriously considered, with
the reason each was or was not taken), **Decision**, **Consequences** (what got easier, what got
harder, what to watch). Date everything; name who decided where it matters.
