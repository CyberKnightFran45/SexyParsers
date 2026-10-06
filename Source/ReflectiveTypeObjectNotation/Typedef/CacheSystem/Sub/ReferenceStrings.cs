using System.Collections.Generic;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Provides indexed reference storage for cached strings </summary>

public sealed class ReferenceStrings
{
// Native strings

private readonly Dictionary<string, int> _nativeLookup;

private readonly List<string> _nativeStrings;

// Unicode strings

private readonly Dictionary<string, int> _unicodeLookup;

private readonly List<string> _unicodeStrings;

// Cache sizes

private long _nativeStringsLength;

private long _unicodeStringsLength;

/// <summary> Gets the number of cached Native strings. </summary>

public int NativeStringsCount => _nativeStrings.Count;

/// <summary> Gets the number of cached Unicode strings. </summary>

public int UnicodeStringsCount => _unicodeStrings.Count;

/// <summary> Gets the total length of cached Native strings. </summary>

public long NativeCacheSize => _nativeStringsLength;

/// <summary> Gets the total length of cached Unicode strings. </summary>

public long UnicodeCacheSize => _unicodeStringsLength;

// ctor

public ReferenceStrings()
{
_nativeLookup = new();
_nativeStrings = new();

_unicodeLookup = new();
_unicodeStrings = new();
}

// Get lookup table

private Dictionary<string, int> GetLookup(StringCacheType cacheType)
{

return cacheType switch
{
StringCacheType.Native =>  _nativeLookup,
StringCacheType.Unicode => _unicodeLookup,
_ => null
};

}

// Get cached strings

private List<string> GetCache(StringCacheType cacheType)
{

return cacheType switch
{
StringCacheType.Native =>  _nativeStrings,
StringCacheType.Unicode => _unicodeStrings,
_ => null
};

}

// Increment cache size

private void SumLength(int strLen, StringCacheType cacheType)
{

switch(cacheType)
{
case StringCacheType.Native:
_nativeStringsLength += strLen;
break;

case StringCacheType.Unicode:
_unicodeStringsLength += strLen;
break;

default:
break; // no-op
}

}

/** <summary> Adds a string to Cache, while preserving its positional index. </summary> 

<remarks>1- RTON cache is positional: every <b>0x90</b> and <b>0x92</b> type adds an entry, 
            including duplicates. <para>

</para>2- Lookup only tracks unique strings and preserves their first index. </remarks>

<param name = "str"> The String. </param> **/

public void Add(string str, StringCacheType cacheType)
{
var lookup = GetLookup(cacheType);
var cache = GetCache(cacheType);

int strIndex = cache.Count;

cache.Add(str);

if(lookup.TryAdd(str, strIndex) )
SumLength(str.Length, cacheType);

}

// Warn on invalid cache index

private static void WarnInvalidCacheIndex(int index, int cacheCount, StringCacheType cacheType)
{
string msg;

if(cacheCount == 0)
msg = $"{cacheType} cache is empty.";

else
msg = $"Invalid string index: '{index}' @ {cacheType} cache. Expected range: [0 .. {cacheCount - 1}]";

TraceLogger.WriteWarn(msg);
}

/** <summary> Gets a string from the List. </summary>

<param name = "index"> The String index. </param>

<returns> The String Obtained or <c>null</c> if index is invalid. </returns> **/

public string Get(int index, StringCacheType cacheType)
{
var cache = GetCache(cacheType);

int cacheCount = cache.Count;

if(index < 0 || index >= cacheCount)
{
WarnInvalidCacheIndex(index, cacheCount, cacheType);

return null;
}

return cache[index];
}

/** <summary> Locates a string in the List and gets its Index. </summary>

<param name = "str"> The String </param>

<returns> The String index or <c>-1</c> if no ocurrence was found. </returns> **/

public int IndexOf(string str, StringCacheType cacheType)
{
var lookup = GetLookup(cacheType);

return lookup.TryGetValue(str, out int index) ? index : -1;
}

/** <summary> Checks if a String is Contained in the List or not. </summary>

<param name = "str"> The String to Check. </param>

<returns> <b>true</b> if the String exists; otherwise, <b>false</b> </returns> **/

public bool Contains(string str, StringCacheType cacheType)
{
var lookup = GetLookup(cacheType);

return lookup.ContainsKey(str);
}

/// <summary> Removes all Strings from List. </summary>

public void Clear()
{
_nativeLookup.Clear();
_nativeStrings.Clear();
	
_unicodeLookup.Clear();
_unicodeStrings.Clear();

_nativeStringsLength = 0;
_unicodeStringsLength = 0;
}

}

}