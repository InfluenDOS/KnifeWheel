# Test Strategy

KnifeWheel uses tests as a hard gate between implementation and review.

## Layers

### EditMode
Use for pure logic and data behavior such as damage calculation, thresholds, state transitions, cooldowns, and utility code.

### PlayMode
Use for GameObject, Rigidbody, collider, MonoBehaviour, and scene-dependent behavior.

### Smoke
A minimal playable test scene should eventually verify that the project boots, the player vehicle exists, critical systems initialize, and no unexpected exceptions occur during a short simulation.

### Build
At least one CI path should verify that the Unity project compiles/builds in a clean environment once the project skeleton exists.

## Rules

- Non-trivial features should add or update appropriate tests.
- An agent must not weaken, delete, skip, or rewrite a valid test merely to make CI pass.
- Review fixes must trigger relevant tests again.
- A failed automated test blocks merge.
- Visual feel, animation quality, VFX quality, camera feel, and subjective vehicle handling require `Needs Human Playtest` even when automated tests pass.

## Initial KnifeWheel targets

When implementation begins, tests should gradually cover:
- Vehicle input mapping and acceleration logic.
- Steering constraints.
- Blade-wheel activation/damage conditions.
- Minimum speed/rotation thresholds for slicing.
- Repeated collision protection.
- Sliceable state transitions.
- Destruction behavior.
- Null/destroyed target handling.
