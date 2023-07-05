# Butterfly Soup Discord Bot
Turns discord conversations into dialogue from the game Butterfly Soup (and sequel).

All assets are taken from these games [which you can get here on itch.io](https://brianna-lei.itch.io/butterfly-soup)
## Usage
Simply type `!soup NUMBER` into chat and a conversation the specified number of messages back will be created.
## Building and Running
### Requirements
- [.NET SDK 7.0](https://dotnet.microsoft.com/en-us/download) installed
- [FFmpeg](http://ffmpeg.org/download.html) installed and in your PATH
- A bot account with the message content intent enabled
- The token for the bot account you wish to run this application out of is in your PATH with the key `BUTTERFLY_SOUP_TOKEN`
### Instructions
1. Clone this repository
2. `cd` into the cloned repository 
3. Run `dotnet publish`
4. The folder `/Path/To/Cloned/Repo/BSoupSpeechGenerator/bin/Debug/net7.0/[Your Platform]/publish/` will contain an executable called `BSoupSpeechGenerator` that you can now run. woo yeah.

The `Resources` folder must be in the same folder as the executable for the application to run correctly.