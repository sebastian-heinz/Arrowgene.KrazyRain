using System.Collections.Generic;

namespace Arrowgene.KrazyRain.VirtualDisk
{
    public class VFile
    {
        /***
         * 0 = File
         * 1 = Folder
         */
        public byte Type { get; set; }
        public uint EntryOffset { get; set; }
        public uint EntryOffsetEnd { get; set; }
        public uint FolderOffset { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public byte[] DataCompressed { get; set; }
        public uint DataOffset { get; set; }
        public uint DataLengthCompressed { get; set; }
        public uint DataLengthUncompressed { get; set; }
        public string Path { get; set; }
        public bool HasEntryOffsetEnd { get; set; }
        public List<VFile> Files { get; }
        public List<VFile> Folders { get; }
        public VFile ParentFolder { get; set; }
        public bool Written { get; set; }

        public VFile()
        {
            Written = false;
            Type = 0;
            EntryOffset = 0;
            EntryOffsetEnd = 0;
            FolderOffset = 0;
            DataOffset = 0;
            Path = null;
            Name = null;
            Data = null;
            DataLengthCompressed = 0;
            DataLengthUncompressed = 0;
            HasEntryOffsetEnd = true;
            Files = new List<VFile>();
            Folders = new List<VFile>();
            ParentFolder = null;
        }

        public string Dump()
        {
            return
                "\r\n" +
                $"Type:{Type}\r\n" +
                $"Name:{Name}\r\n" +
                $"DataLengthUncompressed:{DataLengthUncompressed}\r\n" +
                $"DataLengthCompressed:{DataLengthCompressed}\r\n" +
                $"FolderOffset:{FolderOffset}\r\n" +
                $"EntryOffsetEnd:{EntryOffsetEnd}\r\n" +
                $"DataOffset:{DataOffset}\r\n" +
                $"EntryOffset:{EntryOffset}\r\n" +
                $"Path:{Path}\r\n";
        }
    }
}