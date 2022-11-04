# Data Import Installer

`install.ps1` and `uninstall.ps1`, in combination with `install-config.json` and the build script's output `package` folder, can be used to create an IIS website and alter web configuration.

The `install` script will install the `Ed-Fi` website and matching app pool but *will not* uninstall them. This is because all Ed-Fi products use this website and we cannot safely delete the website without potentially removing other Ed-Fi web applications.