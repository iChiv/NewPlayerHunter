# Unity Development Rules

Unity Editor version: 6000.0.81f1

## Development workflow

For every gameplay implementation:

1. Inspect the existing project before modifying it.
2. Implement or modify C# source files directly.
3. Allow Unity to compile the project.
4. Check compilation errors and warnings.
5. Fix errors caused by the change.
6. Use Unity MCP for Editor operations:
   - Scenes
   - GameObjects
   - Components
   - Prefabs
   - Materials
   - Assets
7. Do not directly edit Unity scene or prefab YAML files unless absolutely necessary.
8. Use Unity CLI / Unity Pipeline for:
   - tests
   - builds
   - automated Editor commands
9. Add EditMode or PlayMode tests when the feature contains testable logic.
10. Verify the feature after implementation instead of stopping after code generation.

## Tool preference

C# / text files:
Use normal filesystem editing.

Scene / Prefab / GameObject / Components:
Prefer Unity MCP.

Compilation / Test / Build / automation:
Prefer Unity CLI and Unity Pipeline.

## Completion criteria

A task is not complete merely because code has been generated.

A task is complete when:
- the project compiles,
- relevant tests pass,
- required Unity objects/components are configured,
- and the requested behavior has been verified where practical.