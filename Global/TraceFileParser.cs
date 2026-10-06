using System;
using System.IO;

/// <summary> Logger for file parser </summary>

public static class TraceFileParser
{
// Encode execution

private static void ExecuteFileOp(TraceContext ctx, string inputPath, ref string outputPath,
                                  string extension, string message,
                                  Action<Stream, Stream> action)
{
PathHelper.ChangeExtension(ref outputPath, extension);

TraceFileSteps.Run(ctx,
                   inputPath,
				   outputPath,
				   message,
                   (i, o, _) => action(i, o) 
);

}

// Log encode process

public static void Encode(string operationName, string inputPath, string outputPath, string extension,
                          Action<Stream, Stream> action,
                          params (string Name, object Value)[] args)
{

TraceExecutor.Run(operationName, ctx =>
{

ExecuteFileOp(ctx,
              inputPath,
			  ref outputPath,
              extension,
              "Encoding data...",
              action);
},

args
);

}

// 

public static void Decode(string operationName, string inputPath, string outputPath, string extension,
                          Action<Stream, Stream> action,
                          params (string Name, object Value)[] args)
{

TraceExecutor.Run(operationName, ctx =>
{

ExecuteFileOp(ctx,
              inputPath,
			  ref outputPath,
              extension,
              "Decoding data...",
              action);
},

args
);	

}

}