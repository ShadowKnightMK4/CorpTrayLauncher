
---

## CorpTray

**CorpTray** is a lightweight system tray utility for Windows, designed with IT deployment and power users in mind. It dynamically builds a context menu in the system tray based on registry-defined groups that resolve to filesystem and/or registry shortcut data.

---

### 🔧 Key Features

* **Custom Tray Icon & Tooltip**
  Set via registry keys (user or policy). Tooltip and icon override follows policy > user.

* **Dynamic Context Menu**
  Right-clicking the tray icon displays a menu constructed from **Groups** defined in the registry. Policy entries override user-defined ones when conflicts arise.

* **Shortcut Execution**
  Shortcuts are parsed from `.lnk` aka normal Windows shortcuts scanned from a group's indicated folders. More than one folder can be used in a group.


* **Custom Icons**
  Icon is pulled from:

  1. `.lnk` metadata
  2. Shortcut target (via `ExtractIconEx`)
  3. If neither is usable, the icon is left blank for that menu item.

---



### 🗂 Registry Layout

CorpTray scans both **user** and **policy** registry paths. Policy always overrides user if keys are present in both.

| Location | What is it |
-----------|------------|
|`HKCU\Software\CorpTrayLauncher\Groups` | User-defined groups are here |
|`HKLM\SOFTWARE\Policies\CorpTrayLauncher\Groups`| Policy or Admin defined groups are here |
|`HKCU\Software\CorpTrayLauncher\` | User-defined settings here |
|`HKLM\SOFTWARE\Policies\CorpTrayLauncher\`| Policy or Admin defined settings here  |


#### Tray Settings 

The settings here are from ether the user defined setting location or the policy defined location.



| Setting Name        | Type     | Description                                                       |
| ------------------- | -------- | ----------------------------------------------------------------- |
| `Tooltip`           | `string` | Tooltip shown on the tray icon.                                   |
| `TrayIcon`          | `string` | Path to tray icon file.                                           |
| `EnableExitMenu`    | `bool`   | If true, adds an "Exit" option to the menu.                       |
| `EnableRefreshMenu` | `bool`   | If true, adds a "Refresh" option to re-scan and rebuild the menu. |


### 📁 Group Structure

Each **Group** is a registry key under either the **user** or **policy** branches. Identically named groups will resolve to the policy-defined version.

#### Group-Level Settings

| Key                  | Type     | Description                                                                                                                                                |
| -------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Disabled`           | `bool`   | If true, the group is recognized but not rendered in the tray.                                                                                             |
| `FolderBuildingPath` | `string` | A VS-style `folder1;folder2;folder3` string. These folders are scanned for `.lnk` shortcuts. Environment variables are supported BUT must be opted into via a Policy setting below. |
| `RegBuildingBranch`  | `string` | *Planned*: Will allow building menu items from registry-only shortcut definitions.                                                                         |
| `TrayIcon`           | `string` | Path to the icon file used for this group. If not there, no icon is used.                                                                 |
| `IsTopLevel`         | `bool`   | If true, group items are added to the root of the context menu. If false or missing, the group appears as a submenu.                |

---


#### Policy Only settings.

There is only one policy only setting so far:

HKLM\SOFTWARE\Policies\CorpTrayLauncher\ExpandEnviromentVariables. 

Defining this in the registroy to be a dword set to something other than 0 will enable expanding enviroment variables. If not defined or set to zero, no enviroment variables are expanded.

Currently those variables are expanded when when scanning group defined folders,



---

### 🧠 Design Notes

* `.lnk` files are parsed using `Microsoft Shell Controls And Automation` part of the .NET environment.
* Icons are pulled via the `ExtractIconEx` Win32 API.
* The system prioritizes keeping things simple, deployable and a minimialist GUI approach.
* Written in **C# targeting .NET Framework 4.7.2**.
* Developed and tested on **Windows 10**.
* if being debugged, the app will show a running log of most of its actions in a window.
---

### 📦 TODO

* Implement `RegBuildingBranch` parsing logic for fully registry-defined shortcuts.
* Optional: Logging or diagnostics for failed icon loads or invalid paths.
* Make an editor to configure this without raw registry editing.
---



