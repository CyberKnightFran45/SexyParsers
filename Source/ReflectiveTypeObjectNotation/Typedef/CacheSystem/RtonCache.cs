using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace SexyParsers.ReflectiveTypeObjectNotation
{
/// <summary> Stores cached <c>ReferenceStrings</c> for RTON buffers </summary>

public static class RtonCache
{
// Represents a Key used for Cache lookup

private readonly record struct RtonCacheKey(ulong Hash, int Length);

// Cached strings for RTON files

private static readonly ConcurrentDictionary<RtonCacheKey, ReferenceStrings> CachedStrings = new();

// Cache hits

private static long _hits;

// Cache misses

private static long _misses;

// Duplicate adds

private static long _reuseCount;

// Compute hash (FNV-1a 64)

private static ulong ComputeHash(ReadOnlySpan<byte> buffer)
{
const ulong OFFSET = 14695981039346656037UL;
const ulong PRIME = 1099511628211UL;

ulong hash = OFFSET;

foreach(byte b in buffer)
{
hash ^= b;
hash *= PRIME;
}

return hash;
}

// Make key for RTON cache

private static RtonCacheKey MakeKey(ReadOnlySpan<byte> buffer)
{
ulong hash = ComputeHash(buffer);

return new(hash, buffer.Length);
}

/// <summary> Gets the cached reference strings associated with an RTON buffer </summary>

public static ReferenceStrings GetStrings(ReadOnlySpan<byte> buffer)
{
var k = MakeKey(buffer);

if(CachedStrings.TryGetValue(k, out var strCache) )
{
Interlocked.Increment(ref _hits);

return strCache;
}

Interlocked.Increment(ref _misses);

return new();
}

/// <summary> Adds reference strings associated with an RTON buffer </summary>

public static void AddStrings(ReadOnlySpan<byte> buffer, ReferenceStrings strCache)
{
var k = MakeKey(buffer);

if(!CachedStrings.TryAdd(k, strCache) )
Interlocked.Increment(ref _reuseCount);

}

// Debug Cache (Core logic)

private static void DebugCore(Stream writer)
{
const int HASH_WIDTH = 16;

const int COUNT_WIDTH = 15;
const int SIZE_WIDTH = 14;

writer.WriteLine("================  RTON Cache Status  ================\n\n");

// Table header

var header = $"{"Hash", -HASH_WIDTH} | " +
             $"{"Native Strings", COUNT_WIDTH} | " +
             $"{"Native Size", SIZE_WIDTH} | " +
             $"{"Unicode Strings", COUNT_WIDTH} | " +
             $"{"Unicode Size", SIZE_WIDTH}";

writer.WriteLine(header);

// Separator

var separator = $"{new string('-', HASH_WIDTH)}-+-" +
                $"{new string('-', COUNT_WIDTH)}-+-" +
                $"{new string('-', SIZE_WIDTH)}-+-" +
                $"{new string('-', COUNT_WIDTH)}-+-" +
                $"{new string('-', SIZE_WIDTH)}";

writer.WriteLine(separator);

int totalNativeCount = 0;
int totalUnicodeCount = 0;

long totalNativeSize = 0;
long totalUnicodeSize = 0;

// Entries

foreach(var entry in CachedStrings)
{
ulong hash = entry.Key.Hash;
var cache = entry.Value;

int nativeCount = cache.NativeStringsCount;
int unicodeCount = cache.UnicodeStringsCount;

long nativeSize = cache.NativeCacheSize;
long unicodeSize = cache.UnicodeCacheSize;

var entryInfo = $"{hash:X16} | " +
                $"{nativeCount, COUNT_WIDTH} | " +
                $"{SizeT.FormatSize(nativeSize), SIZE_WIDTH} | " +
                $"{unicodeCount, COUNT_WIDTH} | " +
                $"{SizeT.FormatSize(unicodeSize), SIZE_WIDTH}";

writer.WriteLine(entryInfo);

totalNativeCount += nativeCount;
totalUnicodeCount += unicodeCount;

totalNativeSize += nativeSize;
totalUnicodeSize += unicodeSize;
}

writer.WriteLine("\n");

// Global info

var globalDisplaySizeN = SizeT.FormatSize(totalNativeSize);
var globalDisplaySizeU = SizeT.FormatSize(totalUnicodeSize);

long timesReused = Interlocked.Read(ref _reuseCount);

writer.WriteLine($"[GLOBAL] Entries: {CachedStrings.Count} | Reused: {timesReused}\n");

// NativeStrings info

var nativeInfo = $"Strings: {totalNativeCount} | Cache Size: {globalDisplaySizeN}";

writer.WriteLine($"[NATIVE] {nativeInfo}\n");

// UnicodeStrings info

var unicodeInfo = $"Strings: {totalUnicodeCount} | Cache Size: {globalDisplaySizeU}";

writer.WriteLine($"[UNICODE] {unicodeInfo}\n");

// Statistics

long hits = Interlocked.Read(ref _hits);
long misses = Interlocked.Read(ref _misses);

long total = hits + misses;
double hitRate = total == 0 ? 0 : hits * 100.0 / total;

var cacheStats = $"Hits: {hits} | Misses: {misses} | Hit Rate: {hitRate:0.##}%";

writer.WriteLine($"[STATS] {cacheStats}");
}

/// <summary> Debugs the current cache status </summary>

public static void Debug(string logPath)
{
using var writer = FileManager.OpenWrite(logPath);

DebugCore(writer);
}

/// <summary> Removes all cached RTON reference strings </summary>

public static void Clear()
{
CachedStrings.Clear();

_hits = 0;
_misses = 0;

_reuseCount = 0;
}

}

}