---
title: Domain guides
description: What each business area does and the rules it enforces, written for the person who owns the feature.
nav_order: 3
nav_icon: book-open
---

# Domain guides

A domain guide describes a business area in the words of the business: what the thing is, what
you can do to it, what is not allowed and what happens afterwards. It does not explain how the
code is organised; that is the [architecture](/architecture/) section. Read a domain guide to
answer "what does this application let me do and why did it refuse?"

The template ships one business area, [Items](/domain/items/), as a worked sample. An application
generated from it replaces the sample with its own areas and writes one guide per area in the same
shape:

| Section | Answers |
| --- | --- |
| What it is | The noun, its fields, what each one means to the business |
| What you can do | The operations, who may perform them |
| Rules | What is refused and with which message |
| What happens afterwards | Events, audit, side effects |
| Where it lives | The code, for the reader who needs to go further |
