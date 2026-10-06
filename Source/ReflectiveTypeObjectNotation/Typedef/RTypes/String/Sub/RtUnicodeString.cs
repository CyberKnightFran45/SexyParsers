using System.Text.Json;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Represents a Unicode String in the RtSystem. </summary>

internal static class RtUnicodeString
{
/// <summary> The Encoding used </summary>

private const EncodingType ENCODING = EncodingType.UTF8;

/// <summary> The Cache type </summary>

private const StringCacheType CACHE_TYPE = StringCacheType.Unicode;

/** <summary> Reads a Unicode String from RTON and writes it to JSON. </summary>

<param name = "buffer"> RTON buffer </param>
<param name = "writer"> JSON writer </param>
<param name = "isPropertyName"> Wheter to Write String. </param> */

internal static void Read(NativeBuffer buffer, ref ulong pos, Utf8JsonWriter writer, bool isPropertyName)
{
string str = RtStringCacheL2.Read(buffer, ENCODING, ref pos);

JsonHelper.WriteString(writer, str, isPropertyName);
}

// Decode index string

private static string DecodeIndexed(NativeBuffer buffer, ReferenceStrings strCache, ref ulong pos)
{
var strIndex = (int)buffer.GetVarInt(pos, out int varLen);
pos += (ulong)varLen;

return strCache.Get(strIndex, CACHE_TYPE);
}

// Decode raw value and add it to cache

private static string DecodeRaw(NativeBuffer buffer, ReferenceStrings strCache, ref ulong pos)
{
string str = RtStringCacheL2.Read(buffer, ENCODING, ref pos);
strCache.Add(str, CACHE_TYPE);

return str;
}

// Decode Unicode string (Core logic)

private static string DecodeCore(NativeBuffer buffer,
                                 ReferenceStrings strCache,
								 bool isIndexed,
                                 ref ulong pos)
{

if(isIndexed)
return DecodeIndexed(buffer, strCache, ref pos);

return DecodeRaw(buffer, strCache, ref pos);
}

/** <summary> Reads a CachedString from a RTON and Write it to JSON. </summary>

<param name = "buffer"> The RTON buffer. </param>
<param name = "writer"> The JSON writer. </param>
<param name = "isIndexed"> Determines if the UnicodeString is in the Reference List or not. </param>
<param name = "isPropertyName"> Wheter to write string as a PropertyName or not. </param> */

internal static void ReadCached(NativeBuffer buffer,
                                ref ulong pos,
                                Utf8JsonWriter writer,
                                ReferenceStrings strCache,
								bool isIndexed,
                                bool isPropertyName)
{
var str = DecodeCore(buffer, strCache, isIndexed, ref pos);

JsonHelper.WriteString(writer, str, isPropertyName);
}

// Encode index string

private static void EncodeIndexed(string str,
                                  NativeBuffer buffer,
								  ReferenceStrings strCache,
                                  ref ulong pos)
{
buffer.SetUInt8(pos, RTypeId.UNICODE_STRING_INDEX);
pos++;

var strIndex = (uint)strCache.IndexOf(str, CACHE_TYPE);

pos += (ulong)buffer.SetVarInt(pos, strIndex);
}

// Encode raw value and add it to cache

private static void EncodeRaw(string str, NativeBuffer buffer, ReferenceStrings strCache, ref ulong pos)
{
buffer.SetUInt8(pos, RTypeId.UNICODE_STRING_CACHE);
pos++;

RtStringCacheL2.Write(str, buffer, ENCODING, ref pos);

strCache.Add(str, CACHE_TYPE);
}

/** <summary> Writes a CachedString to RTON, by indexing or adding it. </summary>

<param name = "writer"> The RTON writer. </param>
<param name = "str"> The String to be Written. </param> */

internal static void Write(string str, NativeBuffer buffer, ReferenceStrings strCache, ref ulong pos)
{

if(strCache.Contains(str, CACHE_TYPE) )
EncodeIndexed(str, buffer, strCache, ref pos);

else
EncodeRaw(str, buffer, strCache, ref pos);

}

}

}