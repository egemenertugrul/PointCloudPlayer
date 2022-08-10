#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PCP.Utils
{
    public class AssetIndexerBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.LogWarning("BuildProcessor - OnPreprocessBuild");

            PointCloudPlayer[] players = Object.FindObjectsOfType<PointCloudPlayer>();
            foreach (var player in players)
            {
                SaveStreamingAssetPaths(player.StreamingAssetsPath);
            }

            //SaveStreamingAssetPaths("Recording_1");
            //SaveStreamingAssetPaths("Recording_2");
        }

        private void SaveStreamingAssetPaths(string subDirectory = "", string file_name = "paths")
        {
            List<string> paths = StreamingAssetsExtension.GetPathsRecursively(subDirectory)
                .Where(p => !p.Contains(file_name)).ToList(); // Gets list of files from StreamingAssets/directory

            string txtPath = Path.Combine(AssetIndexerConfig.ResourcesBaseDirectory, subDirectory, file_name + ".txt"); // writes the list of file paths to /Assets/Resources/BaseDirectory..

            if (!Directory.Exists(txtPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(txtPath));
            }

            if (File.Exists(txtPath))
            {
                File.Delete(txtPath);
            }

            using (FileStream fs = File.Create(txtPath)) { }

            using (StreamWriter writer = new StreamWriter(txtPath, false))
            {
                foreach (string path in paths)
                {
                    writer.WriteLine(path);
                }
            }
        }
    }
}

#endif