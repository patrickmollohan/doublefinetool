using System.IO;
using System.Text;

namespace DoubleFine
{
  public struct FileTypeIndex
  {
    public string Name;
    public string Extension;
    public bool HasSizeHeader;
    public int Unknown1;
    public int Unknown2;
    public int Unknown3;

    public FileTypeIndex(BinaryReader r)
    {
      this.Name = new string(Encoding.ASCII.GetChars(r.ReadBytes(r.ReadInt32()))).TrimEnd(new char[1]);
      this.Extension = "";
      this.HasSizeHeader = false;
      this.Unknown1 = r.ReadInt32();
      this.Unknown2 = r.ReadInt32();
      this.Unknown3 = r.ReadInt32();
    }

    public void Write(BinaryWriter w)
    {
      w.Write(Encoding.ASCII.GetByteCount(this.Name + (object) char.MinValue));
      w.Write(Encoding.ASCII.GetBytes(this.Name + (object) char.MinValue));
      w.Write(this.Unknown1);
      w.Write(this.Unknown2);
      w.Write(this.Unknown3);
    }
  }
}
