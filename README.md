Point Cloud / Volumetric Video Player for Unity
=============================================

**Point Cloud Player (PCP)** is a tool for reading & playing series of .PLY files as point clouds / volumetric videos. It can import .PLY files on-the-fly from `Local`, `Remote`, and `StreamingAssets` sources and display through the native particle system.

PCP uses a modified version of `PlyImporter.cs` from [keijiro/Pcx](https://github.com/keijiro/Pcx).

![Demo](https://imgur.com/UkSCUDq.gif)

# Requirements
- Unity 2019.4

# Installation

Use the [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) feature to import the packages.

Add the following in the package manifest file `Packages/manifest.json`

To the `scopedRegistries` section:

```
{
    "name": "egemenertugrul",
    "url": "https://registry.npmjs.com",
    "scopes": [ "com.egemenertugrul" ]
}
```

To the `dependencies` section:
```
 "com.egemenertugrul.pointcloudplayer": "1.0.0",
```

The manifest file should like like:
```
{
    "scopedRegistries": [
    {
      "name": "egemenertugrul",
      "url": "https://registry.npmjs.com",
      "scopes": [ "com.egemenertugrul" ]
    }
  ],
  "dependencies": {
    "com.egemenertugrul.pointcloudplayer": "1.0.0",
    ...
```