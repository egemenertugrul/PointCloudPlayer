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
using System.Linq;

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
        public int BufferSize = 20;
        public int filterEveryN = 0;

        private float t;
        private int playIndex, lastPlayedIndex;
        private string[] plyFiles;

        private int loadIndex, orderedLoadIndex;

        public int PlayIndex { get => playIndex; set => playIndex = value; }

        //Queue<PlyImporter.DataBody> plyBuffer;
        PlyImporter.DataBody[] plyBuffer;
        //private int loadOrderCount;

        UnityEvent OnPlyFilesListUpdated = new UnityEvent();

        private void OnEnable()
        {
            //plyBuffer = new Queue<PlyImporter.DataBody>(BufferSize);
            ResetIndexesAndBuffer();

            OnPlyFilesListUpdated.AddListener(() =>
            {
                plyBuffer = new PlyImporter.DataBody[plyFiles.Length];
                print(plyBuffer[0]);
            });

            UpdatePLYFiles();
        }

        private void ResetIndexesAndBuffer()
        {
            PlayIndex = 0;
            loadIndex = 0;
            if (plyFiles != null) plyBuffer = new PlyImporter.DataBody[plyFiles.Length];
            else plyBuffer = null;
        }

        private void UpdatePLYFiles()
        {
            if (ReadMode == DataReadModes.Local)
            {
                plyFiles = Directory.GetFiles(LocalPath, "*.ply*");
                OnPlyFilesListUpdated.Invoke();
            }
            else if (ReadMode == DataReadModes.StreamingAssets)
            {
#if UNITY_EDITOR
                plyFiles = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, StreamingAssetsPath), "*.ply*");
#else
                List<string> plyFilesList = ReadAssetIndexer();
                plyFiles = plyFilesList.ToArray();
#endif
                OnPlyFilesListUpdated.Invoke();
            }
            else if (ReadMode == DataReadModes.Remote)
                StartCoroutine(GetFilesFromHTTP(RemoteHostPath, (val) => { 
                    plyFiles = val;
                    OnPlyFilesListUpdated.Invoke();
                }));
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

        private IEnumerator LoadToBuffer(int index)
        {
            //PlyImporter.DataBody plyData = null;
            string filepath = plyFiles[index];

            yield return this.StartCoroutineAsync(PlyImporter.Instance.ImportData(filepath, ReadMode, (data) =>
            {
                //print("Loaded: " + filepath);
                //--loadOrderCount;
                //plyBuffer.Enqueue(data);
                plyBuffer[index] = data;
                //print(loadOrderCount);
            }, () =>
            {
                plyBuffer[index] = new PlyImporter.DataBody(0);
                //--loadOrderCount;
                //print(loadOrderCount);
            }));

        }
        private void FillBuffer()
        {
            var bufferEndIndex = Mathf.Clamp(playIndex + BufferSize, 0, plyFiles.Length);
            while (bufferEndIndex > loadIndex)
            {
                StartCoroutine(LoadToBuffer(loadIndex++));
                //if (loadIndex >= plyFiles.Length && isLoop)
                //{
                //    loadIndex = 0;
                //}
            }
        }

        private void Play(PlyImporter.DataBody plyData)
        {
            //bool dropFrames = false;
            if (plyData == null)
            {
                return;
            }
            //if (lastPlayedIndex > index && index != 0)
            //{
            //    //print("Obsolete data received");
            //    OnObsoleteDataReceived.Invoke();
            //    dropFrames = true;
            //}

            //if (dropFrames)
            //    yield break;

            var vertices = plyData.vertices;
            var colors = plyData.colors;

            Collider[] activeFilterColliders = GetComponentsInChildren<Collider>().Where(c => c.enabled).ToArray();

            if (activeFilterColliders.Length > 0)
            {
                List<Vector3> filteredVertices = new List<Vector3>();
                List<Color32> filteredColors = new List<Color32>();

                for (int i = 0; i < vertices.Count; i++)
                {
                    var vertex = vertices[i];
                    var color = colors[i];
                    for (int j = 0; j < activeFilterColliders.Length; j++)
                    {
                        var filterCollider = activeFilterColliders[j];
                        if (filterCollider.bounds.Contains(transform.TransformPoint(vertices[i])))
                        {
                            filteredVertices.Add(vertex);
                            filteredColors.Add(color);
                            break;
                        }
                    }
                }

                vertices = filteredVertices;
                colors = filteredColors;
            }

            if (filterEveryN > 0)
            {
                vertices = vertices.Where((x, i) => i % filterEveryN == 0).ToList();
                colors = colors.Where((x, i) => i % filterEveryN == 0).ToList();
            }

            if(vertices.Count > 0)
            {
                gameObject.GetComponent<ParticlesFromData>().Set(vertices, colors);
                //lastPlayedIndex = index;

                var PfV = gameObject.GetComponent<ParticlesFromVertices>();
                PfV?.New(vertices, colors);
            }
        }

        private void Start()
        {
            //StartCoroutine(FillBuffer());
        }

        private void Update()
        {
            if (plyFiles == null || plyBuffer == null)
            {
                return;
            }

            FillBuffer();

            FPS = Mathf.Max(1, FPS);
            t += Time.deltaTime;
            if (plyFiles.Length > 0 && t >= 1f / FPS)
            {
                t = 0;
                if (PlayIndex < plyFiles.Length)
                {
                    //StartCoroutine(Play(playIndex));
                    //if (plyBuffer.TryDequeue(out PlyImporter.DataBody data))
                    var plyData = plyBuffer[PlayIndex];
                    if (plyData != null)
                    {
                        Play(plyData);
                        plyBuffer[PlayIndex] = null;
                        ++PlayIndex;
                    }
                    else
                    {
                        print("Running faster than buffer can fill!");
                    }
                }
                else if (isLoop)
                {
                    ResetIndexesAndBuffer();
                }
            }

        }
    }
}