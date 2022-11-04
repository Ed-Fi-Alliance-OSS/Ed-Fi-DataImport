# First-Time Data Import Configuration

## Prerequisites

* The API base URL and version of your Ed-Fi ODS/API.
* An API key and secret with create permissions for entities into which the system will be importing.

## Configuration Steps

### Step 1. Create Data Import Administrative User

Provide an Email and Password to Log in. If there are no users in the system and the `Register` option can be seen, please click on the link to create an application user.

### Step 2. Setup ODS API Configuration

Once logged in, Data Import will open the `Admin>Configuration` screen. The API Server block contains essential information required for Data Import to function.

* Click on the `Ed-Fi API Connections` button to open the `API Connections` panel. Data Import works with multiple API connections and needs at least one to function.
* Click on the `Add API Connection` to add the first Ed-Fi ODS / API connection.
* Configure the following fields:
  * **Name for the API Connection**
  * **API Server Url**: The URL of the Ed-Fi API targeted for import.
  * **API Server Key**: An Ed-Fi ODS / API key with permissions to create data in targeted entities.
  * **API Server Secret**: An Ed-Fi ODS / API secret paired with key above.
* Click on the `Test Connection` button. If successful as indicated by the confirmation message, click `Save Changes` to add the connection.

> **Claim Set Requirements**: With the current release of Data Import, the claim set (key & secret) provided needs to enable authorized access read the Ed-Fi ODS / API /schools resource. If you use an "out of box" claimset, then you will need to use either the "Ed-Fi Sandbox" or "SIS Vendor" claim sets. The "Assessment Vendor" claim set does not enabled with this access. Alternatively, you can use the [Admin App's Claim Set Editor](https://techdocs.ed-fi.org/display/ADMIN/Claim+Set+Editor) to create a custom claim set.

### Step 3. User Management (Optional)

If you wish to register additional users within an instance of Data Import, enable `Allow User Registration` and run through `Step 1` to create additional users. **Ensure this option is disabled once all users are created.** If not adding new users beyond the initial administrative user in `Step 1`, it is recommended to not allow user registration (i.e., leave the checkbox unchecked) unless and until needed.

### Step 4. Update Configuration

With all prior steps completed, click on the `Update Configuration` button. Once configuration is verified, `Configuration was modified` will appear in the top-right of the screen. From here, it is recommended to view [Quick Start](../README.md#quick-start) for additional information on how to use Data Import.
