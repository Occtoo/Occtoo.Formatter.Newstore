using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Occtoo.Formatter.Newstore.Services
{
    public class ZipHelper
    {
        public static MemoryStream CreateZip(List<ZipContentsEntry> entries)
        {
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var entry in entries)
                {
                    var archiveEntry = archive.CreateEntry(entry.FullPath);
                    using var entryStream = archiveEntry.Open();
                    using var streamWriter = new StreamWriter(entryStream);
                    streamWriter.Write(entry.Contents);
                }
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        public class ZipContentsEntry
        {
            public string FullPath { get; set; }
            public string Contents { get; set; }
            public string Locale { get; set; }

        }
    }
}
