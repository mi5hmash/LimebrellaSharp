[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/license/mit)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/LimebrellaSharp?label=Version)](https://github.com/mi5hmash/LimebrellaSharp/releases/latest)
[![Visual Studio 2026](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202026-F0ECF8.svg?&logo=visual-studio-26)](https://visualstudio.microsoft.com/)
[![.NET10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# 🍋☂️ LimebrellaSharp - What is it :interrobang:
This app can **unpack and pack archives encrypted with the 1st version of "Lime" encryption**. It can also **re-sign these SaveData files** with your own SteamID to **use anyone’s SaveData on your Steam Account**.

## Supported titles*
|Game Title|App ID|Platform|
|---|---|---|
|Resident Evil 4 (Remake)|2050650|Steam|

# 🤯 Why was it created :interrobang:
I wanted to share my SaveData files with a friend and find out why the game takes so long to load my SaveData files.

# :scream: Is it safe?
The short answer is: **No.**

> [!CAUTION]
> If you unreasonably edit your SaveData files, you risk corrupting them or getting banned from playing online. In both cases, you will lose your progress.

> [!IMPORTANT]
> Always back up the files you intend to edit before editing them.

> [!IMPORTANT]
> Disable the Steam Cloud before you replace any SaveData files.

You have been warned, and now that you are completely aware of what might happen, you may proceed to the next chapter.

# :scroll: How to use this tool
## [GUI] - 🪟 Windows 
> [!IMPORTANT]
> If you’re working on Linux or macOS, skip this chapter and move on to the next one.

On Windows, you can use either the CLI or the GUI version, but in this chapter I’ll describe the latter.

<img src="https://github.com/mi5hmash/LimebrellaSharp/blob/main/.resources/images/MainWindow-v3.png" alt="MainWindow-v3"/>

### BASIC OPERATIONS

#### 1. Setting the Input Directory
You can set the input folder in whichever way feels most convenient:
- **Drag & drop:** Drop SaveData file - or the folder containing it - onto the TextBox **(1)**.
- **Pick a folder manually:** Click the button **(2)** to open a folder‑picker window and browse to the directory where SaveData file is.
- **Type it in:** If you already know the path, simply enter it directly into the TextBox **(1)**.

#### 2. Entering the User ID
In the case of Steam, your User ID is [your Friend Code](https://steamcommunity.com/friends/add).  

#### 3. Re-signing SaveData files
If you want to re‑sign your SaveData file/s so it works on another Steam account, type the User ID of the account that originally created that SaveData file/s into the TextBox **(3)**. Then enter the User ID of the account that should be allowed to use that SaveData file/s into the TextBox **(5)**. Finally, press the **"Re-sign All"** button **(9)**.

> [!NOTE]
> The re‑signed files will be placed in a newly created folder within the ***"LimebrellaSharp/_OUTPUT/"*** folder.

#### 4. Accessing modified files
Modified files are being placed in a newly created folder within the ***"LimebrellaSharp/_OUTPUT/"*** folder. You may open this directory in a new File Explorer window by pressing the button **(10)**.

> [!NOTE]
> After you locate the modified files, you can copy them into your save‑game folder.
> For Steam, the path looks like this:
> ***"<STEAM_INSTALL_DIRECTORY>/userdata/<USER_ID>/<APP_ID>/remote/win64_save/"***

> [!IMPORTANT]
> If the SaveData files you re‑signed do not appear in the game menu, please read <a href="https://github.com/mi5hmash/LimebrellaSharp/tree/main/.resources/Save%20Files" target="_blank">this document</a>.

### ADVANCED OPERATIONS

#### Enabling SuperUser Mode

> [!WARNING]
> This mode is for advanced users only.

If you really need it, you can enable SuperUser mode by triple-clicking the version number label **(11)**.

#### Unpacking SaveData files

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled. 

If you want to unpack SaveData file\s to read its content, type the User ID of the account that originally created that SaveData file/s into the TextBox **(3)**, and press the **"Unpack All"** button **(6)**.

#### Packing SaveData files

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled. 

If you want to pack SaveData file\s, enter the User ID of the account that should be allowed to use that SaveData file/s into the TextBox **(5)**, and press the **"Pack All"** button **(7)**.

### OTHER BUTTONS
Button **(8)** cancels the currently running operation.
Button **(4)** swaps the values in the **"Steam ID (INPUT)"** and **"Steam ID (OUTPUT)"** TextBoxes.

## [CLI] - 🪟 Windows | 🐧 Linux | 🍎 macOS

```plaintext
Usage: .\limebrella-sharp-cli.exe -m <mode> [options]

Modes:
  -m u  Unpack SaveData files
  -m p  Pack SaveData files
  -m r  Re-sign SaveData files

Options:
  -p <path>      Path to folder containing SaveData files
  -s <steam_id>  Steam ID (used in unpack/pack modes)
  -sI <old_id>   Original Steam ID (used in re-sign mode)
  -sO <new_id>   New Steam ID (used in re-sign mode)
  -v             Verbose output
  -h             Show this help message
```

### Examples
#### Unpack
```bash
.\limebrella-sharp-cli.exe -m u -p ".\InputDirectory" -s 1
```
#### Pack
```bash
.\limebrella-sharp-cli.exe -m p -p ".\InputDirectory" -s 2
```
#### Re-sign
```bash
.\limebrella-sharp-cli.exe -m r -p ".\InputDirectory" -sI 1 -sO 2
```

> [!NOTE]
> Modified files are being placed in a newly created folder within the ***"LimebrellaSharp/_OUTPUT/"*** folder.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issues (which I hope you won't) feel free to report them [there](https://github.com/mi5hmash/LimebrellaSharp/issues).

> [!TIP]
> This application creates a log file that may be helpful in troubleshooting.  
It can be found in the same directory as the executable file.  
Application stores up to two log files from the most recent sessions.
