using System.IO;
using PopCapResManager;

namespace SexyParsers.Newton
{
/// <summary> Initializes Parsing Tasks for NEWTON Files </summary>

public static class NewtonParser
{
// SubGroup serializer

private static readonly ResGroupSerializer resSerializer = new();

// Encode Newton Stream

public static void EncodeStream(Stream input, Stream output)
{
var resGroup = JsonSerializer.DeserializeObject<MResGroup>(input, MResGroup.Context);

resSerializer.WriteBin(output, resGroup);
}

/** <summary> Converts a ResGroup to NEWTON. </summary>

<param name = "inputPath"> The Path where the ResGroup to be Encoded is Located. </param>
<param name = "outputPath"> The Location where the Encoded NEWTON File will be Saved. </param> */

public static void Encode(string inputPath, string outputPath)
{

TraceFileParser.Encode("NewRes Encoding",
                       inputPath,
                       outputPath,
                       ".newton",
                       EncodeStream,
                       ("InputPath", inputPath),
                       ("OutputPath", outputPath)
);

}

// Decode Newton Stream

public static void DecodeStream(Stream input, Stream output)
{
var resGroup = resSerializer.ReadBin(input);

JsonSerializer.SerializeObject(resGroup, output, MResGroup.Context);
}

/** <summary> Converts a NEWTON Stream to a Json ResGroup </summary>

<param name = "inputPath"> The Path where the NEWTON File to Encode is Located. </param>
<param name = "outputPath"> The Location where the ResGroup File will be Saved. </param> */

public static void Decode(string inputPath, string outputPath)
{

TraceFileParser.Decode("NewRes Decoding",
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