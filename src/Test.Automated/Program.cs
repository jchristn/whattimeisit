namespace Test.Automated
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using WhatTimeIsIt;

    class Program
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8604 // Possible null reference argument.

        private static int _totalTests = 0;
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _failureDetails = new List<string>();

        public static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("DateTimeParser Comprehensive Test Suite");
            Console.WriteLine("========================================\n");

            // Test categories
            TestStaticMethods();
            TestInstanceMethods();
            TestAllFormats();
            TestNumericFormats();
            TestTimezoneHandling();
            TestPrecisionPreservation();
            TestEdgeCases();
            TestNegativeCases();
            TestFormatPropertyBehavior();
            TestCultureHandling();
            TestOracleSpecialFormats();
            TestTryParseMethods();

            // Print summary
            Console.WriteLine("\n========================================");
            Console.WriteLine("Test Summary");
            Console.WriteLine("========================================");
            Console.WriteLine($"Total Tests: {_totalTests}");
            Console.WriteLine($"Passed: {_passedTests}");
            Console.WriteLine($"Failed: {_failedTests}");
            Console.WriteLine($"Success Rate: {(_passedTests * 100.0 / _totalTests):F2}%");

            if (_failureDetails.Count > 0)
            {
                Console.WriteLine("\nFailure Details:");
                foreach (var failure in _failureDetails.Take(10)) // Show first 10 failures
                {
                    Console.WriteLine($"  - {failure}");
                }
                if (_failureDetails.Count > 10)
                {
                    Console.WriteLine($"  ... and {_failureDetails.Count - 10} more failures");
                }
            }

            // Run DateTimeOffsetParser tests
            DateTimeOffsetParserTests.RunAll();

            // Combine results
            _failedTests += DateTimeOffsetParserTests.GetFailedCount();

            Environment.Exit(_failedTests > 0 ? 1 : 0);
        }

        static void TestStaticMethods()
        {
            Console.WriteLine("Testing Static Methods...");

            // Basic static parse
            TestParse("2024-01-15 14:30:45.123456",
                     new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),
                     "Static ParseString with microseconds");

            // Static parse with custom formats
            var customFormats = new[] { "yyyy/MM/dd HH:mm:ss" };
            TestParseWithFormats("2024/01/15 14:30:45",
                                customFormats,
                                new DateTime(2024, 1, 15, 14, 30, 45),
                                "Static ParseString with custom format");
        }

        static void TestInstanceMethods()
        {
            Console.WriteLine("\nTesting Instance Methods...");

            var parser = new DateTimeParser();

            // Test instance parse with default formats
            TestInstanceParse(parser, "2024-01-15T14:30:45.123456Z",
                            null, // Expected will be calculated considering timezone
                            "Instance Parse with default formats");

            // Test instance with custom formats
            parser.Formats = new[] { "dd-MMM-yyyy HH:mm:ss" };
            TestInstanceParse(parser, "15-Jan-2024 14:30:45",
                            new DateTime(2024, 1, 15, 14, 30, 45),
                            "Instance Parse with custom formats");

            // Reset and test
            parser.ResetToDefaults();
            TestInstanceParse(parser, "2024-01-15 14:30:45",
                            new DateTime(2024, 1, 15, 14, 30, 45),
                            "Instance Parse after reset");
        }

        static void TestAllFormats()
        {
            Console.WriteLine("\nTesting All Supported Formats...");

            var testCases = new Dictionary<string, DateTime>
            {
                // 7-digit precision
                ["2024-01-15 14:30:45.1234567"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4567),
                ["2024-01-15T14:30:45.1234567"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4567),
                ["2024-01-15 14:30:45.1234567Z"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4567),
                ["2024-01-15T14:30:45.1234567Z"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4567),

                // 6-digit precision
                ["2024-01-15 14:30:45.123456"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),
                ["2024-01-15T14:30:45.123456"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),
                ["2024-01-15 14:30:45.123456Z"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),
                ["2024-01-15T14:30:45.123456Z"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),

                // 5-digit precision
                ["2024-01-15 14:30:45.12345"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4500),
                ["2024-01-15T14:30:45.12345"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4500),

                // 4-digit precision
                ["2024-01-15 14:30:45.1234"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4000),
                ["2024-01-15T14:30:45.1234"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4000),

                // 3-digit precision (milliseconds)
                ["2024-01-15 14:30:45.123"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
                ["2024-01-15T14:30:45.123"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
                ["2024-01-15 14:30:45.123Z"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
                ["Jan 15 2024 02:30:45:123PM"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
                ["01/15/2024 14:30:45.123"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
                ["15/01/2024 14:30:45.123"] = new DateTime(2024, 1, 15, 14, 30, 45, 123), // Fixed

                // 2-digit precision
                ["2024-01-15 14:30:45.12"] = new DateTime(2024, 1, 15, 14, 30, 45, 120),
                ["2024-01-15T14:30:45.12"] = new DateTime(2024, 1, 15, 14, 30, 45, 120),

                // 1-digit precision
                ["2024-01-15 14:30:45.1"] = new DateTime(2024, 1, 15, 14, 30, 45, 100),
                ["2024-01-15T14:30:45.1"] = new DateTime(2024, 1, 15, 14, 30, 45, 100),

                // Seconds precision
                ["2024-01-15 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["2024-01-15T14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["2024/01/15 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["2024.01.15 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["15/01/2024 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["01/15/2024 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["15.01.2024 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["15-01-2024 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["15-Jan-2024 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["2024-01-15 2:30:45 PM"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["2024-01-15 02:30:45 PM"] = new DateTime(2024, 1, 15, 14, 30, 45),

                // Compact formats
                ["20240115T143045Z"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["20240115T143045"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["20240115143045"] = new DateTime(2024, 1, 15, 14, 30, 45),
                ["20240115 14:30:45"] = new DateTime(2024, 1, 15, 14, 30, 45),

                // Minutes precision
                ["2024-01-15 14:30"] = new DateTime(2024, 1, 15, 14, 30, 0),
                ["2024-01-15T14:30"] = new DateTime(2024, 1, 15, 14, 30, 0),
                ["2024/01/15 14:30"] = new DateTime(2024, 1, 15, 14, 30, 0),

                // Date only
                ["2024-01-15"] = new DateTime(2024, 1, 15),
                ["2024/01/15"] = new DateTime(2024, 1, 15),
                ["01/15/2024"] = new DateTime(2024, 1, 15),
                ["15/01/2024"] = new DateTime(2024, 1, 15),
                ["15-Jan-2024"] = new DateTime(2024, 1, 15),
                ["15-Jan-24"] = new DateTime(2024, 1, 15),
                ["20240115"] = new DateTime(2024, 1, 15),
                ["2024.01.15"] = new DateTime(2024, 1, 15),
                ["15.01.2024"] = new DateTime(2024, 1, 15),

                // Oracle formats
                ["15-Jan-24 02.30.45.123456 PM"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),
                ["15-Jan-24 14.30.45.123456"] = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560),

                // SQL Server formats
                ["Jan 15 2024  2:30:45:123PM"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
                ["Jan 15 2024 14:30:45:123"] = new DateTime(2024, 1, 15, 14, 30, 45, 123),
            };

            foreach (var kvp in testCases)
            {
                TestParse(kvp.Key, kvp.Value, $"Format: {kvp.Key}");
            }
        }

        static void TestNumericFormats()
        {
            Console.WriteLine("\nTesting Numeric Formats (Unix timestamps, Ticks)...");

            // .NET Ticks
            long ticks = new DateTime(2024, 1, 15, 14, 30, 45).Ticks;
            TestParse(ticks.ToString(), new DateTime(2024, 1, 15, 14, 30, 45), ".NET Ticks");

            // Unix timestamp in seconds (Jan 15, 2024 14:30:45 UTC)
            // Unix timestamps are ALWAYS UTC - should return UTC DateTime, not local
            TestParse("1705329045", new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc), "Unix timestamp (seconds)");

            // Unix timestamp in milliseconds
            // Unix timestamps are ALWAYS UTC - should return UTC DateTime, not local
            TestParse("1705329045000", new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc), "Unix timestamp (milliseconds)");
        }

        static void TestTimezoneHandling()
        {
            Console.WriteLine("\nTesting Timezone Handling...");

            // Note: These tests may need adjustment based on local timezone
            TestParseAllowingTimezoneAdjustment("2024-01-15T14:30:45Z", "UTC format with Z");
            TestParseAllowingTimezoneAdjustment("2024-01-15T14:30:45+00", "UTC format with +00");
            TestParseAllowingTimezoneAdjustment("2024-01-15T14:30:45-00", "UTC format with -00");
        }

        static void TestPrecisionPreservation()
        {
            Console.WriteLine("\nTesting Precision Preservation...");

            // Test that microseconds are preserved
            var input = "2024-01-15 14:30:45.123456";
            var parsed = DateTimeParser.ParseString(input);
            var expected = new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560);

            var success = parsed == expected;
            RecordTest($"Microsecond precision preservation", success);

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
            TestParse("2024-02-29", new DateTime(2024, 2, 29), "Leap year date");

            // End of year
            TestParse("2024-12-31 23:59:59.999999",
                     new DateTime(2024, 12, 31, 23, 59, 59, 999).AddTicks(9990),
                     "End of year with microseconds");

            // Start of year
            TestParse("2024-01-01 00:00:00.000000",
                     new DateTime(2024, 1, 1, 0, 0, 0),
                     "Start of year");

            // Single digit month/day
            TestParse("1/5/2024 3:30:45 PM",
                     new DateTime(2024, 1, 5, 15, 30, 45),
                     "Single digit month/day");
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
                "2024-13-01", // Invalid month
                "2024-01-32", // Invalid day
                "2024-02-30", // Invalid day for February
                "2023-02-29", // Not a leap year
                "2024-01-15 25:00:00", // Invalid hour
                "2024-01-15 14:60:00", // Invalid minute
                "2024-01-15 14:30:60", // Invalid second
                "abc123xyz",
                "2024/01/15/14/30/45", // Too many slashes
                "2024--01--15",
                "99999999999999999999999999", // Number too large
            };

            foreach (var input in invalidInputs)
            {
                TestNegativeParse(input, $"Invalid input: '{input ?? "null"}'");
            }
        }

        static void TestFormatPropertyBehavior()
        {
            Console.WriteLine("\nTesting Format Property Behavior...");

            var parser = new DateTimeParser();

            // Test null assignment
            parser.Formats = null;
            TestInstanceParse(parser, "2024-01-15 14:30:45",
                            new DateTime(2024, 1, 15, 14, 30, 45),
                            "Formats=null should use defaults");

            // Test empty array assignment
            parser.Formats = new string[0];
            TestInstanceParse(parser, "2024-01-15 14:30:45",
                            new DateTime(2024, 1, 15, 14, 30, 45),
                            "Formats=empty should use defaults");

            // Test custom formats
            parser.Formats = new[] { "yyyy-MM-dd" };
            TestInstanceParse(parser, "2024-01-15",
                            new DateTime(2024, 1, 15),
                            "Custom format (date only)");

            // Test accessing DefaultFormats doesn't modify original
            var defaults = DateTimeParser.DefaultFormats;
            defaults[0] = "modified";
            TestParse("2024-01-15 14:30:45.1234567",
                     new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4567),
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
                TestParse("01/15/2024", new DateTime(2024, 1, 15), "US culture date");

                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                TestParse("15/01/2024", new DateTime(2024, 1, 15), "UK culture date");

                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                TestParse("15.01.2024", new DateTime(2024, 1, 15), "German culture date");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        static void TestOracleSpecialFormats()
        {
            Console.WriteLine("\nTesting Oracle Special Formats...");

            // Oracle formats with periods as time separators
            var input = "15-Jan-24 14.30.45";
            try
            {
                var result = DateTimeParser.ParseString(input);
                var expected = new DateTime(2024, 1, 15, 14, 30, 45);
                RecordTest("Oracle period separator format", result == expected);
            }
            catch
            {
                RecordTest("Oracle period separator format", false);
            }
        }

        static void TestTryParseMethods()
        {
            Console.WriteLine("\nTesting TryParse Methods...");

            // Static TryParse
            DateTime result;
            bool success = DateTimeParser.TryParseString("2024-01-15 14:30:45", out result);
            RecordTest("Static TryParse valid input", success && result == new DateTime(2024, 1, 15, 14, 30, 45));

            success = DateTimeParser.TryParseString("invalid-date", out result);
            RecordTest("Static TryParse invalid input", !success && result == DateTime.MinValue);

            // Instance TryParse
            var parser = new DateTimeParser();
            success = parser.TryParse("2024-01-15", out result);
            RecordTest("Instance TryParse valid input", success && result == new DateTime(2024, 1, 15));

            success = parser.TryParse("not-a-date", out result);
            RecordTest("Instance TryParse invalid input", !success && result == DateTime.MinValue);

            // TryParse with custom formats
            var formats = new[] { "dd-MMM-yyyy" };
            success = DateTimeParser.TryParseString("15-Jan-2024", formats, out result);
            RecordTest("TryParse with custom format", success && result == new DateTime(2024, 1, 15));
        }

        // Helper methods
        static void TestParse(string input, DateTime expected, string description)
        {
            try
            {
                var result = DateTimeParser.ParseString(input);
                var success = result == expected;
                RecordTest(description, success);
                if (!success)
                {
                    _failureDetails.Add($"{description}: Expected {expected:yyyy-MM-dd HH:mm:ss.ffffff}, got {result:yyyy-MM-dd HH:mm:ss.ffffff}");
                }
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Exception - {ex.Message}");
            }
        }

        static void TestParseWithFormats(string input, string[] formats, DateTime expected, string description)
        {
            try
            {
                var result = DateTimeParser.ParseString(input, formats);
                var success = result == expected;
                RecordTest(description, success);
                if (!success)
                {
                    _failureDetails.Add($"{description}: Expected {expected:yyyy-MM-dd HH:mm:ss.ffffff}, got {result:yyyy-MM-dd HH:mm:ss.ffffff}");
                }
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Exception - {ex.Message}");
            }
        }

        static void TestInstanceParse(DateTimeParser parser, string input, DateTime? expected, string description)
        {
            try
            {
                var result = parser.Parse(input);
                if (expected == null)
                {
                    // For timezone-aware formats, just check that it parses
                    RecordTest(description, true);
                }
                else
                {
                    var success = result == expected.Value;
                    RecordTest(description, success);
                    if (!success)
                    {
                        _failureDetails.Add($"{description}: Expected {expected:yyyy-MM-dd HH:mm:ss.ffffff}, got {result:yyyy-MM-dd HH:mm:ss.ffffff}");
                    }
                }
            }
            catch (Exception ex)
            {
                RecordTest(description, false);
                _failureDetails.Add($"{description}: Exception - {ex.Message}");
            }
        }

        static void TestParseAllowingTimezoneAdjustment(string input, string description)
        {
            try
            {
                var result = DateTimeParser.ParseString(input);
                RecordTest(description, true); // Just verify it parses without error
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
                var result = DateTimeParser.ParseString(input);
                RecordTest(description, false); // Should have thrown
                _failureDetails.Add($"{description}: Should have thrown but returned {result}");
            }
            catch (ArgumentNullException)
            {
                RecordTest(description, true); // Expected for null input
            }
            catch (FormatException)
            {
                RecordTest(description, true); // Expected for invalid formats
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