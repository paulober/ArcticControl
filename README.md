# Arctic Control [under development, early beta]

# Description

Arctic Control is an alternative driver update/configuration and overclocking utility for Intel® Arc™ A-Series dGPUs. Important, this is an beta relase to test compatibility and only contains a very limited amount of useable features. This project is under development. Let me know with a rating if you like this project.

Currently it only contains two stable features:
- Driver download utility with easy access to Releasenotes
- Integrated WebView of Intel® Arc™ News website for quick access to Arc™ related updates and changes

Included experimental features:
- Performance tab: Does only include limited overclocking sliders but many more are planed or already in development.
- Games tab: For management of driver settings for diffrent games. Currently only lists installed steam games and has not functionality enabled in release build. Feature is currently under development.

This application does and will not include a "Studio" feature like Arc Control as there are way better programms already out there than I can ever develop, like the OBS-Project.

The application uses Microsoft AppCenter Analytics/Crashes, please read the related privacy policy on their official website (https://appcenter.ms/ or https://aka.ms/appcenterprivacy).

This application is a project of a Intel® Arc™ community member and NOT related to Intel® in any way.

If you have feedback let me now in following thread on the Intel Insiders Community discord: https://discord.com/channels/554824368740630529/1049459749160235058 or here on the repo the issues tab-

# Installation instructions
| All files contain "x64" or "arm64" in their name, always download the version of a certain file which contains your processor architecture.

- Head over to the releases tab on the right and download the file ending with .msix from the latest release.

### Requirements
- Microsoft [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

### First installation
- If this is the first time you install this application, also download the .cer and .cmd file.
- Open your downloads folder and right click the "installCertificate.cmd" and select "Run as administrator"
- Continue with steps in "Update" section

### Update
- If you already installed this application once, just duble click the .msix installer and click install/update.

### Finally
- You can now delete all downloaded files as they are no longer needed. You find the installed application in you start menu.

# Why I started this project

Since its launch, I have heard many criticisms about Intel Arc Control, such as: "Why can you only open it as an overlay?", "Why does it look so different?", "No apply button?", etc. I think you all know what I mean.
This got me thinking about how I can help without being an Intel employee.

So I decided to use my knowledge and experience in Windows desktop software development to code Arc Control in the style of WinUi 3.0 and integrate it as much as possible (for me) with Windows to ensure smooth integration into the OS design and API ecosystem.
With my future goal to create an alternative to Arc Control that provides an easy entry point for new users.

