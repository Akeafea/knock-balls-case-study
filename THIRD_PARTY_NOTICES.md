# Third-party assets and publication scope

The MIT license covers original project code, the custom block gradient shader, and authored scene/prefab/level/configuration data in this repository. It does not relicense the original Knock Balls game or third-party assets.

## External dependencies

| Dependency | Used for | Public repository treatment |
| --- | --- | --- |
| Unity / URP / uGUI / Input System / TextMesh Pro | Engine, rendering, UI and input packages | Package declarations included; obtain packages through Unity Package Manager under their own terms |
| DOTween, Demigiant | Camera shake | Package omitted; install the standard version separately under the [DOTween license](https://dotween.demigiant.com/license.php) |
| Trails VFX – URP, Vefects (Asset Store ID 242574) | Projectile trail prefab referenced by the ball | Raw package omitted; [publisher listing](https://marketplace.unity.com/packages/vfx/trails-vfx-urp-242574) |
| New Stylized Explosion Package, Kyeoms (Asset Store ID 260177) | Explosion VFX referenced by the effect pool | Raw package omitted; [publisher listing](https://marketplace.unity.com/packages/vfx/particles/new-stylized-explosion-package-260177) |
| Original model, sprite, texture and audio files | Cannon/barrel appearance, UI graphics, surfaces and SFX | Omitted because no redistribution license was supplied |

Asset package names and IDs above were identified from local import metadata. Being free to acquire is not evidence that a raw package can be republished. See the [Unity Asset Store terms](https://unity.com/legal/as-terms).

## Screenshots

The PNGs in `docs/images` are screenshots supplied by the project author or captured from the complete local prototype. They illustrate the case study. The repository's MIT license does not grant rights to extract, redistribute or reuse the individual third-party artworks shown within them.

The SVG banner is an original diagrammatic illustration created for this repository and is included under MIT.

## Restoring dependencies

An inventory of references to omitted local files is provided in [docs/omitted-local-assets.txt](docs/omitted-local-assets.txt). Restoring assets locally does not make them eligible for a public commit. Keep the exclusions in `.gitignore` unless an explicit redistribution license has been established.

This is an unofficial educational recreation of Knock Balls. The original game's name and rights remain with its respective rights holders; no affiliation or endorsement is claimed.
