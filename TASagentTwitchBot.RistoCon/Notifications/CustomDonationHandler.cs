using System.Web;

using TASagentTwitchBot.Core.Notifications;


namespace TASagentTwitchBot.RistoCon.Notifications;

public class CustomDonationHandler : Core.Donations.IDonationHandler
{
    private readonly Core.ICommunication communication;

    private readonly IActivityHandler activityHandler;
    private readonly IActivityDispatcher activityDispatcher;

    private readonly Core.Audio.ISoundEffectSystem soundEffectSystem;
    private readonly Core.TTS.ITTSRenderer ttsRenderer;
    private readonly Core.Donations.IDonationTracker donationTracker;

    private readonly INotificationImageHelper notificationImageHelper;

    public CustomDonationHandler(
        Core.ICommunication communication,
        IActivityHandler activityHandler,
        IActivityDispatcher activityDispatcher,
        Core.Audio.ISoundEffectSystem soundEffectSystem,
        Core.TTS.ITTSRenderer ttsRenderer,
        Core.Donations.IDonationTracker donationTracker,
        INotificationImageHelper notificationImageHelper)
    {
        this.communication = communication;
        this.activityHandler = activityHandler;
        this.activityDispatcher = activityDispatcher;
        this.soundEffectSystem = soundEffectSystem;
        this.ttsRenderer = ttsRenderer;
        this.donationTracker = donationTracker;
        this.notificationImageHelper = notificationImageHelper;
    }

    public async void HandleDonation(
        string name,
        double amount,
        string message,
        bool approved)
    {
        communication.NotifyEvent($"{name} Donation: {amount:c} - {message}");

        donationTracker.AddDirectDonations(amount);

        string chatResponse = await GetDonationChatResponse(name, amount);
        if (!string.IsNullOrWhiteSpace(chatResponse))
        {
            communication.SendPublicChatMessage(chatResponse);
        }

        activityDispatcher.QueueActivity(
            activity: new DonationActivityRequest(
                activityHandler: activityHandler,
                description: $"Donation: {name} for {amount:c}",
                requesterId: "",
                notificationMessage: await GetDonationNotificationRequest(name, amount, message),
                audioRequest: await GetDonationAudioRequest(name, amount, message),
                marqueeMessage: await GetDonationMarqueeMessage(name, message)),
            approved: approved);
    }

    private Task<string> GetDonationChatResponse(string name, double amount)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult($"Thanks to {name} for the donation of {amount:c}!");
        }
        else
        {
            return Task.FromResult($"Thanks for the donation Anonymous donation of {amount:c}!");
        }
    }


    private Task<NotificationMessage> GetDonationNotificationRequest(
        string name,
        double amount,
        string message)
    {
        return Task.FromResult<NotificationMessage>(new ImageNotificationMessage(
            image: notificationImageHelper.GetRandomDefaultImageURL(),
            duration: 10_000,
            message: GetDonationMessage(name, amount, message)));
    }

    protected virtual string GetDonationMessage(
        string name,
        double amount,
        string message)
    {
        return $"<span style=\"color: #0000FF\">{HttpUtility.HtmlEncode(name)}</span> has donated {amount:c}: {HttpUtility.HtmlEncode(message)}";
    }


    protected virtual async Task<Core.Audio.AudioRequest?> GetDonationAudioRequest(
        string name,
        double amount,
        string message)
    {
        Core.Audio.AudioRequest? soundEffectRequest = null;
        Core.Audio.AudioRequest? ttsAnnouncement;
        Core.Audio.AudioRequest? ttsRequest = null;

        if (soundEffectSystem.HasSoundEffects())
        {
            Core.Audio.SoundEffect? subSoundEffect = soundEffectSystem.GetSoundEffectByName("Donation Effect");
            if (subSoundEffect is null)
            {
                communication.SendWarningMessage($"Expected Sub SoundEffect not found.  Defaulting to first sound effect.");
                subSoundEffect = soundEffectSystem.GetAnySoundEffect();
            }

            if (subSoundEffect is not null)
            {
                soundEffectRequest = new Core.Audio.SoundEffectRequest(subSoundEffect);
            }
        }

        ttsAnnouncement = await ttsRenderer.TTSRequest(
            authorizationLevel: Core.Commands.AuthorizationLevel.Admin,
            voicePreference: "Brian",
            pitchPreference: Core.TTS.TTSPitch.Medium,
            speedPreference: Core.TTS.TTSSpeed.Medium,
            effectsChain: new Core.Audio.Effects.NoEffect(),
            ttsText: $"{name} has donated {amount:c}.");

        if (!string.IsNullOrWhiteSpace(message))
        {
            ttsRequest = await ttsRenderer.TTSRequest(
                authorizationLevel: Core.Commands.AuthorizationLevel.Admin,
                voicePreference: "Brian",
                pitchPreference: Core.TTS.TTSPitch.Medium,
                speedPreference: Core.TTS.TTSSpeed.Medium,
                effectsChain: new Core.Audio.Effects.NoEffect(),
                ttsText: message);
        }

        return Core.Audio.AudioTools.JoinRequests(300, soundEffectRequest, ttsAnnouncement, ttsRequest);
    }

    protected virtual Task<string?> GetDonationMarqueeMessage(
        string name,
        string message)
    {
        return Task.FromResult(GetStandardMarqueeMessage(name, message));
    }

    private static string? GetStandardMarqueeMessage(string userName, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        return $"<h1><span style=\"color: #0000FF\" >{HttpUtility.HtmlEncode(userName)}</span>: {HttpUtility.HtmlEncode(message)}</h1>";
    }

    public class DonationActivityRequest : ActivityRequest, IAudioActivity, IOverlayActivity, IMarqueeMessageActivity
    {
        public NotificationMessage? NotificationMessage { get; }
        public Core.Audio.AudioRequest? AudioRequest { get; }
        public string? MarqueeMessage { get; }

        public DonationActivityRequest(
            IActivityHandler activityHandler,
            string description,
            string requesterId,
            NotificationMessage? notificationMessage = null,
            Core.Audio.AudioRequest? audioRequest = null,
            string? marqueeMessage = null)
            : base(activityHandler, description, requesterId)
        {
            NotificationMessage = notificationMessage;
            AudioRequest = audioRequest;
            MarqueeMessage = marqueeMessage;
        }
    }
}
