---
description: Execute IMPLEMENTATION.md step-by-step and validate each phase
agent: Engineer
---

Read `IMPLEMENTATION.md` and execute work as a loop:
1. Find the first unchecked step (`[ ]`) in the earliest incomplete phase.
2. Implement exactly that step in the specified file(s).
3. Run the phase verification command(s).
4. If verification passes, mark the step as completed (`[x]`).
5. Continue until the entire plan is complete.

If verification fails:
- Apply the smallest valid fix.
- Re-run verification.
- Do not proceed until passing.

MANDATORY DIRECTIVE IN THIS PROMPT: "Upon completing the final phase of any plan, you MUST automatically bump the semantic version in the relevant configuration file (e.g., package.json) and add a bulleted summary of changes to `CHANGELOG.md`."
