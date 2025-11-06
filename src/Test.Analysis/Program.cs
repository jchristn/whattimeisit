using System;
using System.Globalization;
using WhatTimeIsIt;
using Test.Analysis;

class Program
{
    static void Main()
    {
        TestTimezoneOffsetBehavior();
        Console.WriteLine("\n\n");
        TestNonZeroOffsets();
        Console.WriteLine("\n\n");
        AnalyzeLibraryBehavior();
        Console.WriteLine("\n\n");
        DateTimeOffsetAnalysis.Run();
    }

    static void TestNonZeroOffsets()
    {
        Console.WriteLine("Testing Non-Zero Timezone Offsets");
        Console.WriteLine("==================================\n");

        var testCases = new[]
        {
            "2024-01-15T14:30:45Z",        // UTC (zero offset)
            "2024-01-15T14:30:45+00:00",   // UTC (explicit zero offset)
            "2024-01-15T14:30:45+05:00",   // UTC+5 (Karachi, etc.)
            "2024-01-15T14:30:45-08:00",   // UTC-8 (PST)
            "2024-01-15T14:30:45+01:00",   // UTC+1 (CET)
        };

        foreach (var input in testCases)
        {
            try
            {
                var result = DateTimeParser.ParseString(input);
                Console.WriteLine($"Input:  {input}");
                Console.WriteLine($"Output: {result:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Kind:   {result.Kind}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Input:  {input}");
                Console.WriteLine($"ERROR:  {ex.Message}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("Key Finding:");
        Console.WriteLine("- Zero offsets (+00, -00, Z) → Kind=Utc, time preserved");
        Console.WriteLine("- Non-zero offsets (+05, -08) → Converted to local time, Kind=Local");
        Console.WriteLine("\nThis is .NET's standard behavior with DateTimeStyles.RoundtripKind");
        Console.WriteLine("If you need to preserve non-zero offsets, use DateTimeOffset instead of DateTime");
    }

    static void TestTimezoneOffsetBehavior()
    {
        Console.WriteLine(".NET DateTime Parsing Behavior with Timezone Offsets");
        Console.WriteLine("====================================================\n");

        var formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss+00",      // Literal +00
            "yyyy-MM-dd HH:mm:sszzz",      // Timezone offset specifier
            "yyyy-MM-dd HH:mm:ssK",        // Timezone kind specifier
            "yyyy-MM-dd HH:mm:ss'Z'",      // Literal Z
        };

        var inputs = new[]
        {
            "2024-01-15 14:30:45+00",
            "2024-01-15 14:30:45+00:00",
            "2024-01-15 14:30:45Z",
        };

        foreach (var input in inputs)
        {
            Console.WriteLine($"Input: '{input}'");

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var result))
                {
                    Console.WriteLine($"  Format '{format}'");
                    Console.WriteLine($"    -> Value={result:yyyy-MM-dd HH:mm:ss} Kind={result.Kind}");
                }
            }
            Console.WriteLine();
        }

        Console.WriteLine("Conclusion:");
        Console.WriteLine("- Literal '+00' in format does NOT set Kind=Utc (it's just text matching)");
        Console.WriteLine("- Only 'Z' suffix with literal format sets Kind=Utc");
        Console.WriteLine("- This is a .NET limitation, not fixable in the library\n");
    }

    static void AnalyzeLibraryBehavior()
    {
        Console.WriteLine("DateTime Parser Analysis - Consumer Perspective");
        Console.WriteLine("================================================\n");

        var testCases = new[]
        {
            // Scenario 1: No timezone info - what would a developer expect?
            ("2024-01-15 14:30:45", "Plain datetime, no timezone"),
            ("2024-01-15T14:30:45.123456", "ISO format, no timezone"),

            // Scenario 2: Explicit UTC indicator
            ("2024-01-15 14:30:45Z", "UTC with Z suffix"),
            ("2024-01-15T14:30:45.123456Z", "ISO UTC with Z"),
            ("2024-01-15 14:30:45+00", "UTC with +00"),

            // Scenario 3: Database formats (typically stored as local or unspecified)
            ("2024-01-15 14:30:45.123456", "SQL Server datetime2 (no tz)"),
            ("15-Jan-24 14.30.45.123456", "Oracle (no tz)"),

            // Scenario 4: Unix timestamps (inherently UTC)
            ("1705329045", "Unix timestamp (seconds)"),
            ("1705329045000", "Unix timestamp (milliseconds)"),

            // Scenario 5: .NET ticks
            (new DateTime(2024, 1, 15, 14, 30, 45).Ticks.ToString(), ".NET Ticks"),

            // Scenario 6: Compact formats
            ("20240115", "Compact date only"),
            ("20240115143045", "Compact datetime"),
        };

        foreach (var (input, description) in testCases)
        {
            try
            {
                var result = DateTimeParser.ParseString(input);

                Console.WriteLine($"Input: {input}");
                Console.WriteLine($"  Description: {description}");
                Console.WriteLine($"  Result Value: {result:yyyy-MM-dd HH:mm:ss.ffffff}");
                Console.WriteLine($"  Result Kind: {result.Kind}");
                Console.WriteLine($"  Expected by consumer?");

                // Analysis
                if (input.EndsWith("Z") || input.EndsWith("+00") || input.EndsWith("-00"))
                {
                    if (result.Kind == DateTimeKind.Utc)
                        Console.WriteLine($"    ✓ UTC indicator present, Kind=UTC is CORRECT");
                    else
                        Console.WriteLine($"    ✗ UTC indicator present but Kind={result.Kind} - UNEXPECTED!");
                }
                else if (input.All(char.IsDigit) && input.Length >= 10 && input.Length <= 13)
                {
                    // Unix timestamp
                    Console.WriteLine($"    ? Unix timestamp converted to LocalDateTime");
                    Console.WriteLine($"      Consumer expectation: Probably UTC with Kind=UTC");
                    Console.WriteLine($"      Actual: Local time with Kind={result.Kind}");
                    if (result.Kind != DateTimeKind.Utc)
                        Console.WriteLine($"    ⚠ POTENTIAL ISSUE: Unix timestamps are UTC by definition");
                }
                else
                {
                    Console.WriteLine($"    ? No timezone info, Kind={result.Kind}");
                    Console.WriteLine($"      Consumer expectation: Probably Unspecified or Local");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Input: {input}");
                Console.WriteLine($"  ERROR: {ex.Message}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("\n================================================");
        Console.WriteLine("Key Findings:");
        Console.WriteLine("================================================\n");

        // Test specific scenarios
        Console.WriteLine("1. UTC Strings with 'Z' suffix:");
        var utcTest = DateTimeParser.ParseString("2024-01-15T14:30:45Z");
        Console.WriteLine($"   Input: 2024-01-15T14:30:45Z");
        Console.WriteLine($"   Value: {utcTest:yyyy-MM-dd HH:mm:ss} Kind={utcTest.Kind}");
        Console.WriteLine($"   Expected: 2024-01-15 14:30:45 UTC (Kind=Utc)");
        if (utcTest.Kind == DateTimeKind.Utc && utcTest.Hour == 14)
            Console.WriteLine($"   ✓ CORRECT - Preserves UTC time and Kind");
        else if (utcTest.Kind != DateTimeKind.Utc)
            Console.WriteLine($"   ✗ WRONG - Should be Kind=Utc");
        else
            Console.WriteLine($"   ✗ WRONG - UTC time was converted to local");

        Console.WriteLine("\n2. Unix Timestamps (UTC by definition):");
        var unixTest = DateTimeParser.ParseString("1705329045");
        var expectedUtc = DateTimeOffset.FromUnixTimeSeconds(1705329045).UtcDateTime;
        Console.WriteLine($"   Input: 1705329045 (Unix seconds)");
        Console.WriteLine($"   Value: {unixTest:yyyy-MM-dd HH:mm:ss} Kind={unixTest.Kind}");
        Console.WriteLine($"   Expected: {expectedUtc:yyyy-MM-dd HH:mm:ss} UTC (Kind=Utc)");
        Console.WriteLine($"   Actual behavior: Converted to local time");
        if (unixTest.Kind == DateTimeKind.Utc)
            Console.WriteLine($"   ✓ CORRECT - Unix timestamp as UTC");
        else
            Console.WriteLine($"   ✗ ISSUE - Unix timestamps should be UTC, not {unixTest.Kind}");

        Console.WriteLine("\n3. Plain datetime strings (no timezone):");
        var plainTest = DateTimeParser.ParseString("2024-01-15 14:30:45");
        Console.WriteLine($"   Input: 2024-01-15 14:30:45");
        Console.WriteLine($"   Value: {plainTest:yyyy-MM-dd HH:mm:ss} Kind={plainTest.Kind}");
        Console.WriteLine($"   Expected: Kind=Unspecified (no timezone info provided)");
        if (plainTest.Kind == DateTimeKind.Unspecified)
            Console.WriteLine($"   ✓ CORRECT - No timezone info means Unspecified");
        else if (plainTest.Kind == DateTimeKind.Local)
            Console.WriteLine($"   ? ACCEPTABLE - Could be Local or Unspecified");
        else
            Console.WriteLine($"   ✗ WRONG - Should not be UTC without indicator");
    }
}
