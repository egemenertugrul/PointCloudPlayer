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
 "com.egemenertugrul.pointcloudplayer": "1.0.16",
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


## Asset Package

Download and install the latest .unitypackage from [Releases](https://github.com/egemenertugrul/PointCloudPlayer/releases).

# Demo

You can try replaying the basketball pick-up game recording. 

- Copy the `/Runtime/PointCloudPlayer_ExampleScene.unity` scene to a directory under `/Assets/` and open.

- Download the demo [.ply dataset](https://drive.google.com/file/d/1nYlKXekA25xuq3vdRwbUMiJqUvn1ufbZ/view?usp=sharing).

- Unzip to the `/StreamingAssets/`. The folder structure should look like this:
```
/StreamingAssets/Basketball_PLY/Cam1/
/StreamingAssets/Basketball_PLY/Cam1/2022-07-22_15-52-41_0000.ply
/StreamingAssets/Basketball_PLY/Cam1/2022-07-22_15-52-41_0001.ply
...

/StreamingAssets/Basketball_PLY/Cam2/
/StreamingAssets/Basketball_PLY/Cam2/2022-07-22_15-52-41_0001.ply
/StreamingAssets/Basketball_PLY/Cam2/2022-07-22_15-52-41_0001.ply
...
```

## How the .PLY dataset was recorded
The recordings were done using iPi Recorder 4 with two KinectV2's facing each other.

<center><img src="https://imgur.com/jFSuXMV.jpg" style="width: 50%;"></center>

Later, they recordings were exported as .PLY files using the Biomech add-on in iPi Mocap Studio 4. Since no prior calibration was performed on the iPi side, transformations between the two cameras/recordings were manually adjusted in the Unity scene.

    A non-proprietary solution would definitely be more welcome for the future of this project.

    