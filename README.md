# Resilient Form Test Framework

## Overview
This project implements a robust Selenium test framework for testing web forms, with special handling for Shadow DOM elements. The framework uses the Page Object Model pattern to provide a clean separation between test logic and page interactions.

## Key Features
- **Regular DOM Testing**: Standard form element interactions using Selenium WebDriver
- **Shadow DOM Support**: Advanced JavaScript-based interactions for Shadow DOM elements
- **Nested Shadow DOM Support**: Handling of complex nested Shadow DOM structures
- **Screenshot Capabilities**: Automatic screenshots for visual verification
- **Detailed Logging**: Comprehensive logging for debugging and verification

## Project Structure
- **FormPage.cs**: Page Object Model implementation with methods for interacting with form elements
- **FormTests.cs**: Test cases for regular DOM, Shadow DOM, and nested Shadow DOM interactions
- **ResilientFormTest.csproj**: Project configuration file

## Test Verification

### Screenshots
All tests generate screenshots that are saved to the `bin/Debug/net8.0/Screenshots` directory. These screenshots provide visual verification of the test execution and results.

Screenshots are taken at the following points:
- Before interaction with form elements
- After interaction with form elements
- On test failure (if applicable)

### Console Output
Detailed logs are output to the console during test execution, providing information about:
- Element discovery
- Interaction attempts
- Success/failure of operations
- Page structure details

## Running the Tests
To run the tests, use the following command from the project directory:

```
dotnet test
```

## Proof of Work

### Test Results
The framework successfully handles both regular DOM and Shadow DOM elements:

```
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 32 s - ResilientFormTest.dll (net8.0)
```

### Shadow DOM Handling
The framework implements multiple strategies for interacting with Shadow DOM elements:

1. **Direct JavaScript Access**: Using shadowRoot to access elements within Shadow DOM
2. **DOM Traversal**: Recursively searching through all shadow roots in the document
3. **Fallback Mechanisms**: Multiple approaches to ensure reliable interaction

### Key Achievements
- Successfully interacts with regular DOM elements
- Sets values in Shadow DOM input fields (demonstrated with 'Utkarsh Parihar')
- Handles radio button selection in Shadow DOM
- Manages dropdown selection in Shadow DOM
- Provides detailed logging and screenshots for verification

## Sources and References

### Technical References
- [Selenium WebDriver Documentation](https://www.selenium.dev/documentation/webdriver/)
- [Shadow DOM Specification](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_shadow_DOM)
- [NUnit Testing Framework](https://nunit.org/documentation/)

### Test Target
- The framework tests against the form at: https://app.cloudqa.io/home/AutomationPracticeForm

## Author
Utkarsh Parihar
