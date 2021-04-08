using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

using TASagentTwitchBot.Core.Extensions;
using TASagentTwitchBot.Core.Web;

//Initialize DataManagement
BGC.IO.DataManagement.Initialize("TASagentBotRistoCon");

//
// Define and register services
//

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .UseUrls("http://0.0.0.0:5000");

IMvcBuilder mvcBuilder = builder.Services.GetMvcBuilder();

//Register Core Controllers (with potential exclusions) 
mvcBuilder.RegisterControllersWithoutFeatures(Array.Empty<string>());

//Add SignalR for Hubs
builder.Services.AddSignalR();

//Custom Database
builder.Services
    .AddTASDbContext<TASagentTwitchBot.RistoCon.Database.DatabaseContext>();

//Core Agnostic Systems
builder.Services
    .AddTASSingleton(TASagentTwitchBot.Core.Config.BotConfiguration.GetConfig())
    .AddTASSingleton<TASagentTwitchBot.Core.StandardConfigurator>()
    .AddTASSingleton<TASagentTwitchBot.Core.CommunicationHandler>()
    .AddTASSingleton<TASagentTwitchBot.Core.View.BasicView>()
    .AddTASSingleton<TASagentTwitchBot.Core.ErrorHandler>()
    .AddTASSingleton<TASagentTwitchBot.Core.ApplicationManagement>()
    .AddTASSingleton<TASagentTwitchBot.Core.Chat.ChatLogger>()
    .AddTASSingleton<TASagentTwitchBot.Core.MessageAccumulator>();

//Core Twitch Systems
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.API.Twitch.HelixHelper>()
    .AddTASSingleton<TASagentTwitchBot.Core.API.Twitch.BotTokenValidator>()
    .AddTASSingleton<TASagentTwitchBot.Core.API.Twitch.BroadcasterTokenValidator>()
    .AddTASSingleton<TASagentTwitchBot.Core.Database.UserHelper>()
    .AddTASSingleton<TASagentTwitchBot.Core.Bits.CheerHelper>()
    .AddTASSingleton<TASagentTwitchBot.Core.Bits.CheerDispatcher>()
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.TestCommandSystem>()
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.ShoutOutSystem>();

//Core Twitch Chat Systems
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.IRC.IrcClient>()
    .AddTASSingleton<TASagentTwitchBot.Core.IRC.IRCLogger>()
    .AddTASSingleton<TASagentTwitchBot.Core.IRC.NoticeHandler>()
    .AddTASSingleton<TASagentTwitchBot.Core.Chat.ChatMessageHandler>();

//Notification System
//Core Notifications
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.Notifications.NotificationImageHelper>()
    .AddTASSingleton<TASagentTwitchBot.Core.Notifications.NotificationServer>()
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.NotificationSystem>()
    .AddTASSingleton<TASagentTwitchBot.Core.Notifications.ActivityDispatcher>();

//Core Scripting
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.Scripting.ScriptManager>()
    .AddTASSingleton<TASagentTwitchBot.Core.Scripting.ScriptHelper>()
    .AddTASSingleton<TASagentTwitchBot.Core.Scripting.PersistentDataManager>();

builder.Services
    .AddTASSingleton(TASagentTwitchBot.Core.Commands.ScriptedCommands.ScriptedCommandsConfig.GetConfig())
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.ScriptedCommands>();

//Custom Notification
builder.Services
    .AddTASSingleton(TASagentTwitchBot.Core.Notifications.ScriptedActivityProvider.ScriptedNotificationConfig.GetConfig())
    .AddTASSingleton<TASagentTwitchBot.Core.Notifications.ScriptedActivityProvider>();

//Core Audio System
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.NAudioDeviceManager>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.NAudioPlayer>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.NAudioMicrophoneHandler>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.SoundEffectSystem>();


//Core Audio Effects System
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.AudioEffectSystem>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.ChorusEffectProvider>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.EchoEffectProvider>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.FrequencyModulationEffectProvider>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.FrequencyShiftEffectProvider>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.NoiseVocoderEffectProvider>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.PitchShiftEffectProvider>()
    .AddTASSingleton<TASagentTwitchBot.Core.Audio.Effects.ReverbEffectProvider>();

//Core PubSub System
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.PubSub.PubSubClient>()
    .AddTASSingleton<TASagentTwitchBot.Core.PubSub.RedemptionSystem>();

//Core Emote Effects System
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.API.BTTV.BTTVHelper>()
    .AddTASSingleton(TASagentTwitchBot.Core.EmoteEffects.EmoteEffectConfiguration.GetConfig())
    .AddTASSingleton<TASagentTwitchBot.Core.EmoteEffects.EmoteEffectListener>()
    .AddTASSingleton<TASagentTwitchBot.Core.EmoteEffects.EmoteEffectSystem>();

//Core WebServer Config (Shared by WebTTS and EventSub)
builder.Services
    .AddTASSingleton(TASagentTwitchBot.Core.Config.ServerConfig.GetConfig());

//Core Local TTS System
builder.Services
    .AddTASSingleton(TASagentTwitchBot.Core.TTS.TTSConfiguration.GetConfig())
    .AddTASSingleton<TASagentTwitchBot.Core.TTS.TTSRenderer>()
    .AddTASSingleton<TASagentTwitchBot.Core.TTS.TTSSystem>()
    .AddTASSingleton<TASagentTwitchBot.Core.TTS.TTSWebRequestHandler>()
    .AddTASSingleton<TASagentTwitchBot.Plugin.TTS.AmazonTTS.AmazonTTSWebSystem>()
    .AddTASSingleton<TASagentTwitchBot.Plugin.TTS.AzureTTS.AzureTTSWebSystem>()
    .AddTASSingleton<TASagentTwitchBot.Plugin.TTS.GoogleTTS.GoogleTTSWebSystem>();

//EventSub System
//Core EventSub
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.EventSub.EventSubHandler>()
    .AddTASSingleton<TASagentTwitchBot.Core.EventSub.FollowSubscriber>()
    .AddTASSingleton<TASagentTwitchBot.Core.EventSub.StreamChangeSubscriber>();


//Core Timer System
builder.Services.AddTASSingleton<TASagentTwitchBot.Core.Timer.TimerManager>();

//Command System
//Core Commands
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.CommandSystem>()
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.CustomCommands>()
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.SystemCommandSystem>()
    .AddTASSingleton<TASagentTwitchBot.Core.Commands.PermissionSystem>();

//Tiltify Integration
//  Core
builder.Services
    .AddTASSingleton<TASagentTwitchBot.Core.API.Tiltify.TiltifyCampaignMonitor>()
    .AddTASSingleton(TASagentTwitchBot.Core.API.Tiltify.TiltifyConfiguration.GetConfig())
    .AddTASSingleton<TASagentTwitchBot.Core.API.Tiltify.TiltifyHelper>()
    .AddTASSingleton<TASagentTwitchBot.Core.Donations.PersistentDonationTracker>()
    .AddTASSingleton<TASagentTwitchBot.Core.Donations.DonationCommands>();

//  Custom
builder.Services
    .AddTASSingleton<TASagentTwitchBot.RistoCon.Notifications.CustomDonationHandler>();

//Routing
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});


//
// Finished defining services
// Construct application
//

using WebApplication app = builder.Build();

//Handle forwarding properly
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.UseDefaultFiles();

//Config overrides
app.UseDocumentsOverrideContent();

//Custom Web Assets
app.UseStaticFiles();

//Core Web Assets
app.UseCoreLibraryContent("TASagentTwitchBot.Core");

//Controllerspy Assets
app.UseCoreLibraryContent("TASagentTwitchBot.Plugin.ControllerSpy");

//TTTAS Assets
app.UseCoreLibraryContent("TASagentTwitchBot.Plugin.TTTAS");

//Quote Assets
app.UseCoreLibraryContent("TASagentTwitchBot.Plugin.Quotes");

//Authentication Middleware
app.UseMiddleware<TASagentTwitchBot.Core.Web.Middleware.AuthCheckerMiddleware>();

//Map all Core Non-excluded controllers
app.MapControllers();

//Core Notification Hub
app.MapHub<TASagentTwitchBot.Core.Web.Hubs.OverlayHub>("/Hubs/Overlay");

//Core TTS Overlay Hub
app.MapHub<TASagentTwitchBot.Core.Web.Hubs.TTSMarqueeHub>("/Hubs/TTSMarquee");

//Core Control Page Hub
app.MapHub<TASagentTwitchBot.Core.Web.Hubs.MonitorHub>("/Hubs/Monitor");

//Core Timer Hub
app.MapHub<TASagentTwitchBot.Core.Web.Hubs.TimerHub>("/Hubs/Timer");

//Core Emote Effect Overlay Hub
app.MapHub<TASagentTwitchBot.Core.Web.Hubs.EmoteHub>("/Hubs/Emote");


await app.StartAsync();

//
// Update Database with new migrations
//

using (IServiceScope serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
    TASagentTwitchBot.RistoCon.Database.DatabaseContext context = serviceScope.ServiceProvider!.GetRequiredService<TASagentTwitchBot.RistoCon.Database.DatabaseContext>();
    context.Database.Migrate();
}

//
// Construct and run Configurator
//
TASagentTwitchBot.Core.ICommunication communication = app.Services.GetRequiredService<TASagentTwitchBot.Core.ICommunication>();
TASagentTwitchBot.Core.IConfigurator configurator = app.Services.GetRequiredService<TASagentTwitchBot.Core.IConfigurator>();

app.Services.GetRequiredService<TASagentTwitchBot.Core.View.IConsoleOutput>();

bool configurationSuccessful = await configurator.VerifyConfigured();

if (!configurationSuccessful)
{
    communication.SendErrorMessage($"Configuration unsuccessful. Aborting.");

    await app.StopAsync();
    await Task.Delay(15_000);
    return;
}

//
// Construct required components and run
//
communication.SendDebugMessage("*** Starting Up ***");

TASagentTwitchBot.Core.ErrorHandler errorHandler = app.Services.GetRequiredService<TASagentTwitchBot.Core.ErrorHandler>();
TASagentTwitchBot.Core.ApplicationManagement applicationManagement = app.Services.GetRequiredService<TASagentTwitchBot.Core.ApplicationManagement>();

foreach (TASagentTwitchBot.Core.IStartupListener startupListener in app.Services.GetServices<TASagentTwitchBot.Core.IStartupListener>())
{
    startupListener.NotifyStartup();
}

//
// Wait for signal to end application
//

try
{
    await applicationManagement.WaitForEndAsync();
}
catch (Exception ex)
{
    errorHandler.LogSystemException(ex);
}

//
// Stop webhost
//

await app.StopAsync();
