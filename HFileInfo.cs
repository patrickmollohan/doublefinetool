using JpmodLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DoubleFine
{
  public class HFileInfo
  {
    private HFile hFile;

    public HFileInfo()
    {
      this.hFile = new HFile();
    }

    public HFile Load(string path)
    {
      this.hFile.Header = this.loadHeaderFile(path);
      this.hFile.FileTypes = this.loadFileTypeFile(path);
      this.hFile.Files = this.loadFileIndexFile(path);
      for (int index = 0; index < this.hFile.Files.Length; ++index)
      {
        int fileTypeIndex = (int) this.hFile.Files[index].FileTypeIndex;
        if ((int) this.hFile.Header.Version[0] == 5)
          fileTypeIndex >>= 1;
        this.hFile.Files[index].FileType = this.hFile.FileTypes[fileTypeIndex];
      }
      this.hFile.FooterData = this.loadFooterData(path);
      return this.hFile;
    }

    public void Save(string path, HFile hFile)
    {
      this.saveHeader(path, hFile.Header);
      this.saveFileType(path, hFile.FileTypes);
      this.saveFileList(path, hFile.Files);
      this.saveFooterData(path, hFile.FooterData);
    }

    private HFile.HFileHeader loadHeaderFile(string path)
    {
      HFile.HFileHeader hfileHeader = new HFile.HFileHeader();
      using (StreamReader streamReader = new StreamReader(path + "\\header.txt"))
      {
        string[] strArray = streamReader.ReadToEnd().Split(',');
        hfileHeader.Magic = BitConverter.GetBytes(int.Parse(strArray[0]));
        hfileHeader.Version = BitConverter.GetBytes(int.Parse(strArray[1]));
        hfileHeader.FileTypeOffset = ulong.Parse(strArray[2]);
        hfileHeader.FileNameOffset = ulong.Parse(strArray[3]);
        hfileHeader.FileTypeCount = int.Parse(strArray[4]);
        hfileHeader.FileNameLength = int.Parse(strArray[5]);
        hfileHeader.FileCount = int.Parse(strArray[6]);
        hfileHeader.Delim1 = BitConverter.GetBytes(int.Parse(strArray[7]));
        hfileHeader.UnknownOffset = ulong.Parse(strArray[8]);
        hfileHeader.DataFooterOffset = ulong.Parse(strArray[9]);
        hfileHeader.FileIndexOffset = ulong.Parse(strArray[10]);
        hfileHeader.FooterOffset1 = ulong.Parse(strArray[11]);
        hfileHeader.FooterOffset2 = ulong.Parse(strArray[12]);
        hfileHeader.UnknownCount = int.Parse(strArray[13]);
        hfileHeader.Delim2 = BitConverter.GetBytes(int.Parse(strArray[14]));
      }
      return hfileHeader;
    }

    private FileTypeIndex[] loadFileTypeFile(string path)
    {
      Hashtable section1 = ConfigurationManager.GetSection("FileTypeExtension") as Hashtable;
      Hashtable section2 = ConfigurationManager.GetSection("HasSizeHeader") as Hashtable;
      List<FileTypeIndex> source = new List<FileTypeIndex>();
      using (CsvFileReader csvFileReader = new CsvFileReader(path + "\\filetype.txt"))
      {
        if (csvFileReader.Load())
        {
          foreach (string[] row in csvFileReader.Rows)
          {
            FileTypeIndex fileTypeIndex = new FileTypeIndex();
            fileTypeIndex.Name = row[0];
            fileTypeIndex.Unknown1 = int.Parse(row[1]);
            fileTypeIndex.Unknown2 = int.Parse(row[2]);
            fileTypeIndex.Unknown3 = int.Parse(row[3]);
            if (section1.ContainsKey((object) fileTypeIndex.Name))
              fileTypeIndex.Extension = (string) section1[(object) fileTypeIndex.Name];
            if (section2.ContainsKey((object) fileTypeIndex.Name))
              fileTypeIndex.HasSizeHeader = true;
            source.Add(fileTypeIndex);
          }
        }
      }
      return source.ToArray<FileTypeIndex>();
    }

    private FileIndex[] loadFileIndexFile(string path)
    {
      List<FileIndex> source = new List<FileIndex>();
      using (CsvFileReader csvFileReader = new CsvFileReader(path + "\\filelist.txt"))
      {
        if (csvFileReader.Load())
        {
          foreach (string[] row in csvFileReader.Rows)
          {
            FileIndex fileIndex = (FileIndex) null;
            if ((int) this.hFile.Header.Version[0] == 5)
              fileIndex = (FileIndex) new StackingFileIndex();
            else if ((int) this.hFile.Header.Version[0] == 2)
              fileIndex = (FileIndex) new CostumeQuestFileIndex();
            fileIndex.ContentSize = Convert.ToInt32(row[0], 16);
            fileIndex.FileNameOffset = Convert.ToInt32(row[1], 16);
            fileIndex.UnknownFlag1 = Convert.ToInt32(row[2], 16);
            fileIndex.UnknownFlag2 = Convert.ToByte(row[3], 16);
            fileIndex.FileTypeIndex = Convert.ToByte(row[4], 16);
            fileIndex.CompressFlag = Convert.ToByte(row[5], 16);
            fileIndex.Offset = Convert.ToInt64(row[6], 16);
            fileIndex.FilePathString = row[7];
            source.Add(fileIndex);
          }
        }
      }
      return source.ToArray<FileIndex>();
    }

    private byte[] loadFooterData(string path)
    {
      using (BinaryReader binaryReader = new BinaryReader((Stream) new FileStream(path + "\\footer.bin", FileMode.Open)))
        return binaryReader.ReadBytes((int) binaryReader.BaseStream.Length);
    }

    private void saveHeader(string path, HFile.HFileHeader header)
    {
      using (StreamWriter streamWriter = new StreamWriter(path + "\\header.txt"))
      {
        streamWriter.Write(BitConverter.ToInt32(header.Magic, 0));
        streamWriter.Write(",");
        streamWriter.Write(BitConverter.ToInt32(header.Version, 0));
        streamWriter.Write(",");
        streamWriter.Write(header.FileTypeOffset);
        streamWriter.Write(",");
        streamWriter.Write(header.FileNameOffset);
        streamWriter.Write(",");
        streamWriter.Write(header.FileTypeCount);
        streamWriter.Write(",");
        streamWriter.Write(header.FileNameLength);
        streamWriter.Write(",");
        streamWriter.Write(header.FileCount);
        streamWriter.Write(",");
        streamWriter.Write(BitConverter.ToInt32(header.Delim1, 0));
        streamWriter.Write(",");
        streamWriter.Write(header.UnknownOffset);
        streamWriter.Write(",");
        streamWriter.Write(header.DataFooterOffset);
        streamWriter.Write(",");
        streamWriter.Write(header.FileIndexOffset);
        streamWriter.Write(",");
        streamWriter.Write(header.FooterOffset1);
        streamWriter.Write(",");
        streamWriter.Write(header.FooterOffset2);
        streamWriter.Write(",");
        streamWriter.Write(header.UnknownCount);
        streamWriter.Write(",");
        streamWriter.Write(BitConverter.ToInt32(header.Delim2, 0));
      }
    }

    private void saveFileType(string path, FileTypeIndex[] fileTypes)
    {
      using (StreamWriter streamWriter = new StreamWriter(path + "\\filetype.txt"))
      {
        foreach (FileTypeIndex fileType in fileTypes)
        {
          streamWriter.Write(fileType.Name);
          streamWriter.Write(",");
          streamWriter.Write(fileType.Unknown1);
          streamWriter.Write(",");
          streamWriter.Write(fileType.Unknown2);
          streamWriter.Write(",");
          streamWriter.WriteLine(fileType.Unknown3);
        }
      }
    }

    private void saveFileList(string path, FileIndex[] fileIndex)
    {
      using (StreamWriter streamWriter = new StreamWriter(path + "\\filelist.txt"))
      {
        foreach (FileIndex fileIndex1 in fileIndex)
          streamWriter.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", (object) this.ToBinStr(fileIndex1.ContentSize), (object) this.ToBinStr(fileIndex1.FileNameOffset), (object) this.ToBinStr(fileIndex1.UnknownFlag1), (object) this.ToBinStr(fileIndex1.UnknownFlag2), (object) this.ToBinStr(fileIndex1.FileTypeIndex), (object) this.ToBinStr(fileIndex1.CompressFlag), (object) this.ToBinStr(fileIndex1.Offset), (object) fileIndex1.FilePathString, (object) fileIndex1.GetFilePath());
      }
    }

    private void saveFooterData(string path, byte[] footer)
    {
      using (BinaryWriter binaryWriter = new BinaryWriter((Stream) new FileStream(path + "\\footer.bin", FileMode.Create)))
        binaryWriter.Write(footer);
    }

    private string ToBinStr(byte b)
    {
      return "0x" + string.Format("{0:X2}", (object) b);
    }

    private string ToBinStr(byte[] b)
    {
      return "0x" + BitConverter.ToString(b).Replace("-", "");
    }

    private string ToBinStr(long l)
    {
      return "0x" + string.Format("{0:X16}", (object) l);
    }

    private string ToBinStr(int i)
    {
      return "0x" + string.Format("{0:X8}", (object) i);
    }

    private string ToBinStr(short s)
    {
      return "0x" + string.Format("{0:X4}", (object) s);
    }

    private string ToBinStr(ushort s)
    {
      return "0x" + string.Format("{0:X4}", (object) s);
    }
  }
}
