using JpmodLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace DoubleFine
{
  public class HFile
  {
    public const byte Uncompressed = 4;
    public const byte Compressed = 8;
    private bool debug;
    public HFile.HFileHeader Header;
    public FileTypeIndex[] FileTypes;
    public FileIndex[] Files;
    public byte[] FooterData;
    private bool EndOfFileNameList;

    public void Load(string path)
    {
      Hashtable section1 = ConfigurationManager.GetSection("FileTypeExtension") as Hashtable;
      Hashtable section2 = ConfigurationManager.GetSection("HasSizeHeader") as Hashtable;
      using (EnhancedBinaryReader enhancedBinaryReader = new EnhancedBinaryReader((Stream) new FileStream(path, FileMode.Open), Encoding.ASCII, Endian.BigEndian))
      {
        this.Header = new HFile.HFileHeader((BinaryReader) enhancedBinaryReader);
        enhancedBinaryReader.BaseStream.Seek((long) this.Header.FileTypeOffset, SeekOrigin.Begin);
        List<FileTypeIndex> source1 = new List<FileTypeIndex>();
        for (int index = 0; index < this.Header.FileTypeCount; ++index)
          source1.Add(new FileTypeIndex((BinaryReader) enhancedBinaryReader));
        this.FileTypes = source1.ToArray<FileTypeIndex>();
        for (int index = 0; index < this.FileTypes.Length; ++index)
        {
          if (section1.ContainsKey((object) this.FileTypes[index].Name))
            this.FileTypes[index].Extension = (string) section1[(object) this.FileTypes[index].Name];
          if (section2.ContainsKey((object) this.FileTypes[index].Name))
            this.FileTypes[index].HasSizeHeader = true;
        }
        if (this.debug)
          File.Delete("debug.log");
        enhancedBinaryReader.BaseStream.Seek((long) this.Header.FileIndexOffset, SeekOrigin.Begin);
        List<FileIndex> source2 = new List<FileIndex>();
        for (int index = 0; index < this.Header.FileCount; ++index)
        {
          if ((int) this.Header.Version[0] == 5)
            source2.Add((FileIndex) new StackingFileIndex((BinaryReader) enhancedBinaryReader));
          else if ((int) this.Header.Version[0] == 2)
            source2.Add((FileIndex) new CostumeQuestFileIndex((BinaryReader) enhancedBinaryReader));
        }
        this.Files = source2.ToArray<FileIndex>();
        for (int index = 0; index < this.Header.FileCount; ++index)
        {
          enhancedBinaryReader.BaseStream.Seek((long) this.Header.FileNameOffset + (long) this.Files[index].FileNameOffset, SeekOrigin.Begin);
          this.Files[index].FilePathString = this.getString((BinaryReader) enhancedBinaryReader);
          int fileTypeIndex = (int) this.Files[index].FileTypeIndex;
          if ((int) this.Header.Version[0] == 5)
            fileTypeIndex >>= 1;
          if (fileTypeIndex >= 0 && fileTypeIndex < this.FileTypes.Length)
            this.Files[index].FileType = this.FileTypes[fileTypeIndex];
        }
        if (enhancedBinaryReader.BaseStream.Length - enhancedBinaryReader.BaseStream.Position > 0L)
          this.FooterData = enhancedBinaryReader.ReadBytes((int) (enhancedBinaryReader.BaseStream.Length - enhancedBinaryReader.BaseStream.Position));
        else
          this.FooterData = new byte[0];
      }
    }

    public void Save(string path)
    {
      using (EnhancedBinaryWriter enhancedBinaryWriter = new EnhancedBinaryWriter((Stream) new FileStream(path + ".~h", FileMode.Create), Encoding.ASCII, Endian.BigEndian))
      {
        this.Header.Write((BinaryWriter) enhancedBinaryWriter);
        enhancedBinaryWriter.Write(new byte[(long) this.Header.FileTypeOffset - enhancedBinaryWriter.BaseStream.Position]);
        foreach (FileTypeIndex fileType in this.FileTypes)
          fileType.Write((BinaryWriter) enhancedBinaryWriter);
        while (enhancedBinaryWriter.BaseStream.Position < (long) this.Header.FileIndexOffset)
          enhancedBinaryWriter.Write((byte) 204);
        foreach (FileIndex file in this.Files)
          file.Write((BinaryWriter) enhancedBinaryWriter);
        List<string> stringList = new List<string>();
        foreach (FileIndex file in this.Files)
        {
          if (!stringList.Contains(file.FilePathString))
          {
            enhancedBinaryWriter.Write(Encoding.ASCII.GetBytes(file.FilePathString + (object) char.MinValue));
            stringList.Add(file.FilePathString);
          }
        }
        enhancedBinaryWriter.Write(this.FooterData);
      }
    }

    private string getString(BinaryReader r)
    {
      string str = "";
      if (this.EndOfFileNameList || r.BaseStream.Position >= (long) this.Header.FooterOffset1)
        return (string) null;
      byte num;
      while ((int) (num = r.ReadByte()) != 0)
      {
        if ((int) num == 204)
        {
          this.EndOfFileNameList = true;
          return (string) null;
        }
        str += Convert.ToChar(num).ToString();
      }
      return str;
    }

    protected static int read3BytesAsInt(BinaryReader r)
    {
      byte[] numArray = new byte[4]
      {
        (byte) 0,
        r.ReadByte(),
        r.ReadByte(),
        r.ReadByte()
      };
      Array.Reverse((Array) numArray);
      return BitConverter.ToInt32(numArray, 0);
    }

    protected static byte[] convIntAs3Bytes(long l)
    {
      return HFile.convIntAs3Bytes((int) l);
    }

    protected static byte[] convIntAs3Bytes(int i)
    {
      List<byte> source = new List<byte>((IEnumerable<byte>) BitConverter.GetBytes(i));
      source.Reverse();
      source.RemoveAt(0);
      return source.ToArray<byte>();
    }

    public struct HFileHeader
    {
      public byte[] Magic;
      public byte[] Version;
      public ulong FileTypeOffset;
      public ulong FileNameOffset;
      public int FileTypeCount;
      public int FileNameLength;
      public int FileCount;
      public byte[] Delim1;
      public ulong UnknownOffset;
      public ulong DataFooterOffset;
      public ulong FileIndexOffset;
      public ulong FooterOffset1;
      public ulong FooterOffset2;
      public int UnknownCount;
      public byte[] Delim2;

      public HFileHeader(BinaryReader r)
      {
        this.Magic = r.ReadBytes(4);
        this.Version = r.ReadBytes(4);
        this.FileTypeOffset = r.ReadUInt64();
        this.FileNameOffset = r.ReadUInt64();
        this.FileTypeCount = r.ReadInt32();
        this.FileNameLength = r.ReadInt32();
        this.FileCount = r.ReadInt32();
        this.Delim1 = r.ReadBytes(4);
        this.UnknownOffset = r.ReadUInt64();
        this.DataFooterOffset = r.ReadUInt64();
        this.FileIndexOffset = r.ReadUInt64();
        this.FooterOffset1 = r.ReadUInt64();
        this.FooterOffset2 = r.ReadUInt64();
        this.UnknownCount = r.ReadInt32();
        this.Delim2 = r.ReadBytes(4);
      }

      public void Write(BinaryWriter w)
      {
        w.Write(this.Magic);
        w.Write(this.Version);
        w.Write(this.FileTypeOffset);
        w.Write(this.FileNameOffset);
        w.Write(this.FileTypeCount);
        w.Write(this.FileNameLength);
        w.Write(this.FileCount);
        w.Write(this.Delim1);
        w.Write(this.UnknownOffset);
        w.Write(this.DataFooterOffset);
        w.Write(this.FileIndexOffset);
        w.Write(this.FooterOffset1);
        w.Write(this.FooterOffset2);
        w.Write(this.UnknownCount);
        w.Write(this.Delim2);
      }
    }
  }
}
