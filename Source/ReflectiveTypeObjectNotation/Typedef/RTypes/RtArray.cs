using System.IO;
using System.Text.Json;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Represents an Array in the RtSystem. </summary>

internal static class RtArray
{
// Minimum Array size

private const ulong MIN_ARRAY_SIZE = SizeT.ONE_KILOBYTE * 64;

/** <summary> Reads a RTON Array and writes it to its JSON equivalent </summary>

<param name = "buffer"> The RTON Reader </param>
<param name = "writer"> The JSON Writer. </param> */

internal static void Read(NativeBuffer buffer, ReferenceStrings strCache,
                          ref ulong pos, Utf8JsonWriter writer)
{
byte mArrayStart = buffer.GetUInt8(pos);
pos++;

if(mArrayStart != RTypeId.ARRAY_START)
{
var msg = $"Unknown Array start: 0x{mArrayStart:X2} @ {pos} (Expected: 0x{RTypeId.ARRAY_START:X2})";

throw new InvalidDataException(msg);
}

writer.WriteStartArray();

uint elementsCount = buffer.GetVarInt(pos, out int varLen);
pos += (ulong)varLen;

for(int i = 0; i < elementsCount; i++)
{
byte elementType = buffer.GetUInt8(pos);
pos++;

RtObject.DecodeRToken(buffer, strCache, ref pos, writer, elementType);
}

byte mArrayEnd = buffer.GetUInt8(pos);
pos++;

if(mArrayEnd != RTypeId.ARRAY_END)
{
var msg = $"Invalid Array end: 0x{mArrayEnd:X2} @ {pos} (Expected: 0x{RTypeId.ARRAY_END:X2})";

throw new InvalidDataException(msg);
}

writer.WriteEndArray();
}

// Encode json array

private static uint EncodeJArray(NativeJsonReader reader, NativeBuffer buffer,
                                 ReferenceStrings strCache, ref ulong pos)
{
uint count = 0;

while(reader.ReadToken() )
{
ulong arraySize = buffer.Size;

if(arraySize - pos < MIN_ARRAY_SIZE)
buffer.Realloc(arraySize + MIN_ARRAY_SIZE); // Resize array buffer if needed

switch(reader.CurrentTokenType)
{
case JsonTokenType.EndArray:
return count;

default:
RtObject.EncodeJToken(reader, buffer, strCache, ref pos);

count++;
break;
}

}

throw new JsonException("Unexpected end of JSON before EndArray.");
}

/** <summary> Reads a JSON Array and Writes its equivalent as RTON </summary>

<param name = "reader"> The JSON reader. </param>
<param name = "writer"> The RTON writer. </param> */

internal static void Write(NativeJsonReader reader, NativeBuffer buffer,
                           ReferenceStrings strCache, ref ulong pos)
{
buffer.SetUInt8(pos, RTypeId.ARRAY);
pos++;

buffer.SetUInt8(pos, RTypeId.ARRAY_START);
pos++;

using NativeBuffer rawArray = new(MIN_ARRAY_SIZE);
ulong arrayPos = 0;

uint elementsCount = EncodeJArray(reader, rawArray, strCache, ref arrayPos);
pos += (ulong)buffer.SetVarInt(pos, elementsCount);

buffer.CopyFrom(rawArray, 0, pos, arrayPos);
pos += arrayPos;

buffer.SetUInt8(pos, RTypeId.ARRAY_END);
pos++;
}

}

}