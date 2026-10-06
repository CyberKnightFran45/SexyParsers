using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using SexyCryptor;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Supports parsing RTON files from PvZ 2 and other games </summary>

public static class RtonParser
{
/// <summary> Rton header </summary>

private const uint HEADER = 0x52544F4E;

/// <summary> Expected version </summary>

private const uint VERSION = 1;

/// <summary> Rton footer </summary>

private const uint FOOTER = 0x444F4E45;

// Json writer options

private static readonly JsonWriterOptions JsonWriterCfg = new()
{
Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
Indented = true,
SkipValidation = false
};

// Encode core

private static NativeBuffer EncodeCore(NativeJsonReader reader)
{
ReferenceStrings strCache = new();

NativeBuffer buffer = new(SizeT.ONE_MEGABYTE * 64);
ulong pos = 0;

buffer.SetUInt32(pos, HEADER, Endianness.BigEndian);
pos += 4;

buffer.SetUInt32(pos, VERSION);
pos += 4;

RtObject.Write(reader, buffer, strCache, ref pos);

buffer.SetUInt32(pos, FOOTER, Endianness.BigEndian);
pos += 4;

var rawSize = (int)pos;
var rawData = buffer.GetView(0, rawSize);

RtonCache.AddStrings(rawData, strCache);

buffer.Realloc(rawSize);

return buffer;
}

// Encode buffer

public static NativeBuffer Encode(NativeBuffer src, bool useEncryption = false)
{
using NativeJsonReader jsonReader = new(src);
var rtonBuffer = EncodeCore(jsonReader);

if(!useEncryption)
return rtonBuffer;

var crypto = CRton.Encrypt(rtonBuffer);
rtonBuffer.Dispose();

return crypto;
}

// Encode stream

public static void EncodeStream(Stream input, Stream output, bool useEncryption = false)
{
using var iOwner = input.ReadPtr();
using var rOwner = Encode(iOwner, useEncryption);

var rtonData = rOwner.GetView();

output.Write(rtonData);
}

/** <summary> Converts a JSON File to RTON. </summary>

<param name = "inputPath"> The Path where the JSON File to be Encoded is Located. </param>
<param name = "outputPath"> The Location where the Encoded RTON File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, bool useEncryption)
{

TraceFileParser.Encode("RTON Encoding",
                       inputPath,
                       outputPath,
                       ".rton",
                       (i, o) => EncodeStream(i, o, useEncryption),
                       ("InputPath", inputPath),
                       ("OutputPath", outputPath),
					   ("UseEncryption", useEncryption)
);

}

// Decode core

private static void DecodeCore(NativeBuffer buffer, Utf8JsonWriter writer)
{
var strCache = RtonCache.GetStrings(buffer.GetView() );

ulong pos = 0;

uint inputMagic = buffer.GetUInt32(pos, Endianness.BigEndian);
pos += 4;

if(inputMagic != HEADER)
throw new Exception($"Invalid Rton Identifier: 0x{inputMagic:X8}, expected: 0x{HEADER:X8}");

uint inputVer = buffer.GetUInt32(pos);
pos += 4;

if(inputVer != VERSION)
TraceLogger.WriteWarn($"Unknown RTON version: v{inputVer}, expected: v{VERSION}");

RtObject.Read(buffer, strCache, ref pos, writer);

uint inFooter = buffer.GetUInt32(pos, Endianness.BigEndian);
pos += 4;

if(inFooter != FOOTER)
TraceLogger.WriteWarn($"Invalid footer: 0x{inFooter:X8}, expected: 0x{FOOTER:X8}");

}

// Decode raw

private static void DecodeRaw(NativeBuffer src, Stream output)
{
using Utf8JsonWriter jsonWriter = new(output, JsonWriterCfg);

DecodeCore(src, jsonWriter);
}

// Decode buffer

public static void Decode(NativeBuffer src, Stream output, out bool useEncryption)
{
ushort encryptionFlags = src.GetUInt16(0);

useEncryption = encryptionFlags == 0x10;

if(useEncryption)
{
using var rawBuffer = CRton.Decrypt(src);
DecodeRaw(rawBuffer, output);

return;
}

DecodeRaw(src, output);
}

// Write JSON

public static void DecodeStream(Stream input, Stream output)
{
using var iOwner = input.ReadPtr();

Decode(iOwner, output, out _);
}

/** <summary> Converts a RTON File to JSON. </summary>

<param name = "inputPath"> The Path where the RTON File to be Decoded is Located. </param>
<param name = "outputPath"> The Location where the Decoded JSON File will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{

TraceFileParser.Decode("RTON Decoding",
                       inputPath,
                       outputPath,
                       ".json",
                       DecodeStream,
                       ("InputPath", inputPath),
                       ("OutputPath", outputPath)
);

}

}

}