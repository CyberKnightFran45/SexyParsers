using System;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Represents a String Cache in the RtSystem. </summary>

internal static class RtStringCache
{
// Read cache string

internal static string Read(NativeBuffer buffer, EncodingType encoding, ref ulong pos)
{
var rawLen = (int)buffer.GetVarInt(pos, out int varLen);
pos += (ulong)varLen;

using var charPtr = buffer.GetString(pos, rawLen, encoding);
pos += (ulong)rawLen;

return charPtr.ToString();
}

// Write cache string

internal static void Write(ReadOnlySpan<char> str,
                           NativeBuffer buffer,
                           EncodingType encoding,
                           ref ulong pos)
{
ulong rawLen = buffer.SetStringByVarLen(pos, str, encoding);

pos += rawLen;
}

}

}