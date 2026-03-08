---
description: Apply minimal deterministic fixes from terminal errors
agent: Engineer
---

Use the current terminal error output as the only target signal.

Execution protocol:
1. Identify the failing command and exact error text.
2. Locate the precise source file and line range causing the failure.
3. Apply the exact minimal code or config fix.
4. Re-run the same command to verify resolution.
5. Stop after the failure is resolved or report a concrete blocker.

Constraints:
- No broad refactors.
- No speculative unrelated edits.
- Keep fixes narrowly scoped and reversible.
