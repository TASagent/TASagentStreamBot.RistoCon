# TASagent Stream Bot RistoCon

An implementation of my extensible, modular C# [stream bot development framework](https://github.com/TASagent/TASagentTwitchBotCore).

## How do I use this?

Follow the instructions in the [TASagentTwitchBotCore](https://github.com/TASagent/TASagentTwitchBotCore) project to get started.  More detailed instructions to come.

### Notifications

In OBS, add the following BrowserSources

`http://localhost:5000/BrowserSource/overlay.html` - Image notifications (example size: 800px wide x 600px tall)  
`http://localhost:5000/BrowserSource/timer.html` - Timer overlay (example size: 450px wide x 150px tall)  
`http://localhost:5000/BrowserSource/ttsmarquee.html` - Scrolling TTS Marquee (example size: 1920px wide x 60px tall)  
`http://localhost:5000/BrowserSource/emoteRain.html` - Raining Emote effect (example size: 1920px wide x 1080px tall)  