# TASagent Stream Bot RistoCon

An implementation of my extensible, modular C# [stream bot development framework](https://github.com/TASagent/TASagentTwitchBotCore).

## How do I use this?

Follow the instructions in the [TASagentTwitchBotCore](https://github.com/TASagent/TASagentTwitchBotCore) project to get started.  More detailed instructions to come.

### Notifications

In OBS, add the following BrowserSources

`http://localhost:5000/BrowserSource/overlay.html` - Image notifications (example size: 800px wide x 600px tall)  
`http://localhost:5000/BrowserSource/donationThermometer.html` - Donation meter showing progress to fundraising goal (example size: 50px wide x 600px tall)  
`http://localhost:5000/BrowserSource/donations.html` - Cumulative text donation display (example size: 800px wide x 120px tall)  
