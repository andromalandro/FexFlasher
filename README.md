# FexFlasher

A Windows tool to flash `.fex` firmware files to SD cards. After been affected by the "Horixontal lines appearing on screen" issue I reached the muOS devs and they helped me out trying to fix it, this apparently stems for some devices having a panel that doesn’t like the timings used by muOS. I had to extract files from the sd card, change some values and then reflash the card, all on Linux, got me thinking that most people are probably using Windows and wanted to figure out a way to make the process easy. DISCLAIMER: this has only been tested on my 35XX H using Windows 11, it should work on the plus as far as I understand, the zip file has the necessary .fex file I extracted from my device. Changing screen timings can potentially DAMAGE your screen or BRICK your muOS installation, back up your device and use the tool at your own risk. My testing was, flashed muOS on an sd card, wait for the lines to show, apply the new flash with the tool and check to see if the issue was fixed, it fixed 2 sd cards I flashed.

## Installation
Download the latest release from the [Releases page](../../releases).

## Usage
1. Insert your SD card.
2. Run the `.exe` file.
3. Select the `.fex` file included in the "files" folder and flash.

---
## Special Thanks
Special thanks to the muOS devs (https://muos.dev/) on discord who helped me out and explained how things work on my 35XX H, also a shoutout to u/randomcoder_67 on reddit who has this amazing guide to do all the proces on linux https://www.reddit.com/user/randomcoder_67/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button.

## Support Me ❤️
If you find this project helpful, consider supporting me:

[![Ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/papagamer)
