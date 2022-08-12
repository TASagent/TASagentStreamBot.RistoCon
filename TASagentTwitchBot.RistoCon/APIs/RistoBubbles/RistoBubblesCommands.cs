using TASagentTwitchBot.Core.IRC;
using TASagentTwitchBot.Core.Commands;

namespace TASagentTwitchBot.RistoCon.API.RistoBubbles;

public class RistoBubblesCommands : ICommandContainer
{
    private readonly Core.ICommunication communication;
    private readonly IRistoBubblesManager ristoBubblesManager;

    public RistoBubblesCommands(
        Core.ICommunication communication,
        IRistoBubblesManager ristoBubblesManager)
    {
        this.ristoBubblesManager = ristoBubblesManager;
        this.communication = communication;
    }

    IEnumerable<string> ICommandContainer.GetPublicCommands() => Array.Empty<string>();

    void ICommandContainer.RegisterCommands(ICommandRegistrar commandRegistrar)
    {
        commandRegistrar.RegisterScopedCommand("bubbles", "stop", StopBubbles);
        commandRegistrar.RegisterScopedCommand("bubble", "stop", StopBubbles);
        commandRegistrar.RegisterScopedCommand("stop", "bubbles", StopBubbles);
        commandRegistrar.RegisterScopedCommand("stop", "bubble", StopBubbles);

        commandRegistrar.RegisterScopedCommand("set", "bubbles", SetBubbles);
        commandRegistrar.RegisterScopedCommand("set", "bubble", SetBubbles);
        commandRegistrar.RegisterScopedCommand("bubbles", "set", SetBubbles);
        commandRegistrar.RegisterScopedCommand("bubble", "set", SetBubbles);

        commandRegistrar.RegisterScopedCommand("bubbles", "enable", (chatter, remainingCommand) => SetBubblesState(chatter, true));
        commandRegistrar.RegisterScopedCommand("bubble", "enable", (chatter, remainingCommand) => SetBubblesState(chatter, true));
        commandRegistrar.RegisterScopedCommand("enable", "bubbles", (chatter, remainingCommand) => SetBubblesState(chatter, true));
        commandRegistrar.RegisterScopedCommand("enable", "bubble", (chatter, remainingCommand) => SetBubblesState(chatter, true));

        commandRegistrar.RegisterScopedCommand("bubbles", "disable", (chatter, remainingCommand) => SetBubblesState(chatter, false));
        commandRegistrar.RegisterScopedCommand("bubble", "disable", (chatter, remainingCommand) => SetBubblesState(chatter, false));
        commandRegistrar.RegisterScopedCommand("disable", "bubbles", (chatter, remainingCommand) => SetBubblesState(chatter, false));
        commandRegistrar.RegisterScopedCommand("disable", "bubble", (chatter, remainingCommand) => SetBubblesState(chatter, false));

        commandRegistrar.RegisterScopedCommand("bubbles", "activate", ActivateBubbles);
        commandRegistrar.RegisterScopedCommand("bubble", "activate", ActivateBubbles);
        commandRegistrar.RegisterScopedCommand("activate", "bubbles", ActivateBubbles);
        commandRegistrar.RegisterScopedCommand("activate", "bubble", ActivateBubbles);
    }

    private Task StopBubbles(TwitchChatter chatter, string[] remainingCommand)
    {
        if (chatter.User.AuthorizationLevel < AuthorizationLevel.Moderator)
        {
            //Moderators and Admins can adjust users
            communication.SendPublicChatMessage($"I'm afraid I can't let you do that, @{chatter.User.TwitchUserName}.");
            return Task.CompletedTask;
        }

        ristoBubblesManager.Stop();
        return Task.CompletedTask;
    }

    private Task SetBubbles(TwitchChatter chatter, string[] remainingCommand)
    {
        if (chatter.User.AuthorizationLevel < AuthorizationLevel.Moderator)
        {
            //Moderators and Admins can adjust users
            communication.SendPublicChatMessage($"I'm afraid I can't let you do that, @{chatter.User.TwitchUserName}.");
            return Task.CompletedTask;
        }

        if (remainingCommand.Length < 1)
        {
            communication.SendPublicChatMessage($"@{chatter.User.TwitchUserName}, Error using !set bubbles command. Expected !set bubbles enabled or !set bubbles disabled.");
            return Task.CompletedTask;
        }

        bool enabled;

        switch (remainingCommand[0].ToLowerInvariant())
        {
            case "enabled":
            case "enable":
            case "on":
                enabled = true;
                break;

            case "disable":
            case "disabled":
            case "off":
                enabled = false;
                break;

            default:
                communication.SendPublicChatMessage($"@{chatter.User.TwitchUserName}, Error using !set bubbles command. Expected !set bubbles enabled or !set bubbles disabled.");
                return Task.CompletedTask;
        }

        return SetBubblesState(chatter, enabled);
    }

    private Task SetBubblesState(TwitchChatter chatter, bool enabled)
    {
        if (chatter.User.AuthorizationLevel < AuthorizationLevel.Moderator)
        {
            //Moderators and Admins can adjust users
            communication.SendPublicChatMessage($"I'm afraid I can't let you do that, @{chatter.User.TwitchUserName}.");
            return Task.CompletedTask;
        }

        if (ristoBubblesManager.GetBubblesEnabled() == enabled)
        {
            communication.SendPublicChatMessage($"@{chatter.User.TwitchUserName}, RistoBubbles are already {(enabled ? "Enabled" : "Disabled")}.");
            return Task.CompletedTask;
        }

        ristoBubblesManager.SetBubblesEnabled(enabled);
        return Task.CompletedTask;
    }

    private Task ActivateBubbles(TwitchChatter chatter, string[] remainingCommand)
    {
        if (chatter.User.AuthorizationLevel < AuthorizationLevel.Moderator)
        {
            //Moderators and Admins can adjust users
            communication.SendPublicChatMessage($"I'm afraid I can't let you do that, @{chatter.User.TwitchUserName}.");
            return Task.CompletedTask;
        }

        double duration = 10;

        if (remainingCommand.Length > 0)
        {
            if (!double.TryParse(remainingCommand[0], out duration) || duration <= 0)
            {
                communication.SendPublicChatMessage($"@{chatter.User.TwitchUserName}, Error using !activate bubbles command. Expected !activate bubbles or !activate bubbles 10.");
                return Task.CompletedTask;
            }
        }

        ristoBubblesManager.AddBubbleDuration(duration);
        return Task.CompletedTask;
    }
}
