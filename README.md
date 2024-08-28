[![License: Unlicense](https://img.shields.io/badge/License-Unlicense-blueviolet.svg)](https://opensource.org/licenses/Unlicense)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/LimebrellaSharp?label=Version)](https://github.com/mi5hmash/LimebrellaSharp/releases/latest)
[![Visual Studio 2022](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202022-5C2D91.svg?&logo=visual-studio&logoColor=white)](https://visualstudio.microsoft.com/)
[![.NET8](https://img.shields.io/badge/.NET%208-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# 🍋☂️ LimebrellaSharp - What is it :interrobang:
This app can **unpack and pack archives encrypted with the 1st version of the "Lime" encryption**. It can also **resign files** with your own SteamID so you can **load them on your Steam Account**.

## Supported titles*
| Game Title                | App ID  | Tested Version | Platform |
|---------------------------|---------|----------------|----------|
| Resident Evil 4 (Remake)  | 2050650 | 12302315       | PC       |

**the most recent versions that I have tested and are supported*

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

<img src="https://github.com/mi5hmash/LimebrellaSharp/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Setting the Input Directory
There are three ways to achieve this. The first one is to drop the SaveData file or a folder that contains it, onto a TextBox **(1)** or a button **(2)**. Alternatively, you may use a button **(2)** to open a folder-picker window and navigate to the directory with it. You can also type the path to the folder in the **(1)** TextBox.

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
If you want to resign your SaveData files so you can use them on another Steam Account, follow the steps below.

First, enter the Steam32_ID that was used to pack the SaveData file\s, into a TextBox **(3)**. It is a numeric Steam Account ID that belongs to the individual who handed you their SaveData files.
Then, type in the Steam32_ID, that you want to pack the SaveData file\s with, into a TextBox **(4)**. This should be a numeric ID of a Steam Account on which you would like to load up the resigned SaveData files. Once you have entered both IDs, click the **"Resign All"** button **(9)** to resign all SaveData files.

> [!NOTE]
> You can find the packed files inside the ".\\_OUTPUT" directory. All of them are signed with the Steam32_ID from the TextBox **(4)**.

## Enabling SuperUser Mode

> [!WARNING]
> This mode is for advanced users only.

If you really need it, you can enable SuperUser mode by triple-clicking the version number label **(11)**.

## Unpacking data

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled.  

If you want to unpack SaveData file\s to see its content, type in the Steam32_ID that was used to pack the SaveData file\s, into a TextBox **(3)** and press the **"Unpack All"** button **(6)**.

> [!NOTE]
> You can find the unpacked files inside the ".\\_OUTPUT" directory.

## Packing data

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled.

If you want to pack the unpacked SaveData file\s, you need to type in the Steam32_ID that you want to pack the SaveData file\s with, into a TextBox **(4)** and press the **"Pack All"** button **(7)**.

> [!NOTE]
> You can find the packed files inside the ".\\_OUTPUT" directory.

## Open the Output Directory
You can open the **".\\_OUTPUT"** directory in a new Explorer window by using the button **(10)**.

## Other buttons
Button **(5)** swaps TextBox **(3)** and TextBox **(4)** values.
Button **(8)** cancels the currently running operation.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issues (which I hope you won't) feel free to report them [there](https://github.com/mi5hmash/LimebrellaSharp/issues).
  
> [!TIP]
> This application creates a log file that may be helpful in troubleshooting.  
It can be found in the same directory as the executable file.

**IF YOU DO NOT SEE SAVEDATA FILES THAT YOU HAVE RESIGNED, IN THE GAME MENU, THEN PLEASE, READ <a href="https://github.com/mi5hmash/LimebrellaSharp/tree/main/.resources/Save%20Files" target="_blank">THIS DOCUMENT</a>.**

# :star: Sources
* https://github.com/tremwil/DS3SaveUnpacker
