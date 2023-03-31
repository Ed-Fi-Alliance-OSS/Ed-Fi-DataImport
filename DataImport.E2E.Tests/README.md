# Data Import End To End Tests

## Install

To get started run `npm install`

## Run

Before running, you need to specify: 

- URL: From Data Import
- Username and Password: From current user created
- API_URL: Is the combination of URL + /WebApi/data/v3
- API_Version: Is the version from ODS installed (i.e. 6.1)
- key and secrect: From the application created on Admin App.

File .env.example is a guide about how to set the variables.

To execute all the tests, run `npm test`

## Debug

The preferred method for debug is the integrated playwright inspector.

```
$env:PWDEBUG=1
npm run test .\feature\{feature file name}
```
