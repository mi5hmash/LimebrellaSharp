[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/LimebrellaSharp?label=version)](https://github.com/mi5hmash/LimebrellaSharp/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-Unlicense-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)
[![dotNET8](https://img.shields.io/badge/.NET%208-blueviolet)](https://visualstudio.microsoft.com/)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, then it's likely a scam.**

# 🍋☂️ LimebrellaSharp - What is it :interrobang:
This app can **unpack and pack archives encrypted with the 1st version of the "Lime" encryption**. It can also **resign files** with your own SteamID to **use any SaveData on your profile**.

## Supported titles*
| Game Title                | App ID  | Tested Version | Platform |
|---------------------------|---------|----------------|----------|
| Resident Evil 4 (Remake)  | 2050650 | 12302315       | PC       |

**the latest versions that I have tested and are supported*

# 🤯 Why was it created :interrobang:
I wanted to share my SaveData with a friend and find out why the game is taking so long to load my SaveData.

# :scream: Is it safe?
**No.** 
> [!CAUTION]
> You can corrupt your SaveData files and lose your progress or get banned from playing online if you unreasonably modify your save.

> [!IMPORTANT]
> Remember to always make a backup of the files that you want to edit, before modifying them.

> [!IMPORTANT]
> Disable the Steam Cloud before you replace any SaveData files.

You have been warned and now you can move on to the next chapter, fully aware of possible consequences.

# :scroll: How to use this tool

<img src="https://github.com/mi5hmash/LimebrellaSharp/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Setting the Input Directory
There are three ways to achieve this. The first one is to drop the SaveData file or folder it is in on a TextBox **(1)** or a button **(2)**. Alternatively, you may use button **(2)** to open a folder picker and navigate to the directory from it. Also, you can type in the directory path into the TextBox **(1)**.

> [!TIP]
> Starting from version 1.0.1, the program will extract the Steam32_ID from the "Input Folder Path" TextBox **(1)** (if it ends with *"<steam_id>\2050650\remote\win64_save"*) and will fill the TextBox **(3)** for you.

## About Steam32 ID
It is a 32-bit representation of your 64-bit SteamID.

##### Example:
| 64-bit SteamID    | 32-bit SteamID |
|-------------------|----------------|
| 76561197960265729 | 1              |

> [!NOTE]
> Steam32 ID is also known as AccountID or Friend Code. 

> [!TIP]
You can use the calculator on [steamdb.info](https://steamdb.info/calculator/) to find your SteamID.

## Resigning files
If you just want to resign your SaveData files to use them on another Steam Account then follow the steps below.

First, type in the Steam32_ID that was used to pack the SaveData file\s, into a TextBox **(3)**. It is a numeric ID of a Steam Account that belongs to the person that gave you their SaveData files.
Then, type in the Steam32_ID, that you want to pack the SaveData file\s with, into a TextBox **(4)**. This should be a numeric ID of a Steam Account on which you would like to load up the resigned SaveData files. Once you have both IDs typed in, just click the **"Resign All"** button **(9)** to resign all SaveData files.

> [!NOTE]
> After that, you can find the packed files inside the ".\\_OUTPUT" directory. All of them are signed with the Steam32_ID from the TextBox **(4)**.

## Enabling SuperUser Mode

> [!WARNING]
> This mode is for advanced users only.

If you really need it, you can enable SuperUser Mode by quickly clicking the version number label **(11)**, three times.

## Unpacking data

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled.  

If you want to unpack SaveData file\s to see its content, type in the Steam32_ID that was used to pack the SaveData file\s, into a TextBox **(3)** and press the **"Unpack All"** button **(6)**.

> [!NOTE]
> After that, you can find the unpacked files inside the ".\\_OUTPUT" directory.

## Packing data

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled.

If you want to pack the unpacked SaveData file\s, you need to type in the Steam32_ID that you want to pack the SaveData file\s with, into a TextBox **(4)** and press the **"Pack All"** button **(7)**.

> [!NOTE]
> After that, you can find the packed files inside the ".\\_OUTPUT" directory.

## Open the Output Directory
You can open the **".\\_OUTPUT"** directory in a new Explorer window by using the button **(10)**.

## Other buttons
Button **(5)** swaps TextBox **(3)** and TextBox **(4)** values.
Button **(8)** cancels the currently running operation.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issue (hope you won't) then please, feel free to report it [there](https://github.com/mi5hmash/LimebrellaSharp/issues).
  
> [!TIP]
> This application creates a log file that may be helpful in troubleshooting.  
It can be found in the same directory as the executable file.

**IF YOU DO NOT SEE SAVEDATA FILES THAT YOU HAVE RESIGNED, IN THE GAME MENU, THEN PLEASE, READ <a href="https://github.com/mi5hmash/LimebrellaSharp/tree/main/.resources/Save%20Files" target="_blank">THIS DOCUMENT</a>.**

# :star: Sources
* https://github.com/tremwil/DS3SaveUnpacker
