# Requirement Gate

Every non-trivial feature begins as a rough user request and is converted into a Requirement Brief before implementation.

## Requirement Brief format

### 1. Intent
What player-facing or developer-facing problem are we solving?

### 2. Observable behavior
What should visibly or measurably happen when the feature works?

### 3. Non-goals
What is explicitly outside the requested scope?

### 4. Existing-system investigation
Which systems/files should the implementation agent inspect before changing code?

### 5. Constraints
Performance, physics, architecture, platform, data, and workflow constraints.

### 6. Edge cases
Cases likely to break naive implementations.

### 7. Acceptance criteria
Concrete conditions that determine whether the task is complete.

### 8. Validation mode
Choose one:
- Automated Verified
- Needs Human Playtest

### 9. Task tier
Classify the task before implementation:
- S1 Cheap: local/simple/low-risk change.
- S2 Medium: multi-file or moderate system change.
- S3 Frontier: architecture, difficult debugging, performance-sensitive, physics-heavy, or high-ambiguity work.

Task tier is based on complexity + risk + scope + ambiguity, not code length alone.

## Execution rule

A Requirement Brief being ready does NOT start Cursor or any other agent. Implementation begins only after an explicit Start action.
