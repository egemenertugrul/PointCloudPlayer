// An edited version of
// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

using CielaSpike;
using System.Net;
using UnityEngine.Networking;
using PCP.Utils;

namespace PCP
{
    class PlyImporter : Singleton<PlyImporter>
    {
        #region ScriptedImporter implementation

        public enum ContainerType { Mesh, ComputeBuffer, Texture }

        [SerializeField] ContainerType _containerType = ContainerType.Mesh;

        #endregion

        #region Internal data structure

        enum DataProperty
        {
            Invalid,
            R8, G8, B8, A8,
            R16, G16, B16, A16,
            SingleX, SingleY, SingleZ,
            DoubleX, DoubleY, DoubleZ,
            Data8, Data16, Data32, Data64
        }

        int GetPropertySize(DataProperty p)
        {
            switch (p)
            {
                case DataProperty.R8: return 1;
                case DataProperty.G8: return 1;
                case DataProperty.B8: return 1;
                case DataProperty.A8: return 1;
                case DataProperty.R16: return 2;
                case DataProperty.G16: return 2;
                case DataProperty.B16: return 2;
                case DataProperty.A16: return 2;
                case DataProperty.SingleX: return 4;
                case DataProperty.SingleY: return 4;
                case DataProperty.SingleZ: return 4;
                case DataProperty.DoubleX: return 8;
                case DataProperty.DoubleY: return 8;
                case DataProperty.DoubleZ: return 8;
                case DataProperty.Data8: return 1;
                case DataProperty.Data16: return 2;
                case DataProperty.Data32: return 4;
                case DataProperty.Data64: return 8;
            }
            return 0;
        }

        class DataHeader
        {
            public List<DataProperty> properties = new List<DataProperty>();
            public int vertexCount = -1;
        }

        public class DataBody
        {
            public List<Vector3> vertices;
            public List<Color32> colors;

            public DataBody(int vertexCount)
            {
                vertices = new List<Vector3>(vertexCount);
                colors = new List<Color32>(vertexCount);
            }

            public void AddPoint(
                float x, float y, float z,
                byte r, byte g, byte b, byte a
            )
            {
                vertices.Add(new Vector3(x, y, z));
                colors.Add(new Color32(r, g, b, a));
            }
        }

        #endregion

        #region Reader implementation

        public class MeshWithData
        {
            public Mesh mesh;
            public DataBody data;

            public MeshWithData(Mesh mesh, DataBody data)
            {
                this.mesh = mesh;
                this.data = data;
            }
        }

        private IEnumerator CreateStream(string path, PointCloudPlayer.DataReadModes readMode, Action<Stream> returnCallback)
        {
            Stream stream = null;
            bool isEditor = false;

#if UNITY_EDITOR
            isEditor = true;
#endif
            if (readMode == PointCloudPlayer.DataReadModes.StreamingAssets)
            {
                path = Path.Combine(Application.streamingAssetsPath, path);
            }

            if (readMode == PointCloudPlayer.DataReadModes.Local || readMode == PointCloudPlayer.DataReadModes.StreamingAssets && isEditor)
            {
                stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else // StreamingAssets || Remote
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(path);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    stream = webRequest.downloadHandler.data.GenerateStream();
                }
            }

            returnCallback.Invoke(stream);
        }

        /// <summary>
        /// Read PLY data asyncronously through ThreadNinja background thread.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="readMode"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator ImportData(string path, PointCloudPlayer.DataReadModes readMode, Action<DataBody> callback)
        {
            yield return Ninja.JumpToUnity;

            Stream stream = null;
            yield return CreateStream(path, readMode, (_stream) => { stream = _stream; });

            yield return Ninja.JumpBack;

            if (stream != null)
            {
                var header = ReadDataHeader(new StreamReader(stream));
                var body = ReadDataBody(header, new BinaryReader(stream));
                yield return null;
                callback?.Invoke(body);
            }
        }

        /// <summary>
        /// Read PLY data asyncronously through ThreadNinja background thread and create a mesh.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="readMode"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator ImportAsMesh(string path, PointCloudPlayer.DataReadModes readMode, Action<MeshWithData> callback)
        {
            Stream stream = null;
            yield return CreateStream(path, readMode, (_stream) => { stream = _stream; });

            if (stream == null)
            {
                yield break;
            }

            var header = ReadDataHeader(new StreamReader(stream));
            var body = ReadDataBody(header, new BinaryReader(stream));

            yield return Ninja.JumpToUnity; // Switch to Unity thread

            var mesh = new Mesh();
            mesh.name = Path.GetFileNameWithoutExtension(path);

            mesh.indexFormat = header.vertexCount > 65535 ?
                IndexFormat.UInt32 : IndexFormat.UInt16;

            mesh.SetVertices(body.vertices);
            mesh.SetColors(body.colors);

            mesh.SetIndices(
                Enumerable.Range(0, header.vertexCount).ToArray(),
                MeshTopology.Points, 0
            );

            mesh.UploadMeshData(true);
            callback?.Invoke(new MeshWithData(mesh, body));
        }

        public MeshWithData ImportAsMesh(string path)
        {
            try
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var header = ReadDataHeader(new StreamReader(stream));
                var body = ReadDataBody(header, new BinaryReader(stream));

                var mesh = new Mesh();
                mesh.name = Path.GetFileNameWithoutExtension(path);

                mesh.indexFormat = header.vertexCount > 65535 ?
                    IndexFormat.UInt32 : IndexFormat.UInt16;

                mesh.SetVertices(body.vertices);
                mesh.SetColors(body.colors);

                mesh.SetIndices(
                    Enumerable.Range(0, header.vertexCount).ToArray(),
                    MeshTopology.Points, 0
                );

                mesh.UploadMeshData(true);
                return new MeshWithData(mesh, body);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }

        //public PointCloudData ImportAsPointCloudData(string path)
        //{
        //    try
        //    {
        //        var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        var header = ReadDataHeader(new StreamReader(stream));
        //        var body = ReadDataBody(header, new BinaryReader(stream));
        //        var data = ScriptableObject.CreateInstance<PointCloudData>();
        //        data.Initialize(body.vertices, body.colors);
        //        data.name = Path.GetFileNameWithoutExtension(path);
        //        return data;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed importing " + path + ". " + e.Message);
        //        return null;
        //    }
        //}

        //BakedPointCloud ImportAsBakedPointCloud(string path)
        //{
        //    try
        //    {
        //        var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        var header = ReadDataHeader(new StreamReader(stream));
        //        var body = ReadDataBody(header, new BinaryReader(stream));
        //        var data = ScriptableObject.CreateInstance<BakedPointCloud>();
        //        data.Initialize(body.vertices, body.colors);
        //        data.name = Path.GetFileNameWithoutExtension(path);
        //        return data;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Failed importing " + path + ". " + e.Message);
        //        return null;
        //    }
        //}

        DataHeader ReadDataHeader(StreamReader reader)
        {
            var data = new DataHeader();
            var readCount = 0;

            // Magic number line ("ply")
            var line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
                throw new ArgumentException("Magic number ('ply') mismatch.");

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "format binary_little_endian 1.0")
                throw new ArgumentException(
                    "Invalid data format ('" + line + "'). " +
                    "Should be binary/little endian.");

            // Read header contents.
            for (var skip = false; ;)
            {
                // Read a line and split it with white space.
                line = reader.ReadLine();
                readCount += line.Length + 1;
                if (line == "end_header") break;
                var col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element")
                {
                    if (col[1] == "vertex")
                    {
                        data.vertexCount = Convert.ToInt32(col[2]);
                        skip = false;
                    }
                    else
                    {
                        // Don't read elements other than vertices.
                        skip = true;
                    }
                }

                if (skip) continue;

                // Property declaration line
                if (col[0] == "property")
                {
                    var prop = DataProperty.Invalid;

                    // Parse the property name entry.
                    switch (col[2])
                    {
                        case "red": prop = DataProperty.R8; break;
                        case "green": prop = DataProperty.G8; break;
                        case "blue": prop = DataProperty.B8; break;
                        case "alpha": prop = DataProperty.A8; break;
                        case "x": prop = DataProperty.SingleX; break;
                        case "y": prop = DataProperty.SingleY; break;
                        case "z": prop = DataProperty.SingleZ; break;
                    }

                    // Check the property type.
                    if (col[1] == "char" || col[1] == "uchar" ||
                        col[1] == "int8" || col[1] == "uint8")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data8;
                        else if (GetPropertySize(prop) != 1)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "short" || col[1] == "ushort" ||
                                col[1] == "int16" || col[1] == "uint16")
                    {
                        switch (prop)
                        {
                            case DataProperty.Invalid: prop = DataProperty.Data16; break;
                            case DataProperty.R8: prop = DataProperty.R16; break;
                            case DataProperty.G8: prop = DataProperty.G16; break;
                            case DataProperty.B8: prop = DataProperty.B16; break;
                            case DataProperty.A8: prop = DataProperty.A16; break;
                        }
                        if (GetPropertySize(prop) != 2)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int" || col[1] == "uint" || col[1] == "float" ||
                                col[1] == "int32" || col[1] == "uint32" || col[1] == "float32")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data32;
                        else if (GetPropertySize(prop) != 4)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int64" || col[1] == "uint64" ||
                                col[1] == "double" || col[1] == "float64")
                    {
                        switch (prop)
                        {
                            case DataProperty.Invalid: prop = DataProperty.Data64; break;
                            case DataProperty.SingleX: prop = DataProperty.DoubleX; break;
                            case DataProperty.SingleY: prop = DataProperty.DoubleY; break;
                            case DataProperty.SingleZ: prop = DataProperty.DoubleZ; break;
                        }
                        if (GetPropertySize(prop) != 8)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported property type ('" + line + "').");
                    }

                    data.properties.Add(prop);
                }
            }

            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return data;
        }

        DataBody ReadDataBody(DataHeader header, BinaryReader reader)
        {
            var data = new DataBody(header.vertexCount);

            float x = 0, y = 0, z = 0;
            byte r = 255, g = 255, b = 255, a = 255;

            for (var i = 0; i < header.vertexCount; i++)
            {
                foreach (var prop in header.properties)
                {
                    switch (prop)
                    {
                        case DataProperty.R8: r = reader.ReadByte(); break;
                        case DataProperty.G8: g = reader.ReadByte(); break;
                        case DataProperty.B8: b = reader.ReadByte(); break;
                        case DataProperty.A8: a = reader.ReadByte(); break;

                        case DataProperty.R16: r = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.G16: g = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.B16: b = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.A16: a = (byte)(reader.ReadUInt16() >> 8); break;

                        case DataProperty.SingleX: x = reader.ReadSingle(); break;
                        case DataProperty.SingleY: y = reader.ReadSingle(); break;
                        case DataProperty.SingleZ: z = reader.ReadSingle(); break;

                        case DataProperty.DoubleX: x = (float)reader.ReadDouble(); break;
                        case DataProperty.DoubleY: y = (float)reader.ReadDouble(); break;
                        case DataProperty.DoubleZ: z = (float)reader.ReadDouble(); break;

                        case DataProperty.Data8: reader.ReadByte(); break;
                        case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                        case DataProperty.Data32: reader.BaseStream.Position += 4; break;
                        case DataProperty.Data64: reader.BaseStream.Position += 8; break;
                    }
                }

                data.AddPoint(x, y, z, r, g, b, a);
            }

            return data;
        }
    }
}

#endregion
