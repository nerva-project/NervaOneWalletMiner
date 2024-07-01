# NervaOne Wallet and Miner
Copyright (c) 2024 The Nerva Project

NervaOne is a multi-coin, non-custodial GUI Wallet and CPU Miner. It runs on your desktop device and allows you to control daemon/wallet using modern graphical user interface.

NervaOne Desktop currently supports below cryptos:

- Nerva (XNV)
- Monero (XMR)
- Wownero (WOW)
- Dash (DASH)

## Build instructions
We encourage you to build the project yourself but if you do not feel comfortable doing that, see [Releases][releases-link]


To compile yourself, you'll need DOTNET SDK 8. You can install it for your operating system from Microsoft:

[DOTNET SDK 8][dotnet-sdk-8]


or by issuing command such as this:
```
 sudo apt-get install dotnet-sdk-8.0
```
 
## Building Using Command line
Go to directory where you want to create NervaOneWalletMiner folder and run these commands:

```
rm -rf ./NervaOneWalletMiner/
```
You do not need the "rm -rf"" the first time you build.

```
git clone https://github.com/nerva-project/NervaOneWalletMiner.git
```

```
cd NervaOneWalletMiner/NervaOneWalletMiner.Desktop
```

```
dotnet restore
```

```
dotnet run
```


Instead of dotnet run, you can build using command such as this:

```
dotnet publish .\NervaOneWalletMiner.Desktop.csproj -r osx-x64 -c Release -p:publishsinglefile=true
```

Compiled files will be inside:  ...\NervaOneWalletMiner\NervaOneWalletMiner.Desktop\bin\Release\net8.0\osx-x64\publish\

You'll need to replace "osx-x64" with your operating system. Other common values: 
win-x64, win-x86, linux-x64, linux-arm, osx-x64,osx-arm64

Here is full list: [.NET RID Catalog][rid-catalog]


## Running using Visual Studio 2022 Community (Windows)
You can Run/Debug the NervaOne using free [Visual Studio 2022 Community Edition][visual-studio]


Pick below workloads when installing VS:

.NET multi-platform App UI development

.NET desktop development


Clone this repository (https://github.com/nerva-project/NervaOneWalletMiner.git)

Unload .Android, .Browser and .iOS projects

Set NervaOneWalletMiner.Desktop project as startup project

Build > Build Solution

Debug > Start Debugging


## Running using Visual Studio Code (Windows/Linux/Mac)
Install [Visual Studio Code][visual-studio]

Go to Extensions and install: Avalonia for VSCode. More info:

https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.vscode-avalonia

In VSCode, go to Explorer and choose Clone this repository:

https://github.com/nerva-project/NervaOneWalletMiner.git

If you cannot clone because you do not have Git installed, see this:

https://code.visualstudio.com/docs/sourcecontrol/intro-to-git

Go to TERMINAL and cd into NervaOneWalletMiner.Desktop directory

```
dotnet build
```

```
dotnet run 
```


<!-- Reference links -->
[dotnet-sdk-8]: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
[releases-link]: https://github.com/nerva-project/NervaOneWalletMiner/releases
[rid-catalog]: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
[visual-studio]: https://visualstudio.microsoft.com/