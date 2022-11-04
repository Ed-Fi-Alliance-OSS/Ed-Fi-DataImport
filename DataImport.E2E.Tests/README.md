# Data Import End To End Tests

## Install

To get started run `npm install`

## Run

Before running, you need to specify the URL, username and password in an .env file. File .env.example is a guide about how to set the variables.

To execute the tests, run `npm test`

## Debug

The preferred method for debug is the integrated playwright inspector.

```
$env:PWDEBUG=1
npm run test
```