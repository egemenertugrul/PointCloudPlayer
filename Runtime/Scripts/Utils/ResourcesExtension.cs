using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PCP.Utils
{
    public static class ResourcesExtension
    {
        public static readonly string ResourcesDirectory = Path.Combine(Application.dataPath, "Resources");

        /// <summary>
        /// Recursively traverses each folder under <paramref name="path"/> and returns the list of file paths.
        /// </summary>
        /// <param name="path">Relative to Application.streamingAssetsPath.</param>
        /// <param name="paths">List of file path strings.</param>
        /// <returns>List of file path strings.</returns>
        public static List<string> GetPathsRecursively(string path, ref List<string> paths)
        {
            var fullPath = Path.Combine(ResourcesDirectory, path);
            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
            foreach (var file in dirInfo.GetFiles())
            {
                if (!file.Name.Contains(".meta"))
                {
                    paths.Add(Path.Combine(path, Path.GetFileNameWithoutExtension(file.Name))); // Without file extension
                }
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                GetPathsRecursively(Path.Combine(path, dir.Name), ref paths);
            }

            return paths;
        }

        public static List<string> GetPathsRecursively(string path)
        {
            List<string> paths = new List<string>();
            return GetPathsRecursively(path, ref paths);
        }

        /// <summary>
        /// Recursively traverses each folder under <paramref name="basePath"/> and loads each file as a <typeparamref name="T"/>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="basePath"></param>
        /// <returns>Returns the loaded object of type <typeparamref name="T"/></returns>
        public static List<T> LoadRecursively<T>(string basePath) where T : Object
        {
            List<string> paths = GetPathsRecursively(basePath);

            List<T> objects = new List<T>();
            foreach (var path in paths)
            {
                objects.Add(Resources.Load<T>(path));
            }
            return objects;
        }

        public static List<(T, string)> LoadRecursivelyWithPaths<T>(string basePath) where T : Object
        {
            List<string> paths = new List<string>();
            GetPathsRecursively(basePath, ref paths);

            List<(T, string)> list = new List<(T, string)>();
            foreach (var path in paths)
            {
                list.Add((Resources.Load<T>(path), path));
            }
            return list;
        }
    }
}