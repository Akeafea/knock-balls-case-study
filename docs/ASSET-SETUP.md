# Source checkout and asset setup

[Back to the project](../README.md) · [Third-party notices](../THIRD_PARTY_NOTICES.md)

## What you get

The repository contains original C# systems, the custom block gradient shader, authored level data, scene/prefab configurations, UI animation data, URP settings and Unity package declarations. The source scene is `Assets/Scenes/PlayGround.unity`.

The raw original art, audio and external VFX packages are not included. Their GUID references remain in the scene/prefabs so the structure can be inspected and the original dependencies can be restored locally. Missing asset/prefab warnings are expected until those dependencies are restored or replaced. A [reference inventory](omitted-local-assets.txt) lists the omitted files referred to by included assets.

This is not a self-contained playable demo. The screenshots and recorded runtime checks are from the complete local project.

## Compile the custom code

1. Use **Unity 6000.6.4f1** and let Unity Package Manager resolve `Packages/manifest.json`.
2. Install standard **DOTween** from [Demigiant](https://dotween.demigiant.com/download.php). Run **Tools → Demigiant → DOTween Utility Panel → Setup DOTween** and enable the Unity modules. `CameraShakeController` uses `DG.Tweening`.
3. Import **TMP Essential Resources** from Unity's TextMesh Pro menu. The original UI uses the LiberationSans SDF font asset.

Neither DOTween nor TextMesh Pro resource binaries are bundled. Installing packages locally does not change the repository's source license.

## Restore the original presentation

Known VFX references are:

- **Vefects Trails VFX – URP**, Asset Store ID **242574**. The ball prefab references `VFX_Trail_Earth.prefab`.
- **Kyeoms New Stylized Explosion Package**, Asset Store ID **260177**. The effect pool references `FX_Explosion_A_1_air.prefab`.

Obtain these packages separately under their own licenses. Exact GUID compatibility depends on the original package versions. The original package files are not uploaded here.

The cannon/barrel models, UI sprite images, surface textures and SFX also require separately obtained assets or your own replacements. Their redistribution rights were not supplied, so they were left out. If substituting assets, reassign their scene/prefab references; replacing only a file name does not guarantee that Unity GUIDs and prefab-local identifiers match. The ball itself inherits a VFX prefab, so replacing its missing base prefab is necessary before the original ball pool can operate.

The original screenshot appearance is only reproducible with the original dependencies. The repository does not promise that unrelated replacement packs will reproduce it automatically.

## UI options

After restoring scene dependencies, open `PlayGround` and use:

- **Tools → Knock Balls → UI → Themed** for the updated theme-aware appearance.
- **Tools → Knock Balls → UI → Legacy** for the preserved original appearance.

The same choice is available on `MainCanvas > Game UI Theme Controller > Appearance`. Save in Edit Mode to persist it. Play Mode changes follow Unity's normal temporary-state behavior.

## Public contribution boundary

Keep the raw asset exclusions in `.gitignore`. Do not upload an Asset Store package merely because it was free or already installed on your computer. New art contributions need a clear source and redistribution license. Screenshots are showcase material and do not grant rights to the individual assets shown within them.
