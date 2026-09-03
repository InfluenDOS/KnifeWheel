# KnifeWheel

KnifeWheel is a small 3D Unity game and an AI-assisted game-development workflow sandbox.

## Project setup

- **Unity (local)**: `2022.3.62f3c1` (China Hub LTS) — open the repo root with this editor
- **Unity (GitHub CI / GameCI)**: `2022.3.62f3` (official international LTS; GameCI has no `c1` images)
- **Render pipeline**: Built-in
- **Input**: Legacy Input Manager (`activeInputHandler: 0`)
- **Playable scene**: `Assets/Scenes/MotorcyclePrototype.unity`

Controls: **W/S** throttle/brake, **A/D** steer.

## Game concept

The player rides a motorcycle or bicycle whose wheels are blades. The core fantasy is high-speed driving, slicing through road obstacles, vehicles, and hostile humanoid targets, with exaggerated physical destruction and readable feedback.

## Why this project exists

KnifeWheel is intentionally small. Its main purpose is to validate an AI game-development pipeline before applying that pipeline to larger production projects.

Target workflow:

1. The user describes a feature in ChatGPT.
2. A high-capability model converts the rough idea into a Requirement Brief.
3. The task is classified by complexity/risk so an appropriate model tier can be chosen.
4. Cursor/another implementation agent changes the repository.
5. Unity automated tests run.
6. Codex performs an independent review.
7. The implementation agent fixes review findings and tests run again.
8. Tasks that require visual/game-feel validation stop at `Needs Human Playtest`.
9. Verified code-only tasks may proceed to merge.

## Initial gameplay scope

- 3D third-person vehicle prototype.
- Motorcycle first; bicycle can reuse the same vehicle abstraction later.
- Rigidbody-based driving.
- Blade wheels can apply slicing/damage logic.
- Simple sliceable cars and humanoid dummies.
- No production art required.
- No real-time mesh slicing in the first milestone.

## Workflow principle

Communication events are not execution events. Ordinary comments, reviews, mentions, and status changes must never start an implementation agent by themselves. Starting implementation must always be an explicit action.
