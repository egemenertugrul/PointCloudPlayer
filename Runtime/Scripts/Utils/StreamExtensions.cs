using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PCP.Utils
{
    public static class StreamExtensions
    {
        public static Stream GenerateStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static Stream GenerateStream(this byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return stream;
        }
    }
}