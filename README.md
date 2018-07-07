# Find the path of your desktop background.

I have a gigantic folder of desktop backgrounds scraped from various online sources. I have them all in the same folder set to "shuffle".
Occasionally, I'll come across one that I don't like and want to remove it from my hard drive, but finding it manually among thousands of
images is too time-consuming.

This program examines the registry to find what image is currently displayed on the desktop. By default, it prints it to the console, but
it has a few flags to change its behavior:

* `/explorer` - Selects the file in a new Windows Explorer window
* `/install` - Installs a context menu extension that runs the program in `/explorer` mode when right clicking on the desktop.
* `/uninstall` - Removes the aforementioned shell extension.

This code is BSD 3-clause licensed.
