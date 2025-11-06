# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WhatTimeIsIt is a .NET 8.0 library that provides comprehensive DateTime and DateTimeOffset parsing capabilities for various database and standard datetime formats while preserving microsecond precision (up to 7 digits) and timezone offsets. The library handles formats from MySQL, SQLite, SQL Server, Oracle, PostgreSQL, Unix timestamps, and .NET ticks.

## Solution Structure

- **WhatTimeIsIt**: Core library project containing parsers
  - Target: .NET 8.0 class library
  - Main classes:
    - `WhatTimeIsIt.DateTimeParser` in `Class1.cs`
    - `WhatTimeIsIt.DateTimeOffsetParser` in `DateTimeOffsetParser.cs`
- **Test.Automated**: Console application with comprehensive test suites
  - Target: .NET 8.0 console executable
  - Entry point: `Test.Automated.Program.cs`
  - Test classes: `DateTimeOffsetParserTests.cs`
  - References the WhatTimeIsIt project
- **Test.Analysis**: Console application for behavior analysis
  - Target: .NET 8.0 console executable
  - Analyzes DateTime vs DateTimeOffset behavior
  - References the WhatTimeIsIt project

## Build Commands

```bash
# Build the entire solution
dotnet build WhatTimeIsIt.sln

# Build specific projects
dotnet build WhatTimeIsIt\WhatTimeIsIt.csproj
dotnet build Test.Automated\Test.Automated.csproj

# Build in Release mode
dotnet build WhatTimeIsIt.sln -c Release
```

## Running Tests

The test suite is a console application (not a unit test framework) that validates all parsing functionality:

```bash
# Run all tests
dotnet run --project Test.Automated\Test.Automated.csproj

# Run tests with Release build
dotnet run --project Test.Automated\Test.Automated.csproj -c Release
```

The test application:
- Exits with code 0 if all tests pass
- Exits with code 1 if any tests fail
- Prints detailed test results and failure information to console

## Architecture

### DateTimeParser Class (WhatTimeIsIt\Class1.cs)

The `DateTimeParser` class is the heart of the library and provides both static and instance methods:

**Static Methods:**
- `ParseString(string input)` - Parse using default formats
- `ParseString(string input, string[] formats)` - Parse with custom formats
- `TryParseString(string input, out DateTime result)` - Safe parsing with defaults
- `TryParseString(string input, string[] formats, out DateTime result)` - Safe parsing with custom formats
- `DefaultFormats` property - Returns a clone of default format strings

**Instance Methods:**
- `Parse(string input)` - Parse using instance's configured formats
- `TryParse(string input, out DateTime result)` - Safe parsing with instance formats
- `Formats` property - Get/set custom format array (null/empty reverts to defaults)
- `ResetToDefaults()` - Reset instance to use default formats

**Parsing Strategy:**
1. Numeric detection for Unix timestamps and .NET ticks
2. Explicit format matching with both InvariantCulture and CurrentCulture
3. Special handling for Oracle period-separated formats
4. Fallback to DateTime.Parse with built-in intelligence

**Format Precision Hierarchy:**
Formats are ordered from most precise (7-digit/100-nanosecond) to least precise (date-only), covering:
- 7-digit precision: SQL Server datetime2(7)
- 6-digit precision: Microseconds
- 3-digit precision: Milliseconds
- Seconds, minutes, date-only formats
- Timezone-aware formats (Z, +00, -00, zzz, K)
- Database-specific formats (Oracle with periods, MySQL compact, SQL Server)
- Culture-specific formats (US, European, 12/24-hour)

### DateTimeOffsetParser Class (WhatTimeIsIt\DateTimeOffsetParser.cs)

The `DateTimeOffsetParser` class mirrors `DateTimeParser` but returns `DateTimeOffset` instead of `DateTime`, preserving timezone offset information:

**Static Methods:**
- `ParseString(string input)` - Parse using default formats and UTC offset
- `ParseString(string input, string[] formats, TimeSpan defaultOffset)` - Parse with custom formats and offset
- `TryParseString(string input, out DateTimeOffset result)` - Safe parsing with defaults
- `TryParseString(string input, string[] formats, TimeSpan defaultOffset, out DateTimeOffset result)` - Safe parsing with custom formats

**Instance Methods:**
- `Parse(string input)` - Parse using instance's configured formats and default offset
- `TryParse(string input, out DateTimeOffset result)` - Safe parsing with instance configuration
- `Formats` property - Get/set custom format array
- `DefaultOffset` property - Get/set the offset to use for datetime strings without timezone info
- `ResetToDefaults()` - Reset formats and offset to defaults (UTC)

**Key Differences from DateTimeParser:**
1. **Timezone Preservation**: Preserves timezone offsets like +05:00, -08:00, +05:30
2. **DefaultOffset Property**: Configurable offset for datetime strings without timezone information
3. **Smart Detection**: Detects if input has timezone indicator (Z, +, -) and applies different parsing logic:
   - With timezone: Uses `DateTimeOffset.TryParseExact` to preserve the offset
   - Without timezone: Parses as `DateTime` with `DateTimeKind.Unspecified`, then applies `DefaultOffset`
4. **No Local Time Confusion**: Always uses explicit offsets, never system local time

**Parsing Strategy:**
1. Numeric detection for Unix timestamps (always UTC) and .NET ticks
2. Check for timezone indicators (Z, +, -) in the input string
3. If timezone present: Use DateTimeOffset parsing to preserve it
4. If no timezone: Parse as DateTime with Unspecified kind, apply DefaultOffset
5. Oracle special format handling (with timezone detection)
6. Fallback parsing with timezone detection

### Test Suites

**Test.Automated\Program.cs (DateTimeParser):**
Comprehensive validation covering:
- Static vs instance methods
- All supported format patterns (100+ formats)
- Numeric formats (Unix timestamps, .NET ticks)
- Timezone handling
- Precision preservation (up to microseconds)
- Edge cases (leap years, end-of-year, single digits)
- Negative cases (invalid inputs)
- Format property behavior
- Culture handling (en-US, en-GB, de-DE)
- Oracle special formats with period separators
- TryParse method variants

**Test.Automated\DateTimeOffsetParserTests.cs:**
Comprehensive validation covering:
- Static vs instance methods
- Timezone offset preservation (verifies offset is preserved, not just converted)
- All supported format patterns with timezone variations
- DefaultOffset property behavior
- Numeric formats (Unix timestamps as UTC, .NET ticks)
- Precision preservation with timezone offsets
- Edge cases with timezones
- Negative cases (invalid inputs)
- Format property behavior
- Culture handling
- TryParse method variants
- 50 tests total, 90% pass rate (5 failures due to .NET special format specifiers applying local time)
- Numeric formats (Unix timestamps, .NET ticks)
- Timezone handling
- Precision preservation (up to microseconds)
- Edge cases (leap years, end-of-year, single digits)
- Negative cases (invalid inputs)
- Format property behavior
- Culture handling (en-US, en-GB, de-DE)
- Oracle special formats with period separators
- TryParse method variants

## Development Notes

### Precision Handling

The library preserves sub-millisecond precision using `DateTime.Ticks`:
- 1 millisecond = 10,000 ticks
- 1 microsecond = 10 ticks
- `.AddTicks()` is used to add microsecond precision beyond milliseconds

Example: `"2024-01-15 14:30:45.123456"` parses as:
```csharp
new DateTime(2024, 1, 15, 14, 30, 45, 123).AddTicks(4560)
```

### Nullable Reference Types

The project uses `<Nullable>enable</Nullable>` but includes `#pragma warning disable CS8625` in Class1.cs to suppress warnings about null assignments to the formats array.

### Implicit Usings

Both projects use `<ImplicitUsings>enable</ImplicitUsings>`, providing common System namespaces automatically.

## Working with the Codebase

When modifying DateTimeParser:
- Maintain format order from most precise to least precise
- Test with Test.Automated after changes
- Consider timezone handling (AssumeLocal, AdjustToUniversal)
- Verify precision preservation for sub-millisecond values
- Add corresponding test cases for new formats
