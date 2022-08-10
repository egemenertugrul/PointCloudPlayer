using System.IO;
using UnityEngine;

namespace PCP.Utils
{
    public static class AssetIndexerConfig
    {
        public static readonly string BaseDirectory = "AssetIndexerBase";
        public static readonly string ResourcesBaseDirectory = Path.Combine(Application.dataPath, "Resources", BaseDirectory);
    }
}