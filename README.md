# Arctic Control

# Description

Arctic Control is an alternative driver update/configuration and overclocking utility for Intel® Arc™ A-Series dGPUs. Important, this is an beta relase to test compatibility and only contains a very limited amount of useable features. This project is under development. Let me know with a rating if you like this project.

Currently it only contains some features:
- Driver download utility with easy access to release-notes
- Integrated WebView of Intel® Arc™ News website for quick access to Arc™ related updates and changes
- Performance tab: Does only include limited overclocking sliders but many more are planed or already in development.
- Games tab: For management of driver settings for different games. Currently only lists installed steam and epic games games. Feature is currently under development.

This application does and will not include a "Studio" feature like Arc Control as there are way better programms already out there than I can ever develop, like the OBS-Project.

The application uses Microsoft AppCenter Analytics/Crashes, please read the related privacy policy on their official website (https://appcenter.ms/ or https://aka.ms/appcenterprivacy).

This application is a project of a Intel® Arc™ community member and NOT related to Intel® in any way.

If you have feedback let me now in following thread on the Intel Insiders Community discord: https://discord.com/channels/554824368740630529/1049459749160235058 or here on the repo the issues tab.

# Installation instructions

- Head over to https://arcticcontrol.paulober.dev for the latest download

### Requirements
- Microsoft [.NET 7 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

### First installation
- If this is the first time you install this application, head over to the ~~releases tab on the right and download the files ending with .cer and .cmd from the latest release~~ project website and download the certificate bundle.
- Open your downloads folder unpack the ZIP file and right click the "installCertificate.cmd" and select "Run as administrator" (this will install my development certificate into your local "Trusted Persons/Publisher" certificate store to allow installation of the .msix app package, the new standard for Windows installers also used by the Microsoft Store)
- Continue with steps in the next section.

### Package installation (Please use the website as appinstaller does currently not work for single project packaged Windows App SDK apps)

| Deprecated, please use the project website for downloads
- If you already installed this application once, just open one of the following appinstaller links in your browser or download the .appinstaller file manually from the releases tab: 

| -- OS --       | Installer link (important: copy the hole text starting with ms-appinstaller) |
|:--------------:|----------------|
| Windows 11 x64 | ms-appinstaller:?source=https://github.com/paulober/ArcticControl/releases/latest/download/ArcticControl_x64.appinstaller |
| Windows 11 arm64 | ms-appinstaller:?source=https://github.com/paulober/ArcticControl/releases/latest/download/ArcticControl_arm64.appinstaller |

### Finally
- You can now delete all downloaded files as they are no longer needed. You find the installed application in you start menu.

# Why I started this project

Since its launch, I have heard many criticisms about Intel Arc Control, such as: "Why can you only open it as an overlay?", "Why does it look so different?", "No apply button?", etc. I think you all know what I mean.
This got me thinking about how I can help without being an Intel employee.

So I decided to use my knowledge and experience in Windows desktop software development to code Arc Control in the style of WinUi 3.0 and integrate it as much as possible (for me) with Windows to ensure smooth integration into the OS design and API ecosystem.
With my future goal to create an alternative to Arc Control that provides an easy entry point for new users.

