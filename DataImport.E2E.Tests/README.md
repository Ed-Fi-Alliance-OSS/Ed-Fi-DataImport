# Ed-Fi DataImport End-to-End Tests

## Overview

This project contains comprehensive end-to-end (E2E) test automation for the Ed-Fi DataImport application using **Playwright** and **Cucumber** with TypeScript. The tests cover core functionalities including login, data import/export, API connections, and template sharing services.

> [!CAUTION]
> **Test Status**: These tests may require updates and investigation. Please verify functionality before usage and consider the current test maintenance status.

## Project Structure

```
DataImport.E2E.Tests/
├── features/                   # Cucumber feature files
│   ├── apiConnections.feature # API connection testing
│   ├── export.feature        # Data export functionality  
│   ├── import.feature        # Data import functionality
│   ├── login.feature         # Authentication testing
│   ├── support/              # Step definitions and utils
│   └── models/               # Test data models
├── screenshots/              # Test execution screenshots
├── traces/                   # Playwright traces for debugging
├── videos/                   # Recorded test execution videos
├── data/                     # Test data files
├── .env.example             # Environment variables template
└── package.json             # Project configuration
```

## Prerequisites

### System Requirements
- **Node.js** (version 18+)
- **npm** (latest version)
- **PowerShell** (for Windows debug commands)

### Ed-Fi Environment Setup
Before running tests, ensure you have:

1. **DataImport Application** running locally or accessible via URL
2. **Ed-Fi ODS/API** instance configured and accessible  
3. **Admin App** access for creating API credentials
4. **Valid user account** in the DataImport application

## Installation

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Install Playwright browsers**:
   ```bash
   npx playwright install
   ```

## Configuration

### Environment Variables

1. **Copy environment template**:
   ```bash
   cp .env.example .env
   ```

2. **Configure the `.env` file with your environment details**:

   ```env
   # DataImport Application
   URL="https://localhost:56323/"                 # DataImport application URL
   email="user@example.com"                       # Valid user email for login
   password="your-password"                       # User password
   
   # Ed-Fi API Configuration  
   API_URL="https://localhost:56323/v5.4/data/v3" # Ed-Fi ODS API endpoint
   API_Version="5.4"                              # Ed-Fi API version (e.g., 6.1, 5.4)
   key="YOUR_API_KEY"                             # API key from Admin App
   secret="YOUR_API_SECRET"                       # API secret from Admin App
   
   # Test Execution Settings
   HEADLESS=true                                # Run in headless mode
   TRACE=true                                   # Enable Playwright traces  
   RECORD=true                                  # Record test videos
   ```

### API Credentials Setup

1. **Access Ed-Fi Admin App**
2. **Create a new application** 
3. **Generate API key and secret**
4. **Update `.env` file** with the generated credentials

## Running Tests

### Execute All Tests
```bash
npm test
```

### Run Specific Feature Tests
```bash
# Login functionality
npm run test-login

# API connections  
npm run test-api

# Data import features
npm run test-import  

# Data export features
npm run test-export

# Template sharing service
npm run test-tss

# Work-in-progress tests (tagged with @WIP)
npm run test-wip

# Sanity tests (tagged with @Sanity)  
npm run sanity-test
```

### Run Individual Feature Files
```bash
# Specific feature file
npx cucumber-js features/login.feature

# With fail-fast option
npx cucumber-js features/import.feature --fail-fast
```

### Generate Test Reports
```bash
# Generate JSON report
npm run report

# Publish report to Cucumber Reports
npm run publish
```

## Development & Debugging

### Debug Mode
The recommended debugging approach uses Playwright's integrated inspector:

**PowerShell (Windows)**:
```powershell
$env:PWDEBUG=1
npm test
```

**Bash (macOS/Linux)**:
```bash
PWDEBUG=1 npm test
```

### Code Quality
```bash
# Run ESLint with auto-fix
npm run lint
```

### Debugging Individual Features
```powershell
# Debug specific feature
$env:PWDEBUG=1  
npx cucumber-js features/login.feature
```

### Visual Debugging Tools
- **Screenshots**: Automatically captured in `screenshots/` folder
- **Videos**: Test recordings saved in `videos/` folder  
- **Traces**: Playwright traces in `traces/` folder
- **Playwright Inspector**: Interactive debugging tool

## Test Data Management

### Test Data Location
- **Static test data**: Located in `data/` folder
- **Dynamic test data**: Generated during test execution
- **Screenshots**: Captured automatically on failures

### Environment-Specific Data
Modify test data based on your environment in the respective feature files or step definitions.

## Troubleshooting

### Common Issues

**Authentication Failures**:
- Verify user credentials in `.env` file
- Ensure user account exists and is active
- Check DataImport application accessibility

**API Connection Errors**:  
- Validate API_URL format and accessibility
- Verify API credentials (key/secret) are correct
- Confirm API_Version matches your Ed-Fi ODS version

**Test Environment Issues**:
- Ensure DataImport application is running
- Check network connectivity to Ed-Fi ODS/API
- Verify SSL/TLS certificate configuration if using HTTPS

**Playwright Issues**:
```bash
# Reinstall Playwright browsers
npx playwright install --force

# Clear Playwright cache
npx playwright uninstall
npx playwright install
```

### Debug Information
When reporting issues, include:
- Environment configuration (without sensitive data)
- Test execution logs
- Screenshots from failed tests
- Playwright traces (if available)

## Contributing

### Adding New Tests
1. Create feature files in `features/` directory
2. Implement step definitions in `features/support/`
3. Add appropriate test data to `data/` directory
4. Update this documentation as needed

### Code Standards
- Follow existing TypeScript patterns
- Use ESLint and Prettier for code formatting
- Write descriptive Cucumber scenarios
- Include proper error handling

## Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Cucumber.js Documentation](https://cucumber.io/docs/cucumber/)  
- [Ed-Fi API Documentation](https://techdocs.ed-fi.org/)
- [DataImport User Guide](../docs/)
