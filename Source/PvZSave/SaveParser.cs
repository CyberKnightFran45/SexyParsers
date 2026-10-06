using System.IO;

namespace SexyParsers.PvZSave
{
/// <summary> Converts PvZ Savefiles from Binary to JSON and viceversa </summary>

public static class SaveParser
{
// Save serializer

private static readonly PvZSaveSerializer saveSerializer = new();

// Encode json to stream

public static void EncodeStream(Stream input, Stream output)
{
var save = JsonSerializer.DeserializeObject<PvZUserdata>(input, PvZUserdata.Context);

saveSerializer.WriteBin(output, save);
}

/** <summary> Encodes a Json Save as a Binary. </summary>

<param name = "inputPath"> Save Path (must be a JSON File). </param>
<param name = "outputPath"> Location to Encoded File. </param> */

public static void EncodeFile(string inputPath, string outputPath)
{

TraceFileParser.Encode("PvZUserdata Encoding",
                       inputPath,
                       outputPath,
                       ".dat",
                       EncodeStream,
                       ("InputPath", inputPath),
                       ("OutputPath", outputPath)
);

}
	
// Get Save from BinaryStream

public static void DecodeStream(Stream input, Stream output) 
{
var userdata = saveSerializer.ReadBin(input);

JsonSerializer.SerializeObject(userdata, output, PvZUserdata.Context);
}

/** <summary> Decodes a Raw save as a PvZUserdata Instance. </summary>

<param name = "inputPath"> Path to Raw Save. </param>
<param name = "outputPath"> Location to Decoded Save </param> */

public static void DecodeFile(string inputPath, string outputPath)
{

TraceFileParser.Decode("PvZUserdata Decoding",
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