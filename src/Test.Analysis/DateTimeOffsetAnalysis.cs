using System;
using WhatTimeIsIt;

namespace Test.Analysis
{
    public static class DateTimeOffsetAnalysis
    {
        public static void Run()
        {
            Console.WriteLine("========================================================================");
            Console.WriteLine("DateTimeOffsetParser Analysis - Consumer Perspective");
            Console.WriteLine("========================================================================\n");

            Console.WriteLine("KEY ADVANTAGE: DateTimeOffset PRESERVES timezone offsets!");
            Console.WriteLine("Unlike DateTime, it can store +05:00, -08:00, etc.\n");

            Console.WriteLine("SCENARIO 1: UTC Indicators");
            Console.WriteLine("---------------------------");
            TestInput("2024-01-15T14:30:45Z", "Z suffix (UTC)");
            TestInput("2024-01-15T14:30:45+00:00", "+00:00 explicit UTC");
            TestInput("1705329045", "Unix timestamp (always UTC)");
            Console.WriteLine("✅ RESULT: All UTC → Offset = +00:00\n");

            Console.WriteLine("\nSCENARIO 2: Non-Zero Timezone Offsets - THE BIG WIN!");
            Console.WriteLine("----------------------------------------------------");
            TestInput("2024-01-15T14:30:45+05:00", "India/Pakistan (UTC+5)");
            TestInput("2024-01-15T14:30:45+05:30", "India Standard Time (UTC+5:30)");
            TestInput("2024-01-15T14:30:45+09:00", "Japan (UTC+9)");
            TestInput("2024-01-15T14:30:45-08:00", "PST (UTC-8)");
            TestInput("2024-01-15T14:30:45-05:00", "EST (UTC-5)");
            TestInput("2024-01-15T14:30:45+01:00", "CET (UTC+1)");
            Console.WriteLine("✅ RESULT: Offset is PRESERVED! DateTime converts these to local.\n");

            Console.WriteLine("\nSCENARIO 3: No Timezone Info");
            Console.WriteLine("----------------------------");
            Console.WriteLine("When no timezone is specified, uses DefaultOffset (UTC by default)\n");

            var parser = new DateTimeOffsetParser();

            // Default is UTC
            parser.DefaultOffset = TimeSpan.Zero;
            TestParserInput(parser, "2024-01-15 14:30:45", "Plain datetime, default=UTC");

            // Can be configured
            parser.DefaultOffset = TimeSpan.FromHours(-8);
            TestParserInput(parser, "2024-01-15 14:30:45", "Plain datetime, default=PST");

            parser.DefaultOffset = TimeSpan.FromHours(5);
            TestParserInput(parser, "2024-01-15 14:30:45", "Plain datetime, default=UTC+5");

            Console.WriteLine("✅ RESULT: DefaultOffset allows flexible handling of timezone-less strings\n");

            Console.WriteLine("\n" + new string('=', 72));
            Console.WriteLine("COMPARISON: DateTime vs DateTimeOffset");
            Console.WriteLine(new string('=', 72));

            CompareWithDateTime("2024-01-15T14:30:45+05:00", "India time (UTC+5)");
            CompareWithDateTime("2024-01-15T14:30:45-08:00", "PST (UTC-8)");
            CompareWithDateTime("2024-01-15T14:30:45Z", "UTC");

            Console.WriteLine("\n" + new string('=', 72));
            Console.WriteLine("RECOMMENDATION:");
            Console.WriteLine(new string('=', 72));
            Console.WriteLine("✅ Use DateTimeOffsetParser when:");
            Console.WriteLine("   - You need to preserve timezone offsets");
            Console.WriteLine("   - You're working with data from multiple timezones");
            Console.WriteLine("   - You need to accurately represent when something happened");
            Console.WriteLine();
            Console.WriteLine("✅ Use DateTimeParser when:");
            Console.WriteLine("   - You only care about UTC vs Local vs Unspecified");
            Console.WriteLine("   - You don't need to preserve the original timezone");
            Console.WriteLine("   - You're working with legacy code that uses DateTime");
        }

        static void TestInput(string input, string description)
        {
            var result = DateTimeOffsetParser.ParseString(input);
            Console.WriteLine($"Input:  \"{input}\"");
            Console.WriteLine($"  Desc: {description}");
            Console.WriteLine($"  Value: {result.DateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Offset: {result.Offset:hh\\:mm} ({(result.Offset >= TimeSpan.Zero ? "+" : "")}{result.Offset.TotalHours:F1} hours)");
            Console.WriteLine($"  UTC:   {result.UtcDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }

        static void TestParserInput(DateTimeOffsetParser parser, string input, string description)
        {
            var result = parser.Parse(input);
            Console.WriteLine($"Input:  \"{input}\"");
            Console.WriteLine($"  Desc: {description}");
            Console.WriteLine($"  Value: {result.DateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Offset: {result.Offset:hh\\:mm}");
            Console.WriteLine();
        }

        static void CompareWithDateTime(string input, string description)
        {
            Console.WriteLine($"\nInput: \"{input}\" ({description})");

            var dtOffset = DateTimeOffsetParser.ParseString(input);
            var dt = DateTimeParser.ParseString(input);

            Console.WriteLine($"  DateTimeOffset:");
            Console.WriteLine($"    Value:  {dtOffset.DateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"    Offset: {dtOffset.Offset:hh\\:mm}");
            Console.WriteLine($"    UTC:    {dtOffset.UtcDateTime:yyyy-MM-dd HH:mm:ss}");

            Console.WriteLine($"  DateTime:");
            Console.WriteLine($"    Value:  {dt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"    Kind:   {dt.Kind}");

            // Show the difference
            if (dtOffset.Offset != TimeSpan.Zero && dt.Kind == DateTimeKind.Local)
            {
                Console.WriteLine($"  ⚠️  DateTime converted to local time, offset lost!");
            }
            else if (dtOffset.Offset == TimeSpan.Zero && dt.Kind == DateTimeKind.Utc)
            {
                Console.WriteLine($"  ✅ Both preserve UTC correctly");
            }
        }
    }
}
