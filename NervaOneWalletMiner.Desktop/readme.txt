NervaOne Desktop is a multi-coin, non-custodial GUI Wallet and CPU Miner.
It runs on your desktop device and allows you to control daemon/wallet using modern graphical user interface.


Windows
-------
Run NervaOne.exe


Linux
-----
To install NervaOne Desktop, run included install script. It should add NervaOne to your Applications.
Alternatively, you can go to: NervaOne/Contents in Terminal and run: ./NervaOne

If NervaOne does not appear in Applications after running the install script, or if a previously installed
version launches instead of the new one, first verify the installation was updated:

	cat ~/.local/share/applications/nervaone.desktop

The Exec= line should point to the current version's directory. If it does, refresh the desktop database:

	update-desktop-database ~/.local/share/applications/

Then log out and log back in, or restart your application launcher.


macOS
-----
Run NervaOne
Alternatively, you can go to NervaOne.app/Contents/MacOS and run ./NervaOne

Newer versions of macOS make running apps that are not published to Apple Store difficult.
If you get NervaOne is damaged and can't be opened error, try this:

1. Open New Terminal at Folder where NervaOne is located
2. Issue command:
	xattr -d com.apple.quarantine ./NervaOne.app
3. Try running NervaOne now