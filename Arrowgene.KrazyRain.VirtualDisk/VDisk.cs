using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Arrowgene.Buffers;
using Arrowgene.Logging;

namespace Arrowgene.KrazyRain.VirtualDisk
{
    public class VDisk
    {
        private static readonly ILogger Logger = LogProvider.Logger<Logger>(typeof(VDisk));

        private const string MagicBytes = "VDISK1.0";
        private const int HeaderSize = 24;
        private static readonly byte[] ZLibHeader = new byte[2] {0x78, 0x01};
        private const int Modulus = 65521;
        private const int ChecksumLength = 4;
        private const int EntryBlockSize = 145;

        private readonly Dictionary<uint, VFile> _folderEntries;
        private readonly Dictionary<string, VFile> _fileEntries;
        private ushort _unknown1;
        private byte _unknown2;

        // Size of file without root block (filesize - EntryBlockSize)
        private uint _size;

        public VDisk()
        {
            _folderEntries = new Dictionary<uint, VFile>();
            _fileEntries = new Dictionary<string, VFile>();
            _unknown1 = 15235;
            _unknown2 = 3;
            _size = 0;
        }

        public bool Open(string filePath)
        {
            _folderEntries.Clear();
            _fileEntries.Clear();

            IBuffer buffer = new StreamBuffer(filePath);
            buffer.SetPositionStart();
            string header = buffer.ReadCString();
            if (header != MagicBytes)
            {
                Logger.Error($"Wrong Header. (FoundHeader:{header} ExpectedHeader:{MagicBytes})");
                return false;
            }

            _unknown1 = buffer.ReadUInt16();
            _unknown2 = buffer.ReadByte();
            uint filesCount = buffer.ReadUInt32();
            uint folderCount = buffer.ReadUInt32();
            _size = buffer.ReadUInt32();

            VFile currentFolder = null;
            while (buffer.Position < buffer.Size)
            {
                VFile vFile = new VFile();
                vFile.EntryOffset = (uint) buffer.Position;
                vFile.Type = buffer.ReadByte();
                vFile.Name = buffer.ReadFixedString(128);
                vFile.DataLengthUncompressed = buffer.ReadUInt32();
                vFile.DataLengthCompressed = buffer.ReadUInt32();
                if (vFile.DataLengthCompressed > int.MaxValue)
                {
                    Logger.Error($"DataLengthCompressed:{vFile.DataLengthCompressed} not supported");
                    return false;
                }

                if (vFile.Type == 0 && vFile.DataLengthUncompressed == 0)
                {
                    Logger.Error($"File without data. (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})");
                    return false;
                }

                vFile.FolderOffset = buffer.ReadUInt32();
                vFile.EntryOffsetEnd = buffer.ReadUInt32();

                switch (vFile.Type)
                {
                    case 0:
                    {
                        vFile.Path = $"{currentFolder.Path}/{vFile.Name}".Trim('/');
                        break;
                    }
                    case 1:
                    {
                        break;
                    }
                    default:
                    {
                        Logger.Error(
                            $"Unknown Type: {vFile.Type}. (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})");
                        if (vFile.DataLengthCompressed > 0 && buffer.Position != vFile.EntryOffsetEnd)
                        {
                            Logger.Error($"Adjusting offset from:{buffer.Position} to:{vFile.EntryOffsetEnd}");
                            buffer.Position = (int) vFile.EntryOffsetEnd;
                        }

                        continue;
                    }
                }

                if (vFile.DataLengthCompressed > 0)
                {
                    vFile.DataOffset = (uint) buffer.Position;
                    byte[] zlibHeader = buffer.ReadBytes(2);
                    uint compressedLength = (uint) (vFile.DataLengthCompressed - zlibHeader.Length - ChecksumLength);
                    byte[] compressed = buffer.ReadBytes((int) compressedLength);
                    byte[] adler32Checksum = buffer.ReadBytes(ChecksumLength);
                    byte[] uncompressed = null;
                    try
                    {
                        using MemoryStream output = new MemoryStream();
                        using MemoryStream input = new MemoryStream(compressed, 0, compressed.Length);
                        using DeflateStream decompressionStream = new DeflateStream(input, CompressionMode.Decompress);
                        decompressionStream.CopyTo(output);
                        uncompressed = new byte[output.Length];
                        output.Position = 0;
                        output.Read(uncompressed, 0, uncompressed.Length);
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex);
                    }

                    if (uncompressed == null)
                    {
                        Logger.Error($"Failed to decompress. (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})");
                        if (vFile.DataLengthCompressed > 0 && buffer.Position != vFile.EntryOffsetEnd)
                        {
                            Logger.Error($"Adjusting offset from:{buffer.Position} to:{vFile.EntryOffsetEnd}");
                            buffer.Position = (int) vFile.EntryOffsetEnd;
                        }

                        continue;
                    }

                    int adler32Calculated = Adler32(uncompressed, 0, uncompressed.Length);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(adler32Checksum);
                    }

                    int adler32Expected = (int) BitConverter.ToUInt32(adler32Checksum);
                    if (adler32Calculated != adler32Expected)
                    {
                        Logger.Error($"Checksum: Calculated:{adler32Calculated} != Expected:{adler32Expected}" +
                                     $" (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})"
                        );
                        if (vFile.DataLengthCompressed > 0 && buffer.Position != vFile.EntryOffsetEnd)
                        {
                            Logger.Error($"Adjusting offset from:{buffer.Position} to:{vFile.EntryOffsetEnd}");
                            buffer.Position = (int) vFile.EntryOffsetEnd;
                        }
                    }

                    vFile.Data = uncompressed;

                    IBuffer bufferC = new StreamBuffer();
                    bufferC.WriteBytes(ZLibHeader);
                    bufferC.WriteBytes(compressed);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(adler32Checksum);
                    }

                    bufferC.WriteBytes(adler32Checksum);
                    vFile.DataCompressed = bufferC.GetAllBytes();
                }

                if (vFile.DataLengthCompressed > 0 && buffer.Position != vFile.EntryOffsetEnd)
                {
                    // Apparently if the end offset of the block is 0, we traverse a directory up.
                    // Logger.Error(
                    //    $"Position:{buffer.Position} != EntryOffsetEnd:{vFile.EntryOffsetEnd} (Name:{vFile.Name})");
                    int lastIndex = currentFolder.Path.LastIndexOf(
                        $"/{currentFolder.Name}",
                        StringComparison.InvariantCultureIgnoreCase
                    );
                    string parentFolderPath = ""; // default root path
                    if (lastIndex >= 0)
                    {
                        parentFolderPath = currentFolder.Path.Substring(0, lastIndex);
                    }

                    bool found = false;
                    foreach (VFile parentFolder in _folderEntries.Values)
                    {
                        if (parentFolder.Path == parentFolderPath)
                        {
                            currentFolder = parentFolder;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Logger.Error(
                            $"Could not find parent directory. (CurrentPath:{currentFolder.Path} ParentPath:{parentFolderPath})");
                    }
                }

                switch (vFile.Type)
                {
                    case 0:
                    {
                        _fileEntries.Add(vFile.Path, vFile);
                        Logger.Info($"Read: {vFile.Path}");
                        break;
                    }
                    case 1:
                    {
                        if (vFile.Name == ".")
                        {
                            // set current ?
                            if (currentFolder == null)
                            {
                                // Root Folder
                                vFile.Path = "";
                                currentFolder = vFile;
                                _folderEntries.Add(vFile.FolderOffset, currentFolder);
                            }
                        }
                        else if (vFile.Name == "..")
                        {
                            // update parent
                            if (!_folderEntries.ContainsKey(vFile.FolderOffset))
                            {
                                Logger.Error(
                                    $"Parent not found. (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})");
                            }
                            else
                            {
                                VFile parentFolder = _folderEntries[vFile.FolderOffset];
                                currentFolder.Path = $"{parentFolder.Path}/{currentFolder.Name}".Trim('/');
                            }
                        }
                        else
                        {
                            // register
                            if (_folderEntries.ContainsKey(vFile.FolderOffset))
                            {
                                Logger.Error(
                                    $"Folder already registered. (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})");
                            }
                            else
                            {
                                _folderEntries.Add(vFile.FolderOffset, vFile);
                            }

                            currentFolder = vFile;
                        }

                        break;
                    }
                    default:
                    {
                        Logger.Error(
                            $"Unknown Type: {vFile.Type}. (Name:{vFile.Name} EntryOffset:{vFile.EntryOffset})");
                        break;
                    }
                }

                //  Logger.Info(vFile.Dump());
            }

            Logger.Info($"Open Completed ({filePath})");
            return true;
        }

        public void Save(string filePath)
        {
            _folderEntries.Clear();
            _size = HeaderSize;
            uint fileCount = 0;
            uint folderCount = 0;

            List<VFile> vFiles = new List<VFile>(_fileEntries.Values);
            vFiles.Sort(
                (x, y) => { return String.Compare(x.Path, y.Path, StringComparison.InvariantCultureIgnoreCase); }
            );

            VFile rootDirectory = new VFile();
            rootDirectory.Type = 1;
            rootDirectory.Name = ".";
            rootDirectory.Path = "";


            Dictionary<string, VFile> directories = new Dictionary<string, VFile>();
            foreach (VFile vFile in vFiles)
            {
                if (vFile.Type != 0)
                {
                    Logger.Error("Invalid Type");
                    continue;
                }

                string virtualDirectory = vFile.Path.Replace(vFile.Name, "");
                virtualDirectory = virtualDirectory.Trim('/');

                string currentDirectory = "";
                string parentDirectory = null;
                string[] dirs = virtualDirectory.Split('/');
                foreach (string dir in dirs)
                {
                    // try create all missing folder of file path
                    if (!currentDirectory.EndsWith('/'))
                    {
                        currentDirectory += '/';
                    }

                    currentDirectory += dir;
                    currentDirectory = currentDirectory.Trim('/');

                    if (!directories.ContainsKey(currentDirectory))
                    {
                        // create folder
                        string folderName;
                        int lastIndex = currentDirectory.LastIndexOf('/');
                        if (lastIndex >= 0)
                        {
                            lastIndex++;
                            int length = currentDirectory.Length - lastIndex;
                            folderName = currentDirectory.Substring(lastIndex, length);
                        }
                        else
                        {
                            folderName = currentDirectory;
                        }

                        VFile vFileDirectory = new VFile();
                        vFileDirectory.Type = 1;
                        vFileDirectory.Name = folderName;
                        vFileDirectory.Path = currentDirectory;
                        directories.Add(currentDirectory, vFileDirectory);

                        // find parent
                        if (parentDirectory != null && directories.ContainsKey(parentDirectory))
                        {
                            VFile parent = directories[parentDirectory];
                            parent.Folders.Add(vFileDirectory);
                            vFileDirectory.ParentFolder = parent;
                        }
                        else
                        {
                            rootDirectory.Folders.Add(vFileDirectory);
                            vFileDirectory.ParentFolder = rootDirectory;
                        }

                        folderCount++;
                        _size += EntryBlockSize * 3;
                    }

                    parentDirectory = currentDirectory;
                }

                if (!directories.ContainsKey(virtualDirectory))
                {
                    Logger.Error("File has no parent folder");
                    continue;
                }

                VFile directory = directories[virtualDirectory];
                directory.Files.Add(vFile);
                CompressZlib(vFile);
                vFile.ParentFolder = directory;
                fileCount++;
                _size += EntryBlockSize + vFile.DataLengthCompressed;
            }

            IBuffer buffer = new StreamBuffer();
            buffer.WriteCString(MagicBytes);
            buffer.WriteUInt16(_unknown1);
            buffer.WriteByte(_unknown2);
            buffer.WriteUInt32(fileCount);
            buffer.WriteUInt32(folderCount);
            buffer.WriteUInt32(_size);

            for (int i = 0; i < vFiles.Count; i++)
            {
                VFile vFile = vFiles[i];
                if (i + 1 < vFiles.Count)
                {
                    VFile vFileNext = vFiles[i + 1];
                    if (vFileNext.ParentFolder != vFile.ParentFolder)
                    {
                        vFile.HasEntryOffsetEnd = false;
                    }
                }

                WriteVFolder(vFile.ParentFolder, buffer);
                WriteVFile(vFile, buffer);
            }

            byte[] result = buffer.GetAllBytes();
            File.WriteAllBytes(filePath, result);
        }

        private void WriteVFolder(VFile root, IBuffer buffer)
        {
            if (root.Written)
            {
                return;
            }

            if (root.ParentFolder != null)
            {
                WriteVFolder(root.ParentFolder, buffer);
            }


            root.EntryOffset = (uint) buffer.Position;

            uint totalDataLength = EntrySize(root);


            if (root.ParentFolder != null)
            {
                root.FolderOffset = (uint) buffer.Position + EntryBlockSize;
                uint dotEntryOffsetEnd = root.FolderOffset + EntryBlockSize;
                uint dotDotEntryOffsetEnd = dotEntryOffsetEnd + EntryBlockSize;
                root.EntryOffsetEnd = dotDotEntryOffsetEnd + totalDataLength;
                if (root.Name == "DATA")
                {
                    root.EntryOffsetEnd = 0;
                }

                //root.EntryOffsetEnd = (uint) buffer.Position + FolderBlockSize;

                buffer.WriteByte(root.Type);
                buffer.WriteFixedString(root.Name, 128);
                buffer.WriteUInt32(root.DataLengthUncompressed);
                buffer.WriteUInt32(root.DataLengthCompressed);
                buffer.WriteUInt32(root.FolderOffset);
                buffer.WriteUInt32(root.EntryOffsetEnd);

                // .
                buffer.WriteByte(root.Type);
                buffer.WriteFixedString(".", 128);
                buffer.WriteUInt32(root.DataLengthUncompressed);
                buffer.WriteUInt32(root.DataLengthCompressed);
                buffer.WriteUInt32(root.FolderOffset);
                buffer.WriteUInt32(dotEntryOffsetEnd);

                // ..
                buffer.WriteByte(root.ParentFolder.Type);
                buffer.WriteFixedString("..", 128);
                buffer.WriteUInt32(root.ParentFolder.DataLengthUncompressed);
                buffer.WriteUInt32(root.ParentFolder.DataLengthCompressed);
                buffer.WriteUInt32(root.ParentFolder.FolderOffset);
                buffer.WriteUInt32(dotDotEntryOffsetEnd);
            }
            else
            {
                root.FolderOffset = (uint) buffer.Position;
                root.EntryOffsetEnd = (uint) buffer.Position + EntryBlockSize;
                buffer.WriteByte(root.Type);
                buffer.WriteFixedString(root.Name, 128);
                buffer.WriteUInt32(root.DataLengthUncompressed);
                buffer.WriteUInt32(root.DataLengthCompressed);
                buffer.WriteUInt32(root.FolderOffset);
                buffer.WriteUInt32(root.EntryOffsetEnd);
            }

            root.Written = true;
        }

        private uint EntrySize(VFile entry)
        {
            uint size = 0;
            foreach (VFile dir in entry.Folders)
            {
                size += EntrySize(dir);
                size += EntryBlockSize * 3;
            }

            foreach (VFile file in entry.Files)
            {
                size += EntryBlockSize;
                size += file.DataLengthCompressed;
            }

            return size;
        }

        private void WriteVFile(VFile vFile, IBuffer buffer)
        {
            vFile.EntryOffset = (uint) buffer.Position;
            vFile.EntryOffsetEnd = vFile.EntryOffset + vFile.DataLengthCompressed + EntryBlockSize;

            buffer.WriteByte(vFile.Type);
            buffer.WriteFixedString(vFile.Name, 128);
            buffer.WriteUInt32(vFile.DataLengthUncompressed);
            buffer.WriteUInt32(vFile.DataLengthCompressed);
            buffer.WriteUInt32(vFile.FolderOffset);
            if (!vFile.HasEntryOffsetEnd && vFile.Name != "ADInfo.xml")
            {
                vFile.EntryOffsetEnd = 0;
                Logger.Info($"Clearing:{vFile.Path}");
            }

            if (vFile.Name == "StringTable.txt")
            {
                vFile.EntryOffsetEnd = 0;
                Logger.Info($"Clearing:{vFile.Path}");
            }


            buffer.WriteUInt32(vFile.EntryOffsetEnd);
            buffer.WriteBytes(vFile.DataCompressed);
        }


        private void DecompressZlib()
        {
        }

        private void DecompressDeflate()
        {
        }

        private void CompressZlib(VFile vFile)
        {
            if (vFile.DataCompressed != null)
            {
                return;
            }

            if (vFile.Type != 0)
            {
                return;
            }

            if (vFile.Data == null || vFile.Data.Length <= 0)
            {
                return;
            }

            byte[] compressed = null;
            try
            {
                using MemoryStream input = new MemoryStream(vFile.Data, 0, vFile.Data.Length);
                using MemoryStream output = new MemoryStream();

                using zlib.ZOutputStream compressionStream = new zlib.ZOutputStream(
                    output, zlib.zlibConst.Z_DEFAULT_COMPRESSION
                );
                input.Position = 0;
                byte[] buffer = new byte[2000];
                int len;
                while ((len = input.Read(buffer, 0, 2000)) > 0)
                {
                    compressionStream.Write(buffer, 0, len);
                }

                compressionStream.Flush();
                compressionStream.finish();

                compressed = new byte[output.Length];
                output.Position = 0;
                output.Read(compressed, 0, compressed.Length);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            if (compressed == null)
            {
                Logger.Error($"Failed to compress. (Name:{vFile.Name}");
                vFile.DataLengthUncompressed = 0;
                vFile.DataLengthCompressed = 0;
                vFile.DataCompressed = null;
                return;
            }

            vFile.DataLengthUncompressed = (uint) vFile.Data.Length;
            vFile.DataLengthCompressed = (uint) compressed.Length;
            vFile.DataCompressed = compressed;
        }

        private void CompressDeflate(VFile vFile)
        {
            if (vFile.DataCompressed != null)
            {
                return;
            }

            if (vFile.Type == 0 && vFile.Data != null && vFile.Data.Length > 0)
            {
                int adler32Calculated = Adler32(vFile.Data, 0, vFile.Data.Length);
                byte[] adler32Checksum = BitConverter.GetBytes(adler32Calculated);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(adler32Checksum);
                }

                byte[] compressed = null;
                try
                {
                    using MemoryStream input = new MemoryStream(vFile.Data, 0, vFile.Data.Length);
                    using MemoryStream output = new MemoryStream();
                    using DeflateStream compressionStream = new DeflateStream(output, CompressionMode.Compress);
                    //   using zlib.ZOutputStream compressionStream = new zlib.ZOutputStream(output, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
                    // CopyStream(input, compressionStream);
                    //  compressionStream.finish();

                    input.Position = 0;
                    input.CopyTo(compressionStream);
                    compressionStream.Flush();

                    compressed = new byte[output.Length];
                    output.Position = 0;
                    output.Read(compressed, 0, compressed.Length);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }

                if (compressed == null)
                {
                    Logger.Error($"Failed to compress. (Name:{vFile.Name}");
                    vFile.DataLengthUncompressed = 0;
                    vFile.DataLengthCompressed = 0;
                    vFile.DataCompressed = null;
                    return;
                }

                IBuffer buffer = new StreamBuffer();
                buffer.WriteBytes(ZLibHeader);
                buffer.WriteBytes(compressed);
                buffer.WriteBytes(adler32Checksum);
                compressed = buffer.GetAllBytes();

                vFile.DataLengthUncompressed = (uint) vFile.Data.Length;
                vFile.DataLengthCompressed = (uint) compressed.Length;
                vFile.DataCompressed = compressed;
            }
        }

        public bool AddFolder(string directoryPath, string virtualRootDirectory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                AddFile(file, directoryInfo, virtualRootDirectory);
            }

            return true;
        }

        public VFile AddFile(FileInfo file, DirectoryInfo rootDirectoryPath, string virtualRootDirectory)
        {
            DirectoryInfo fileDirectoryInfo = file.Directory;
            if (fileDirectoryInfo == null)
            {
                Logger.Error($"Failed to add file. (Fullname:{file.FullName})");
                return null;
            }

            virtualRootDirectory = virtualRootDirectory.Replace("\\", "/");
            string virtualDirectory = fileDirectoryInfo.FullName.Replace(rootDirectoryPath.FullName, "");
            virtualDirectory = virtualDirectory.Replace("\\", "/");
            virtualDirectory = virtualDirectory.Trim('/');
            virtualDirectory = $"{virtualRootDirectory}/{virtualDirectory}";
            virtualDirectory = virtualDirectory.Trim('/');
            string virtualPath = $"{virtualDirectory}/{file.Name}";

            if (_fileEntries.ContainsKey(virtualPath))
            {
                Logger.Error($"File already exists. (VirtualPath:{virtualPath})");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(file.FullName);

            VFile vFile = new VFile();
            vFile.Data = fileData;
            vFile.Path = virtualPath;
            vFile.Name = file.Name;
            vFile.Type = 0;

            _fileEntries.Add(vFile.Path, vFile);

            return vFile;
        }

        public VFile GetFile(string virtualPath)
        {
            return null;
        }

        public bool RemoveFile(VFile vFile)
        {
            return true;
        }

        public bool Extract(string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Logger.Error($"Destination directory does not exists. ({destinationDirectory})");
                return false;
            }

            foreach (VFile vFile in _fileEntries.Values)
            {
                if (vFile.Path.StartsWith('/'))
                {
                    Logger.Info($"Problematic file path assigned: {vFile.Path}, fixed");
                    vFile.Path.Trim('/');
                }

                string filePath = Path.Combine(destinationDirectory, vFile.Path);
                FileInfo fileInfo = new FileInfo(filePath);
                if (!filePath.StartsWith(destinationDirectory))
                {
                    Logger.Info(
                        $"Failed to build correct file path for:{vFile.Name}. (Expected:{destinationDirectory}/{vFile.Path} but will do:{fileInfo.FullName}");
                }

                DirectoryInfo directoryInfo = fileInfo.Directory;
                if (directoryInfo == null)
                {
                    Logger.Error($"Invalid directory. ({filePath})");
                    continue;
                }

                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                File.WriteAllBytes(filePath, vFile.Data);
                Logger.Info($"Extracted: {filePath}");
            }

            Logger.Info($"Extract Completed ({destinationDirectory})");
            return true;
        }

        private int Adler32(byte[] data, int offset, int length)
        {
            int a = 1;
            int b = 0;
            for (int counter = 0; counter < length; ++counter)
            {
                a = (a + (data[offset + counter])) % Modulus;
                b = (b + a) % Modulus;
            }

            return ((b * 65536) + a);
        }
    }
}