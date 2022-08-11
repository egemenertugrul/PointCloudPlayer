Point Cloud / Volumetric Video Player for Unity
=============================================

**Point Cloud Player (PCP)** is a tool for reading & playing series of .PLY files as point clouds / volumetric videos. It can import .PLY files on-the-fly from `Local`, `Remote`, and `StreamingAssets` sources and display through the native particle system.

PCP uses [keijiro/Pcx](https://github.com/keijiro/Pcx) under the hood.

![Demo](https://imgur.com/UkSCUDq.gif)

# Requirements
- Unity 2019.4

# Installation

Use the [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) feature to import the packages.

Add the following in the package manifest file `Packages/manifest.json`

To the `scopedRegistries` section:

```
{
    "name": "Keijiro",
    "url": "https://registry.npmjs.com",
    "scopes": [ "jp.keijiro" ]
},
{
    "name": "egemenertugrul",
    "url": "https://registry.npmjs.com",
    "scopes": [ "com.egemenertugrul" ]
}
```

To the `dependencies` section:
```
 "jp.keijiro.pcx": "1.0.1",
 "com.egemenertugrul.pointcloudplayer": "1.0.0",
```

The manifest file should like like:
```
{
    "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    },
    {
      "name": "egemenertugrul",
      "url": "https://registry.npmjs.com",
      "scopes": [ "com.egemenertugrul" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.pcx": "1.0.1",
    "com.egemenertugrul.pointcloudplayer": "1.0.0",
    ...
```