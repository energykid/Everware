using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everware.Utils;

public static class StreamUtils
{
    public static void WriteVector3(this BinaryWriter bb, Vector3 v)
    {
        bb.Write(v.X);
        bb.Write(v.Y);
        bb.Write(v.Z);
    }

    public static Vector3 ReadVector3(this BinaryReader bb)
    {
        return new Vector3(bb.ReadSingle(), bb.ReadSingle(), bb.ReadSingle());
    }
}
