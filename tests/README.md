# Unit Tests

This directory contains comprehensive unit tests for the Storingsdienst application.

## Test Coverage

The project aims for **at least 80% code coverage** for all business logic. The tests cover:

### Services Tested

1. **HolidayService** (`HolidayServiceTests.cs`)
   - Fixed Dutch holidays (New Year, King's Day, Christmas, etc.)
   - Easter-related holidays calculated with Meeus algorithm
   - Caching behavior
   - Multiple years (2020-2026)
   - Edge cases and invalid dates

2. **MeetingAnalysisService** (`MeetingAnalysisServiceTests.cs`)
   - Single and multi-day events
   - Day categorization (weekday, weekend, holiday)
   - Event spanning multiple months
   - Duplicate events on same day
   - Sorting by date (descending)
   - All-day events

3. **ExcelExportService** (`ExcelExportServiceTests.cs`)
   - Excel file generation
   - Header formatting and styling
   - Data row population
   - Multiple months handling
   - Special characters in subjects
   - Empty data scenarios

4. **JsonImportService** (`JsonImportServiceTests.cs`)
   - Valid JSON parsing
   - Subject filtering (case-insensitive)
   - Date range filtering
   - Invalid JSON handling
   - Missing fields handling
   - Invalid date formats
   - Event overlap detection

## Running Tests Locally

### Prerequisites
- .NET 8.0 SDK installed
- PowerShell (for the convenience script)

### Quick Start

**Option 1: Using the PowerShell script (Recommended)**
```powershell
# From the solution root directory
.\run-tests-with-coverage.ps1

# With automatic report opening
.\run-tests-with-coverage.ps1 -OpenReport

# With custom coverage threshold
.\run-tests-with-coverage.ps1 -CoverageThreshold 85
```

**Option 2: Using dotnet CLI**
```bash
# Run tests only
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

**Option 3: Using Visual Studio**
1. Open `storingsdienst.sln`
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All Tests"
4. For coverage: Analyze → Code Coverage → All Tests

## Test Results in CI/CD

The GitHub Actions workflow automatically:
1. Runs all tests on every push to `main`
2. Generates code coverage reports
3. Enforces minimum 80% line coverage
4. Publishes test results as artifacts
5. Displays coverage summary in workflow output

### Viewing CI/CD Test Results

1. Go to the GitHub Actions tab
2. Select the latest workflow run
3. View the "Test Results and Code Coverage" section in the summary
4. Download artifacts:
   - `test-results` - TRX test result files
   - `code-coverage-report` - HTML coverage report

## Test Structure

```
tests/
├── Storingsdienst.Client.Tests/
│   ├── Services/
│   │   ├── HolidayServiceTests.cs         (11 test methods, 70+ test cases)
│   │   ├── MeetingAnalysisServiceTests.cs (17 test methods)
│   │   ├── ExcelExportServiceTests.cs     (15 test methods)
│   │   └── JsonImportServiceTests.cs      (20 test methods)
│   └── Storingsdienst.Client.Tests.csproj
└── README.md (this file)
```

## Test Technologies

- **xUnit** - Test framework
- **Moq** - Mocking framework for dependencies
- **FluentAssertions** - Fluent assertion library
- **Coverlet** - Code coverage tool
- **ReportGenerator** - Coverage report generation

## Code Coverage Configuration

The project uses Coverlet for code coverage with the following configuration:

- **Format**: Cobertura XML
- **Threshold**: 80% line coverage
- **Exclusions**:
  - `Program.cs` (DI setup)
  - `App.razor` (Blazor root)
  - `*.razor` files (Blazor components - UI logic)
- **Reports**: HTML, Markdown, Cobertura formats

## Writing New Tests

### Guidelines

1. **Follow AAA Pattern**: Arrange, Act, Assert
2. **One Assert Per Test**: Each test should verify one behavior
3. **Descriptive Names**: `MethodName_Scenario_ExpectedResult`
4. **Use Theory for Data-Driven Tests**: Use `[Theory]` and `[InlineData]` for similar tests with different inputs
5. **Mock Dependencies**: Use Moq to mock interfaces
6. **Use FluentAssertions**: For readable assertions

### Example Test

```csharp
[Fact]
public void MethodName_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var service = new MyService();
    var input = "test";

    // Act
    var result = service.DoSomething(input);

    // Assert
    result.Should().Be("expected");
}
```

### Example Theory Test

```csharp
[Theory]
[InlineData(2024, 1, 1)]   // New Year
[InlineData(2024, 12, 25)] // Christmas
public void IsDutchHoliday_FixedHolidays_ReturnsTrue(int year, int month, int day)
{
    // Arrange
    var date = new DateOnly(year, month, day);
    var sut = new HolidayService();

    // Act
    var result = sut.IsDutchHoliday(date);

    // Assert
    result.Should().BeTrue();
}
```

## Continuous Improvement

### Current Coverage (Target: 80%)

After running tests, the coverage report shows:
- **Line Coverage**: Check `./TestResults/CoverageReport/index.html`
- **Branch Coverage**: Available in the detailed report
- **Method Coverage**: Per-class breakdown available

### Adding Tests for New Features

When adding new features:
1. Write tests first (TDD approach recommended)
2. Ensure new code has at least 80% coverage
3. Run `.\run-tests-with-coverage.ps1` before committing
4. Fix any failing tests before pushing

## Troubleshooting

### Tests Not Running
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Restore packages: `dotnet restore`
- Clean build: `dotnet clean && dotnet build`

### Coverage Not Generated
- Ensure `coverlet.msbuild` package is installed
- Check that test project references the main project
- Verify no build errors in the test project

### Test Failures in CI but Not Locally
- Check DateTime/TimeZone differences
- Verify file paths use `Path.Combine` for cross-platform compatibility
- Ensure no hardcoded Windows-specific paths

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
