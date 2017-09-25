using System.IO;

namespace DoubleFine
{
  public class StackingFileIndex : FileIndex
  {
    public StackingFileIndex()
    {
      this.Uncompressed = (byte) 4;
      this.Compressed = (byte) 8;
    }

    public StackingFileIndex(BinaryReader r)
      : this()
    {
      byte[] data = r.ReadBytes(16);
      if (this.debug)
        this.debugBitData(data);
      this.ContentSize = this.toInt32(data, 0, 0, 24);
      this.FileNameOffset = this.toInt32(data, 3, 0, 21);
      this.UnknownFlag1 = this.toInt32(data, 5, 5, 19);
      this.Offset = (long) this.toInt32(data, 8, 0, 29);
      this.Size = this.toInt32(data, 11, 5, 23);
      this.FileTypeIndex = (byte) this.toInt32(data, 14, 4, 8);
      this.CompressFlag = (byte) ((uint) data[15] & 15U);
    }

    public override void Write(BinaryWriter w)
    {
      byte[] numArray = new byte[16];
      this.mapToData(numArray, this.ContentSize, 0, 0, 24);
      this.mapToData(numArray, this.FileNameOffset, 3, 0, 21);
      this.mapToData(numArray, this.UnknownFlag1, 5, 5, 19);
      this.mapToData(numArray, this.Offset, 8, 0, 29);
      this.mapToData(numArray, this.Size, 11, 5, 23);
      this.mapToData(numArray, (short) this.FileTypeIndex, 14, 4, 8);
      this.mapToData(numArray, new byte[1]
      {
        this.CompressFlag
      }, 15, 4, 4);
      w.Write(numArray);
    }
  }
}
