The sample CSV files includes example Student Assessments to be posted to the ODS.

The sample JSON files define the Bootstrap and Data Map necessary to map that CSV to the ODS.

The "Fixed Width" and "Tab Delimited" variants use the preprocessors concept in order to deal with the format conversion line by line.

The "Metadata and ODS API Calls" variant works with a CSV file, but also uses a preprocess to logs additional metadata including the live results of an ODS API call.

These samples assume the "Grand Bend" sample data set is present, which is the case in the demo instances at https://api.ed-fi.org.

For instance, configure your Data Import with a URL like https://api.ed-fi.org/v3.4.0/api/data/v3 or https://api.ed-fi.org/v5.2/api/data/v3

Instances hosted at https://api.ed-fi.org can be configured with the following key and secret:
Key: RvcohKz9zHI4
Secret: E1iEFusaNf81xzCxwHfbolkC