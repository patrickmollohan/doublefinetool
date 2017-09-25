using JpmodLib.Utils;
using System;
using System.IO;
using zlib;

namespace DoubleFine
{
  public class PFile
  {
    private const int ZINBUFSIZ = 32768;
    private const int ZOUTBUFSIZ = 32768;
    private HFile header;

    public PFile(HFile header)
    {
      this.header = header;
    }

    public void Unpack(string hFilePath, string unpackPath)
    {
      using (BinaryReader binaryReader = new BinaryReader((Stream) new FileStream(this.getPath(Path.GetDirectoryName(hFilePath), Path.GetFileNameWithoutExtension(hFilePath) + ".~p"), FileMode.Open)))
      {
        ConsoleProgressBar consoleProgressBar = new ConsoleProgressBar(this.header.Header.FileCount);
        try
        {
          consoleProgressBar.Start();
          foreach (FileIndex file in this.header.Files)
          {
            binaryReader.BaseStream.Seek(file.Offset, SeekOrigin.Begin);
            consoleProgressBar.PerformStep();
            Directory.CreateDirectory(Path.GetDirectoryName(this.getPath(unpackPath, file.GetFilePath())));
            using (MemoryStream memoryStream = new MemoryStream())
            {
              using (ZOutputStream zoutputStream = new ZOutputStream((Stream) memoryStream))
              {
                if (file.IsCompressed())
                  zoutputStream.Write(binaryReader.ReadBytes(file.Size), 0, file.Size);
                else
                  memoryStream.Write(binaryReader.ReadBytes(file.Size), 0, file.Size);
                memoryStream.Seek(0L, SeekOrigin.Begin);
                if ((long) file.ContentSize < memoryStream.Length)
                {
                  using (BinaryWriter binaryWriter = new BinaryWriter((Stream) new FileStream(this.getPath(unpackPath, file.GetFilePath() + ".header"), FileMode.Create)))
                  {
                    byte[] buffer = new byte[memoryStream.Length - (long) file.ContentSize];
                    memoryStream.Read(buffer, 0, buffer.Length);
                    binaryWriter.Write(buffer);
                  }
                }
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream) new FileStream(this.getPath(unpackPath, file.GetFilePath()), FileMode.Create)))
                {
                  int contentSize = file.ContentSize;
                  if (file.FileType.HasSizeHeader)
                    contentSize -= 4;
                  byte[] buffer = new byte[contentSize];
                  if (file.FileType.HasSizeHeader)
                    memoryStream.Read(buffer, 0, 4);
                  memoryStream.Read(buffer, 0, contentSize);
                  binaryWriter.Write(buffer);
                }
              }
            }
          }
        }
        finally
        {
          consoleProgressBar.End();
        }
      }
    }

    public void Pack(string path, string unpackPath, HFile hFile)
    {
      using (FileStream fileStream1 = new FileStream(path + ".~p", FileMode.Create))
      {
        ConsoleProgressBar consoleProgressBar = new ConsoleProgressBar(hFile.Header.FileCount);
        try
        {
          consoleProgressBar.Start();
          for (int index = 0; index < hFile.Files.Length; ++index)
          {
            consoleProgressBar.PerformStep();
            using (MemoryStream memoryStream = new MemoryStream())
            {
              string path1 = unpackPath + "\\" + hFile.Files[index].GetFilePath();
              if (File.Exists(path1 + ".header"))
              {
                using (FileStream fileStream2 = new FileStream(path1 + ".header", FileMode.Open))
                  this.CopyStream((Stream) fileStream2, (Stream) memoryStream);
              }
              using (FileStream fileStream2 = new FileStream(path1, FileMode.Open))
              {
                hFile.Files[index].ContentSize = (int) fileStream2.Length;
                if (hFile.Files[index].FileType.HasSizeHeader)
                {
                  memoryStream.Write(BitConverter.GetBytes(hFile.Files[index].ContentSize), 0, 4);
                  hFile.Files[index].ContentSize += 4;
                }
                this.CopyStream((Stream) fileStream2, (Stream) memoryStream);
              }
              memoryStream.Seek(0L, SeekOrigin.Begin);
              hFile.Files[index].Offset = fileStream1.Position;
              if (hFile.Files[index].IsCompressed())
              {
                int num = this.compress((Stream) memoryStream, (Stream) fileStream1, 9);
                if (num < 0)
                  throw new Exception();
                hFile.Files[index].Size = num;
              }
              else
              {
                hFile.Files[index].Size = (int) memoryStream.Length;
                this.CopyStream((Stream) memoryStream, (Stream) fileStream1);
              }
              if ((int) hFile.Header.Version[1] == 1)
              {
                if (fileStream1.Length % 4L != 0L)
                  fileStream1.Write(new byte[4L - fileStream1.Length % 4L], 0, (int) (4L - fileStream1.Length % 4L));
              }
              else if (fileStream1.Length % 2048L != 0L)
              {
                int count = (int) (2048L * (fileStream1.Length / 2048L + 1L) - fileStream1.Length);
                fileStream1.Write(new byte[count], 0, count);
              }
            }
          }
          hFile.Header.DataFooterOffset = (ulong) fileStream1.Position;
          int num1 = (int) ushort.MaxValue - (int) (fileStream1.Length & (long) ushort.MaxValue);
          for (int index = 0; index <= num1; ++index)
            fileStream1.WriteByte((byte) 122);
        }
        finally
        {
          consoleProgressBar.End();
        }
      }
    }

    private int compress(Stream sin, Stream sout, int level)
    {
      ZStream zstream = new ZStream();
      int length = (int) sin.Length;
      byte[] buffer1 = new byte[length];
      byte[] buffer2 = new byte[32768];
      int num1 = 0;
      int num2 = zstream.deflateInit(level);
      if (num2 != 0)
        return num2;
      zstream.avail_in = 0;
      zstream.avail_out = 32768;
      zstream.next_out = buffer2;
      int flush = 0;
      int num3;
      do
      {
        if (zstream.avail_in == 0)
        {
          int num4 = sin.Read(buffer1, 0, length);
          zstream.next_in = buffer1;
          zstream.avail_in = num4;
          zstream.next_in_index = 0;
          if (num4 < length)
            flush = 4;
        }
        num3 = zstream.deflate(flush);
        if (num3 < 0)
          return num3;
        if (zstream.avail_out == 0 || num3 == 1)
        {
          int count = 32768 - zstream.avail_out;
          sout.Write(buffer2, 0, count);
          num1 += count;
          zstream.next_out = buffer2;
          zstream.avail_out = 32768;
          zstream.next_out_index = 0;
        }
      }
      while (num3 != 1);
      int num5 = zstream.deflateEnd();
      if (num5 != 0)
        return num5;
      zstream.free();
      return num1;
    }

    private string getPath(string basepath, string path)
    {
      if (basepath == null || basepath.Length == 0)
        return path;
      return basepath + "\\" + path;
    }

    private void CopyStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[32768];
      int count;
      while ((count = input.Read(buffer, 0, 32768)) > 0)
        output.Write(buffer, 0, count);
      output.Flush();
    }
  }
}
