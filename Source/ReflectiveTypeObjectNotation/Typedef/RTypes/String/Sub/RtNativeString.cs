using System.Text.Json;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Represents a NativeString in the RtSystem. </summary>

internal static class RtNativeString
{
/// <summary> Raw Encoding (used by RTON reader) </summary>

private const EncodingType RAW_ENCODING = EncodingType.UTF8;

/// <summary> String Encoding (used by RTON writer) </summary>

private const EncodingType STR_ENCODING = EncodingType.US_ASCII;

/// <summary> The Cache type </summary>

private const StringCacheType CACHE_TYPE = StringCacheType.Native;

/** <summary> Reads a NativeString from RTON and Writes it to JSON. </summary>

<param name = "reader"> The RTON Reader. </param>
<param name = "writer"> The JSON Writer. </param>
<param name = "isPropertyName"> Wheter to write string as a PropertyName or not. </param> */

internal static void Read(NativeBuffer buffer, ref ulong pos, Utf8JsonWriter writer, bool isPropertyName)
{
string str = RtStringCache.Read(buffer, RAW_ENCODING, ref pos);

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
string str = RtStringCache.Read(buffer, RAW_ENCODING, ref pos);
strCache.Add(str, CACHE_TYPE);

return str;
}

// Decode Native string (Core logic)

private static string DecodeCore(NativeBuffer buffer,
                                 ReferenceStrings strCache,
								 bool isIndexed,
                                 ref ulong pos)
{

if(isIndexed)
return DecodeIndexed(buffer, strCache, ref pos);

return DecodeRaw(buffer, strCache, ref pos);
}									 

/** <summary> Reads a CachedString from RTON and Write it to JSON. </summary>

<param name = "buffer"> The RTON buffer. </param>
<param name = "writer"> The JSON writer. </param>
<param name = "isIndexed"> Determines if the NativeString is in the Reference List or not. </param>
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
buffer.SetUInt8(pos, RTypeId.NATIVE_STRING_INDEX);
pos++;

var strIndex = (uint)strCache.IndexOf(str, CACHE_TYPE);

pos += (ulong)buffer.SetVarInt(pos, strIndex);
}

// Encode raw value and add it to cache

private static void EncodeRaw(string str, NativeBuffer buffer, ReferenceStrings strCache, ref ulong pos)
{
buffer.SetUInt8(pos, RTypeId.NATIVE_STRING_CACHE);
pos++;

pos += buffer.SetStringByVarLen(pos, str, STR_ENCODING);

strCache.Add(str, CACHE_TYPE);
}

/** <summary> Writes a CachedString to RTON, by indexing or adding it. </summary>

<param name = "writer"> The Stream where the RTON Data will be Written. </param>
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