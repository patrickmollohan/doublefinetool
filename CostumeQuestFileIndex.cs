using System.IO;

namespace DoubleFine
{
  public class CostumeQuestFileIndex : FileIndex
  {
    public CostumeQuestFileIndex()
    {
      this.Uncompressed = (byte) 2;
      this.Compressed = (byte) 4;
    }

    public CostumeQuestFileIndex(BinaryReader r)
      : this()
    {
      byte[] data = r.ReadBytes(16);
      if (this.debug)
        this.debugBitData(data);
      this.ContentSize = this.toInt32(data, 0, 0, 23);
      this.UnknownFlag1 = this.toInt32(data, 2, 7, 18);
      this.Size = this.toInt32(data, 5, 1, 22);
      this.Offset = (long) this.toInt32(data, 7, 7, 30);
      this.UnknownFlag2 = this.toByte(data, 11, 5, 3);
      this.FileNameOffset = this.toInt32(data, 12, 0, 21);
      this.FileTypeIndex = (byte) this.toInt32(data, 14, 5, 7);
      this.CompressFlag = (byte) ((uint) data[15] & 15U);
    }

    public override void Write(BinaryWriter w)
    {
      byte[] numArray = new byte[16];
      this.mapToData(numArray, this.ContentSize, 0, 0, 23);
      this.mapToData(numArray, this.UnknownFlag1, 2, 7, 18);
      this.mapToData(numArray, this.Size, 5, 1, 22);
      this.mapToData(numArray, this.Offset, 7, 7, 30);
      this.mapToData(numArray, (short) this.UnknownFlag2, 11, 5, 3);
      this.mapToData(numArray, this.FileNameOffset, 12, 0, 21);
      this.mapToData(numArray, (short) this.FileTypeIndex, 14, 5, 7);
      this.mapToData(numArray, new byte[1]
      {
        this.CompressFlag
      }, 15, 4, 4);
      w.Write(numArray);
    }
  }
}
