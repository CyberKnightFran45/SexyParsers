using System;
using System.Text.Json;

/// <summary> Helper used for writing json strings. </summary>

public static class JsonHelper
{
/** <summary> Writes a String to JSON as a PropertyName or a Value. </summary>

<param name = "writer"> The Json writer. </param>
<param name = "str"> The String to Write. </param>
<param name = "isPropertyName"> Wether to write a PropertyName or not. </param> */

public static void WriteString(Utf8JsonWriter writer, ReadOnlySpan<char> str, bool isPropertyName)
{

if(isPropertyName)
writer.WritePropertyName(str);

else
writer.WriteStringValue(str);

}

}