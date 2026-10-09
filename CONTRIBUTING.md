# Contributing

This is a small educational Unity case study. Focused fixes and improvements are welcome.

1. Open an issue describing the behavior or improvement. Include the Unity version and reproduction steps for bugs.
2. Keep changes within the existing gameplay, pooling, level-data or UI boundaries where practical.
3. Verify the affected behavior in Unity. For pool changes, exercise both spawn and return paths; for UI changes, check Legacy/Themed switching and scene save/reload.
4. Submit a pull request explaining the resulting behavior and what you checked.

Do not commit Unity caches, builds, credentials, local editor state or the external asset packages excluded by `.gitignore`. Art contributions must include their source and a compatible redistribution license.

The original prototype's external assets are not bundled, so be explicit about which dependencies or replacements you used for runtime verification. Do not report missing licensed assets as evidence that the original local project failed its recorded checks.

By contributing original code, you agree to license that contribution under this repository's MIT license. Third-party content requires its own notices and a documented redistribution right.
