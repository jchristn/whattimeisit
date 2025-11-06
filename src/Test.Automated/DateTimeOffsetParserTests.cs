namespace Test.Automated
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using WhatTimeIsIt;

    public static class DateTimeOffsetParserTests
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8604 // Possible null reference argument.

        private static int _totalTests = 0;
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _failureDetails = new List<string>();

        public static void RunAll()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("DateTimeOffsetParser Test Suite");
            Console.WriteLine("========================================\n");

            TestStaticMethods();
            TestInstanceMethods();
            TestTimezonePreservation();
            TestAllFormatsWithTimezones();
            TestNumericFormats();
            TestDefaultOffsetBehavior();
            TestPrecisionPreservation();
            TestEdgeCases();
            TestNegativeCases();
            TestFormatPropertyBehavior();
            TestCultureHandling();
            TestTryParseMethods();

            // Print summary
            Console.WriteLine("\n========================================");
            Console.WriteLine("DateTimeOffsetParser Test Summary");
            Console.WriteLine("========================================");
            Console.WriteLine($"Total Tests: {_totalTests}");
            Console.WriteLine($"Passed: {_passedTests}");
            Console.WriteLine($"Failed: {_failedTests}");
            Console.WriteLine($"Success Rate: {(_passedTests * 100.0 / _totalTests):F2}%");

            if (_failureDetails.Count > 0)
            {
                Console.WriteLine("\nFailure Details:");
                foreach (var failure in _failureDetails)
                {
                    Console.WriteLine($"  - {failure}");
                }
            }
        }

        public static int GetFailedCount() => _failedTests;

        static void TestStaticMethods()
        {
            Console.WriteLine("Testing Static Methods...");

            // Basic static parse with UTC
            TestParse("2024-01-15T14:30:45Z",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                     "Static ParseString with UTC");

            // Static parse with timezone offset
            TestParse("2024-01-15T14:30:45+05:00",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(5)),
                     "Static ParseString with +05:00 offset");

            // Static parse with custom formats
            var customFormats = new[] { "yyyy/MM/dd HH:mm:ss" };
            TestParseWithFormats("2024/01/15 14:30:45",
                                customFormats,
                                TimeSpan.Zero,
                                new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                                "Static ParseString with custom format");
        }

        static void TestInstanceMethods()
        {
            Console.WriteLine("\nTesting Instance Methods...");

            var parser = new DateTimeOffsetParser();

            // Test instance parse with default formats
            TestInstanceParse(parser, "2024-01-15T14:30:45Z",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                            "Instance Parse with UTC");

            // Test instance with custom formats and offset
            parser.Formats = new[] { "dd-MMM-yyyy HH:mm:ss" };
            parser.DefaultOffset = TimeSpan.FromHours(-8);
            TestInstanceParse(parser, "15-Jan-2024 14:30:45",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(-8)),
                            "Instance Parse with custom formats and PST offset");

            // Reset and test
            parser.ResetToDefaults();
            TestInstanceParse(parser, "2024-01-15 14:30:45",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                            "Instance Parse after reset");
        }

        static void TestTimezonePreservation()
        {
            Console.WriteLine("\nTesting Timezone Offset Preservation...");

            // Test various timezone offsets are preserved
            TestParse("2024-01-15T14:30:45+00:00",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                     "UTC with +00:00");

            TestParse("2024-01-15T14:30:45+05:30",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromMinutes(330)),
                     "India Standard Time +05:30");

            TestParse("2024-01-15T14:30:45-08:00",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(-8)),
                     "PST -08:00");

            TestParse("2024-01-15T14:30:45+09:00",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(9)),
                     "JST +09:00");

            TestParse("2024-01-15T14:30:45-05:00",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(-5)),
                     "EST -05:00");
        }

        static void TestAllFormatsWithTimezones()
        {
            Console.WriteLine("\nTesting All Supported Formats with Timezones...");

            var testCases = new Dictionary<string, DateTimeOffset>
            {
                // 7-digit precision with timezones
                ["2024-01-15T14:30:45.1234567Z"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero).AddTicks(1234567),
                ["2024-01-15T14:30:45.1234567+05:00"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(5)).AddTicks(1234567),

                // 6-digit precision with timezones
                ["2024-01-15T14:30:45.123456Z"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero).AddTicks(1234560),
                ["2024-01-15T14:30:45.123456-08:00"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(-8)).AddTicks(1234560),

                // 3-digit precision with timezones
                ["2024-01-15T14:30:45.123Z"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, 123, TimeSpan.Zero),
                ["2024-01-15T14:30:45.123+01:00"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, 123, TimeSpan.FromHours(1)),

                // Seconds precision with timezones
                ["2024-01-15T14:30:45Z"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                ["2024-01-15T14:30:45+00:00"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),

                // Without timezone (should default to UTC/zero offset)
                ["2024-01-15 14:30:45"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                ["2024-01-15T14:30:45"] = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),

                // Date only formats
                ["2024-01-15"] = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
            };

            foreach (var kvp in testCases)
            {
                TestParse(kvp.Key, kvp.Value, $"Format: {kvp.Key}");
            }
        }

        static void TestNumericFormats()
        {
            Console.WriteLine("\nTesting Numeric Formats (Unix timestamps, Ticks)...");

            // Unix timestamp in seconds - should be UTC
            TestParse("1705329045",
                     DateTimeOffset.FromUnixTimeSeconds(1705329045),
                     "Unix timestamp (seconds)");

            // Unix timestamp in milliseconds - should be UTC
            TestParse("1705329045000",
                     DateTimeOffset.FromUnixTimeMilliseconds(1705329045000),
                     "Unix timestamp (milliseconds)");

            // .NET Ticks - should use default offset (UTC)
            long ticks = new DateTime(2024, 1, 15, 14, 30, 45).Ticks;
            TestParse(ticks.ToString(),
                     new DateTimeOffset(new DateTime(ticks), TimeSpan.Zero),
                     ".NET Ticks");
        }

        static void TestDefaultOffsetBehavior()
        {
            Console.WriteLine("\nTesting Default Offset Behavior...");

            var parser = new DateTimeOffsetParser();

            // Default should be UTC (zero offset)
            parser.DefaultOffset = TimeSpan.Zero;
            TestInstanceParse(parser, "2024-01-15 14:30:45",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                            "Default offset UTC");

            // Set to PST
            parser.DefaultOffset = TimeSpan.FromHours(-8);
            TestInstanceParse(parser, "2024-01-15 14:30:45",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(-8)),
                            "Default offset PST");

            // Explicit timezone in input should override default
            TestInstanceParse(parser, "2024-01-15T14:30:45+05:00",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(5)),
                            "Explicit offset overrides default");
        }

        static void TestPrecisionPreservation()
        {
            Console.WriteLine("\nTesting Precision Preservation...");

            // Test that microseconds are preserved
            var input = "2024-01-15T14:30:45.123456Z";
            var parsed = DateTimeOffsetParser.ParseString(input);
            var expected = new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero).AddTicks(1234560);

            var success = parsed == expected;
            RecordTest("Microsecond precision preservation", success);

            if (!success)
            {
                Console.WriteLine($"  Expected ticks: {expected.Ticks}");
                Console.WriteLine($"  Actual ticks: {parsed.Ticks}");
                Console.WriteLine($"  Difference: {parsed.Ticks - expected.Ticks} ticks");
            }
        }

        static void TestEdgeCases()
        {
            Console.WriteLine("\nTesting Edge Cases...");

            // Leap year
            TestParse("2024-02-29T00:00:00Z",
                     new DateTimeOffset(2024, 2, 29, 0, 0, 0, TimeSpan.Zero),
                     "Leap year date");

            // End of year
            TestParse("2024-12-31T23:59:59.999999Z",
                     new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero).AddTicks(9999990),
                     "End of year with microseconds");

            // Start of year
            TestParse("2024-01-01T00:00:00Z",
                     new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                     "Start of year");
        }

        static void TestNegativeCases()
        {
            Console.WriteLine("\nTesting Negative Cases (Should Fail)...");

            var invalidInputs = new[]
            {
                null,
                "",
                "   ",
                "not-a-date",
                "2024-13-01",
                "2024-01-32",
                "2023-02-29",
                "2024-01-15 25:00:00",
                "abc123xyz",
            };

            foreach (var input in invalidInputs)
            {
                TestNegativeParse(input, $"Invalid input: '{input ?? "null"}'");
            }
        }

        static void TestFormatPropertyBehavior()
        {
            Console.WriteLine("\nTesting Format Property Behavior...");

            var parser = new DateTimeOffsetParser();

            // Test null assignment
            parser.Formats = null;
            TestInstanceParse(parser, "2024-01-15T14:30:45Z",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                            "Formats=null should use defaults");

            // Test empty array assignment
            parser.Formats = new string[0];
            TestInstanceParse(parser, "2024-01-15T14:30:45Z",
                            new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero),
                            "Formats=empty should use defaults");

            // Test accessing DefaultFormats doesn't modify original
            var defaults = DateTimeOffsetParser.DefaultFormats;
            defaults[0] = "modified";
            TestParse("2024-01-15T14:30:45.1234567Z",
                     new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero).AddTicks(1234567),
                     "DefaultFormats should return a clone");
        }

        static void TestCultureHandling()
        {
            Console.WriteLine("\nTesting Culture Handling...");

            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                // Test with different cultures
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                TestParse("01/15/2024",
                         new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
                         "US culture date");

                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                TestParse("15/01/2024",
                         new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
                         "UK culture date");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        static void TestTryParseMethods()
        {
            Console.WriteLine("\nTesting TryParse Methods...");

            // Static TryParse
            DateTimeOffset result;
            bool success = DateTimeOffsetParser.TryParseString("2024-01-15T14:30:45Z", out result);
            RecordTest("Static TryParse valid input",
                      success && result == new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));

            success = DateTimeOffsetParser.TryParseString("invalid-date", out result);
            RecordTest("Static TryParse invalid input", !success && result == DateTimeOffset.MinValue);

            // Instance TryParse
            var parser = new DateTimeOffsetParser();
            success = parser.TryParse("2024-01-15T14:30:45+05:00", out result);
            RecordTest("Instance TryParse valid input",
                      success && result == new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.FromHours(5)));

            success = parser.TryParse("not-a-date", out result);
            RecordTest("Instance TryParse invalid input", !success && result == DateTimeOffset.MinValue);
        }

        // Helper methods
        static void TestParse(string input, DateTimeOffset expected, string description)
        {
            try
            {
                var result = DateTimeOffsetParser.ParseString(input);
                var success = result == expected && result.Offset == expected.Offset;
                RecordTest(description, success);

                if (!success)
                {
                    _failureDetails.Add($"{description}: Expected {expected:yyyy-MM-dd HH:mm:ss.ffffff zzz}, got {result:yyyy-MM-dd HH:mm:ss.ffffff zzz}");
                }
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Exception - {ex.Message}");
            }
        }

        static void TestParseWithFormats(string input, string[] formats, TimeSpan defaultOffset, DateTimeOffset expected, string description)
        {
            try
            {
                var result = DateTimeOffsetParser.ParseString(input, formats, defaultOffset);
                var success = result == expected && result.Offset == expected.Offset;
                RecordTest(description, success);

                if (!success)
                {
                    _failureDetails.Add($"{description}: Expected {expected:yyyy-MM-dd HH:mm:ss.ffffff zzz}, got {result:yyyy-MM-dd HH:mm:ss.ffffff zzz}");
                }
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Exception - {ex.Message}");
            }
        }

        static void TestInstanceParse(DateTimeOffsetParser parser, string input, DateTimeOffset expected, string description)
        {
            try
            {
                var result = parser.Parse(input);
                var success = result == expected && result.Offset == expected.Offset;
                RecordTest(description, success);

                if (!success)
                {
                    _failureDetails.Add($"{description}: Expected {expected:yyyy-MM-dd HH:mm:ss.ffffff zzz}, got {result:yyyy-MM-dd HH:mm:ss.ffffff zzz}");
                }
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Exception - {ex.Message}");
            }
        }

        static void TestNegativeParse(string input, string description)
        {
            try
            {
                var result = DateTimeOffsetParser.ParseString(input);
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Should have thrown but returned {result}");
            }
            catch (ArgumentNullException)
            {
                RecordTest(description, true);
            }
            catch (FormatException)
            {
                RecordTest(description, true);
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Unexpected exception type - {ex.GetType().Name}: {ex.Message}");
            }
        }

        static void RecordTest(string testName, bool passed)
        {
            _totalTests++;
            if (passed)
            {
                _passedTests++;
                Console.WriteLine($"  ✓ {testName}");
            }
            else
            {
                _failedTests++;
                Console.WriteLine($"  ✗ {testName}");
            }
        }

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
