using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using CielaSpike;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using UnityEngine.Events;
using PCP.Utils;

namespace PCP
{
    [RequireComponent(typeof(ParticlesFromData))]
    public class PointCloudPlayer : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnObsoleteDataReceived = new UnityEvent();

        public enum DataReadModes { Remote, Local, StreamingAssets }
        public DataReadModes ReadMode;

        /// <summary>
        /// Path to a remote http server directory (i.e. http://localhost:8000/Recording_1/ , served by 'python -m http.server')
        /// </summary>
        public string RemoteHostPath;

        /// <summary>
        /// Path to a local directory (i.e. C:/../Recording_1/)
        /// </summary>
        public string LocalPath;

        /// <summary>
        /// Path to a streaming asset directory (i.e. Recording_1) <br/>
        /// <b>NOTE:</b> It works in conjunction with AssetIndexerBuildProcessor.cs for accessing files in standalone (non-editor) builds.
        /// </summary>
        public string StreamingAssetsPath;

        public int FPS = 30;
        public bool isLoop = true;

        private float t;
        private int playIndex, lastPlayedIndex;
        private string[] plyFiles;

        public int PlayIndex { get => playIndex; set => playIndex = value; }

        private void OnEnable()
        {
            PlayIndex = 0;
            UpdatePLYFiles();
        }

        private void UpdatePLYFiles()
        {
            if (ReadMode == DataReadModes.Local)
                plyFiles = Directory.GetFiles(LocalPath, "*.ply*");
            else if (ReadMode == DataReadModes.StreamingAssets)
            {
#if UNITY_EDITOR
                plyFiles = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, StreamingAssetsPath), "*.ply*");
#else
                List<string> plyFilesList = ReadAssetIndexer();
                plyFiles = plyFilesList.ToArray();
#endif
            }
            else if (ReadMode == DataReadModes.Remote)
                StartCoroutine(GetFilesFromHTTP(RemoteHostPath, (val) => { plyFiles = val; }));
        }

        private List<string> ReadAssetIndexer()
        {
            List<string> plyFilesList = new List<string>();
            TextAsset paths = Resources.Load<TextAsset>(Path.Combine(AssetIndexerConfig.BaseDirectory, StreamingAssetsPath, "paths"));
            string fs = paths.text;
            string[] fLines = Regex.Split(fs, "\n|\r|\r\n");

            foreach (string line in fLines)
            {
                if (line.Length > 0)
                    plyFilesList.Add(line);
            }

            return plyFilesList;
        }

        private IEnumerator GetFilesFromHTTP(string url, Action<string[]> callback)
        {
            List<string> paths = new List<string>();
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                using (StreamReader reader = new StreamReader(webRequest.downloadHandler.text.GenerateStream()))
                {
                    string html = reader.ReadToEnd();
                    Regex regex = new Regex(GetDirectoryListingRegexForUrl());
                    MatchCollection matches = regex.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                string value = match.Groups["name"].Value;
                                string path = Path.Combine(url, value);

                                paths.Add(path);
                                //print(path);
                            }
                        }
                    }
                    callback(paths.ToArray());
                }
            }
        }

        public static string GetDirectoryListingRegexForUrl()
        {
            return "<a href=\".*\">(?<name>.*)</a>";
        }

        private IEnumerator Play(int index)
        {
            PlyImporter.DataBody plyData = null;
            string filepath = plyFiles[PlayIndex];

            yield return this.StartCoroutineAsync(PlyImporter.Instance.ImportData(filepath, ReadMode, (data) => { plyData = data; }));

            bool dropFrames = false;
            if (plyData != null)
            {
                if (lastPlayedIndex > index && index != 0)
                {
                    //print("Obsolete data received");
                    OnObsoleteDataReceived.Invoke();
                    dropFrames = true;
                }

                if (!dropFrames)
                {
                    gameObject.GetComponent<ParticlesFromData>().Set(plyData.vertices, plyData.colors);
                    lastPlayedIndex = index;
                }
            }

            //gameObject.GetComponent<ParticlesFromVertices>().New(plyData.vertices, plyData.colors, 0.1f);
        }

        private void Update()
        {
            if (plyFiles == null)
            {
                return;
            }

            FPS = Mathf.Max(1, FPS);

            t += Time.deltaTime;

            if (plyFiles.Length > 0 && t >= 1f / FPS)
            {
                t = 0;
                if (PlayIndex < plyFiles.Length)
                {
                    StartCoroutine(Play(playIndex));
                    ++PlayIndex;
                }
                else if (isLoop)
                {
                    PlayIndex = 0;
                }
            }

        }
    }
}