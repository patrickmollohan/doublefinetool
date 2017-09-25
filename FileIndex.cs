using System;
using System.Collections;
using System.IO;

namespace DoubleFine
{
  public abstract class FileIndex
  {
    protected byte Uncompressed;
    protected byte Compressed;
    protected bool debug;
    public int ContentSize;
    public int FileNameOffset;
    public int UnknownFlag1;
    public byte UnknownFlag2;
    public long Offset;
    public int Size;
    public byte FileTypeIndex;
    public DoubleFine.FileTypeIndex FileType;
    public byte CompressFlag;
    public string FilePathString;

    public FileIndex()
    {
    }

    public FileIndex(BinaryReader r)
    {
    }

    public abstract void Write(BinaryWriter w);

    public virtual bool IsCompressed()
    {
      return ((int) this.CompressFlag & (int) this.Compressed) > 0;
    }

    public string GetFilePath()
    {
      if (Path.GetExtension(this.FilePathString).Length > 0 || this.FileType.Name == null || this.FileType.Name == "")
        return this.FilePathString;
      if (this.FileType.Extension == null || this.FileType.Extension.Length == 0)
        return this.FilePathString + "." + this.FileType.Name;
      return this.FilePathString + "." + this.FileType.Extension;
    }

    protected int toInt32(byte[] data, int byteoffset, int bitoffset, int length)
    {
      bool[] bits = this.getBits(data, byteoffset, bitoffset, length);
      int num = 0;
      foreach (bool flag in bits)
      {
        num <<= 1;
        if (flag)
          ++num;
      }
      return num;
    }

    protected short toInt16(byte[] data, int byteoffset, int bitoffset, int length)
    {
      bool[] bits = this.getBits(data, byteoffset, bitoffset, length);
      short num = 0;
      foreach (bool flag in bits)
      {
        num <<= 1;
        if (flag)
          ++num;
      }
      return num;
    }

    protected byte toByte(byte[] data, int byteoffset, int bitoffset, int length)
    {
      bool[] bits = this.getBits(data, byteoffset, bitoffset, length);
      short num = 0;
      foreach (bool flag in bits)
      {
        num <<= 1;
        if (flag)
          ++num;
      }
      return (byte) num;
    }

    protected bool[] getBits(byte[] data, int byteoffset, int bitoffset, int length)
    {
      int num = byteoffset * 8 + bitoffset;
      if (num < 0)
        throw new Exception();
      if (num >= data.Length * 8)
        throw new Exception();
      if (bitoffset < 0 || bitoffset > 7)
        throw new Exception();
      BitArray bitArray = new BitArray(data);
      bool[] flagArray = new bool[length];
      for (int index = 0; index < length; ++index)
        flagArray[index] = bitArray.Get((num + index) / 8 * 8 + (7 - (num + index) % 8));
      return flagArray;
    }

    protected void mapToData(byte[] buf, long val, int byteoffset, int bitoffset, int length)
    {
      byte[] bytes = BitConverter.GetBytes(val);
      Array.Reverse((Array) bytes);
      this.mapToData(buf, bytes, byteoffset, bitoffset, length);
    }

    protected void mapToData(byte[] buf, int val, int byteoffset, int bitoffset, int length)
    {
      byte[] bytes = BitConverter.GetBytes(val);
      Array.Reverse((Array) bytes);
      this.mapToData(buf, bytes, byteoffset, bitoffset, length);
    }

    protected void mapToData(byte[] buf, short val, int byteoffset, int bitoffset, int length)
    {
      byte[] bytes = BitConverter.GetBytes(val);
      Array.Reverse((Array) bytes);
      this.mapToData(buf, bytes, byteoffset, bitoffset, length);
    }

    protected void mapToData(byte[] buf, byte[] val, int byteoffset, int bitoffset, int length)
    {
      BitArray bitArray = new BitArray(val);
      if (bitArray.Length - length < 0 || length < 0)
        throw new Exception();
      if (byteoffset * 8 + bitoffset < 0 || byteoffset * 8 + bitoffset + length > buf.Length * 8)
        throw new Exception();
      int num1 = byteoffset * 8 + bitoffset;
      int num2 = val.Length * 8 - length;
      for (int index1 = 0; index1 < length; ++index1)
      {
        if (bitArray.Get((num2 + index1) / 8 * 8 + (7 - (num2 + index1) % 8)))
        {
          int index2 = (num1 + index1) / 8;
          switch ((num1 + index1) % 8)
          {
            case 0:
              buf[index2] = (byte) ((uint) buf[index2] + 128U);
              continue;
            case 1:
              buf[index2] = (byte) ((uint) buf[index2] + 64U);
              continue;
            case 2:
              buf[index2] = (byte) ((uint) buf[index2] + 32U);
              continue;
            case 3:
              buf[index2] = (byte) ((uint) buf[index2] + 16U);
              continue;
            case 4:
              buf[index2] = (byte) ((uint) buf[index2] + 8U);
              continue;
            case 5:
              buf[index2] = (byte) ((uint) buf[index2] + 4U);
              continue;
            case 6:
              buf[index2] = (byte) ((uint) buf[index2] + 2U);
              continue;
            case 7:
              buf[index2] = (byte) ((uint) buf[index2] + 1U);
              continue;
            default:
              continue;
          }
        }
      }
    }

    protected void debugBitData(byte[] data)
    {
      using (StreamWriter streamWriter = new StreamWriter("debug.log", true))
      {
        foreach (bool bit in this.getBits(data, 0, 0, 128))
          streamWriter.Write(bit ? "1" : "0");
        streamWriter.WriteLine();
      }
    }
  }
}
