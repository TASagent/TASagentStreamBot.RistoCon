using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TASagentTwitchBot.RistoCon.API.RistoBubbles;

[Core.AutoRegister]
public interface IRistoBubblesManager
{
    void Stop();
    bool GetBubblesEnabled();
    void SetBubblesEnabled(bool enabled);
    void AddBubbleDuration(double duration);
}

public class RistoBubblesManager :
    IRistoBubblesManager,
    Notifications.IDonationListener,
    Core.IStartupListener,
    Core.IShutdownListener,
    IDisposable
{
    private readonly RistoBubblesConfig ristoBubblesConfig;
    private readonly IRistoBubblesHelper ristoBubblesHelper;
    private readonly Core.ErrorHandler errorHandler;

    private readonly ChannelReader<double> bubbleChannelReader;
    private readonly ChannelWriter<double> bubbleChannelWriter;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private Task? shutdownTask = null;

    private bool bubblesActive = false;
    private bool shuttingDown = false;
    private bool disposedValue;

    public RistoBubblesManager(
        RistoBubblesConfig ristoBubblesConfig,
        IRistoBubblesHelper ristoBubblesHelper,
        Notifications.CustomDonationHandler donationHandler,
        Core.ErrorHandler errorHandler)
    {
        this.ristoBubblesHelper = ristoBubblesHelper;
        this.ristoBubblesConfig = ristoBubblesConfig;
        this.errorHandler = errorHandler;

        donationHandler.RegisterListener(this);

        Channel<double> bubbleTimeChannel = Channel.CreateUnbounded<double>();

        bubbleChannelReader = bubbleTimeChannel.Reader;
        bubbleChannelWriter = bubbleTimeChannel.Writer;

        Task.Run(LaunchBubbleDisabler);
    }

    public bool GetBubblesEnabled() => ristoBubblesConfig.Enabled;
    public void SetBubblesEnabled(bool enabled)
    {
        ristoBubblesConfig.Enabled = enabled;
        ristoBubblesConfig.Serialize();
    }

    public void AddBubbleDuration(double duration)
    {
        if (!ristoBubblesConfig.Enabled || shuttingDown || disposedValue)
        {
            //Disabled
            return;
        }

        if (duration >= 0)
        {
            bubbleChannelWriter.TryWrite(ristoBubblesConfig.BlastDuration);
        }
    }

    public async void Stop()
    {
        await ristoBubblesHelper.TrySetBubblesState(false, cancellationTokenSource.Token);
        bubblesActive = false;

        SetBubblesEnabled(false);
    }

    private async Task LaunchBubbleDisabler()
    {
        try
        {
            while (true)
            {
                if (!bubblesActive)
                {
                    //Bubbles Disabled - Wait until we need to turn them on
                    if (!await bubbleChannelReader.WaitToReadAsync(cancellationTokenSource.Token))
                    {
                        //Channel closed - return
                        return;
                    }

                    //Turn on bubbles
                    bubblesActive = true;
                    await ristoBubblesHelper.TrySetBubblesState(true, cancellationTokenSource.Token);

                    bubbleChannelReader.TryRead(out double duration);

                    //Wait
                    await Task.Delay(TimeSpan.FromSeconds(duration), cancellationTokenSource.Token);
                }
                else
                {
                    //Bubbles Live - See if it's time to turn them off
                    if (bubbleChannelReader.Count > 0)
                    {
                        //More pending time - Wait more
                        bubbleChannelReader.TryRead(out double duration);
                        await Task.Delay(TimeSpan.FromSeconds(duration), cancellationTokenSource.Token);
                    }
                    else
                    {
                        //Out of time - Turn off
                        await ristoBubblesHelper.TrySetBubblesState(false, cancellationTokenSource.Token);
                        bubblesActive = false;
                    }
                }
            }
        }
        catch (OperationCanceledException) { /* swallow */ }
        catch (Exception ex)
        {
            //Log Error
            errorHandler.LogSystemException(ex);
        }
    }

    private async Task Shutdown()
    {
        if (bubblesActive)
        {
            await ristoBubblesHelper.TrySetBubblesState(false);
            bubblesActive = false;
        }
    }

    #region IDonationListener

    public void NotifyDonation(string name, double amount, string message)
    {
        if (amount >= ristoBubblesConfig.DonationThreshold)
        {
            //Trigger
            AddBubbleDuration(ristoBubblesConfig.BlastDuration);
        }
    }

    #endregion IDonationListener
    #region IShutdownListener

    void Core.IShutdownListener.NotifyShuttingDown()
    {
        shuttingDown = true;

        bubbleChannelWriter.TryComplete();
        cancellationTokenSource.Cancel();

        shutdownTask = Shutdown();
    }

    #endregion IShutdownListener

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (shutdownTask is null)
                {
                    shutdownTask = Shutdown();
                }

                shutdownTask.Wait(2_000);

                bubbleChannelWriter.TryComplete();

                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
