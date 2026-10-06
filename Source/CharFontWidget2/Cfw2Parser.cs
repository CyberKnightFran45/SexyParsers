using System.IO;

namespace SexyParsers.CharFontWidget2
{
/// <summary> Initializes Parsing Tasks for CFW2 Files. </summary>

public static class Cfw2Parser
{
// Widget serializer

private static readonly FontWidgetSerializer widgetSerializer = new();

/// <summary> Expected version number (Major and Minor) </summary>

private const long VERSION = 0;

// Encode CFW2 Stream

public static void EncodeStream(Stream input, Stream output)
{
var widget = JsonSerializer.DeserializeObject<FontWidget>(input, FontWidget.Context);

output.WriteInt64(VERSION);
output.WriteInt64(VERSION);

widgetSerializer.WriteBin(output, widget);
}

/** <summary> Encodes a Json FontWidget as a CFW2 File. </summary>

<param name = "inputPath"> The Path to the FontWidget (must be a JSON File). </param>
<param name = "outputPath"> The Location where the Encoded File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath)
{
	
TraceFileParser.Encode("Cfw2 Encoding",
                       inputPath,
                       outputPath,
                       ".cfw2",
                       EncodeStream,
                       ("InputPath", inputPath),
                       ("OutputPath", outputPath)
);

}
	
// Get FontWidget from BinaryStream

public static void DecodeStream(Stream input, Stream output) 
{
long mMajVer = input.ReadInt64();
long mMinVer = input.ReadInt64();

if(mMajVer != VERSION || mMinVer != VERSION)
TraceLogger.WriteWarn($"Unknown version: v{mMajVer}.{mMinVer} - Expected: v{VERSION}.{VERSION}");

var widget = widgetSerializer.ReadBin(input);

JsonSerializer.SerializeObject(widget, output, FontWidget.Context);
}

/** <summary> Decodes a CFW2 File as a FontWidget Instance. </summary>

<param name = "inputPath"> The Path to the CFW2 File. </param>
<param name = "outputPath"> The Location where the Decoded File will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{

TraceFileParser.Decode("Cfw2 Decoding",
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