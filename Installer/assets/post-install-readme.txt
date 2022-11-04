For production installations, the Ed-Fi Alliance recommends reconfiguring IIS 
web site bindings for improved security configuration, unless the IIS site will
be served to users / clients through a reverse proxy (e.g. HAProxy, NGiNX, etc).
When using a reverse proxy, re-configuring the security may still be a good
practice if the host is in a network segment that is accessible to 
non-administrative users.

By default, the Ed-Fi web site in IIS is set to run with HTTP on port 81 with
no host headers. The Center for Internet Security (CIS) recommends that 
production sites enable transport layer security (TLS) and configure host 
headers for all sites. For the best user / client experience, TLS should be 
configured using a commercially-signed certificate rather than a self-signed
certificate.

To change these settings:

1. Open the IIS Manager (inetmgr)
2. In the Connections pane on the left, expand "sites" and select "Ed-Fi"
3. In the Actions pane on the right, click on bindings
4. In the Site Bindings dialog box, click the Add button
5. In the Add Site Binding dialog box:
   5.1. Select type "https"
   5.2. Optionally change the IP address and/or Port number
   5.3. Enter the host name (example: ods.youragency.edu)
   5.4. Optionally require server name identification
   5.5. Select an appropriate SSL certificate
   5.6. Click the OK button to close the dialog box
6. Delete the default "http" type binding

For more information on Binding configuration in IIS, please visit
https://docs.microsoft.com/en-us/iis/configuration/system.applicationhost/sites/site/bindings/binding