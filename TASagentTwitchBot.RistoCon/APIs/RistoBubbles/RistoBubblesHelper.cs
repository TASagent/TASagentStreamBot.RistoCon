using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;

using TASagentTwitchBot.Core;

namespace TASagentTwitchBot.RistoCon.API.RistoBubbles;

[AutoRegister]
public interface IRistoBubblesHelper
{
    Task<bool> TrySetBubblesState(bool active, CancellationToken cancellationToken = default);
    Task<bool> TryToggleBubblesState(CancellationToken cancellationToken = default);
    Task<bool?> GetBubblesState(CancellationToken cancellationToken = default);
}

public class RistoBubblesHelper : IRistoBubblesHelper
{
    private readonly Uri ristoBubblesURI;
    private readonly RistoBubblesConfig ristoBubblesConfig;

    public RistoBubblesHelper(RistoBubblesConfig ristoBubblesConfig)
    {
        this.ristoBubblesConfig = ristoBubblesConfig;
        ristoBubblesURI = new Uri($"http://{this.ristoBubblesConfig.RistoBubblesMachine}/switch/risto_bubbles");
    }

    public async Task<bool> TrySetBubblesState(bool active, CancellationToken cancellationToken = default)
    {
        RestClient restClient = new RestClient(ristoBubblesURI);
        RestRequest request = new RestRequest(active ? "turn_on" : "turn_off", Method.Post);

        RestResponse response = await restClient.ExecuteAsync(request, cancellationToken);

        return response.IsSuccessful;
    }

    public async Task<bool> TryToggleBubblesState(CancellationToken cancellationToken = default)
    {
        RestClient restClient = new RestClient(ristoBubblesURI);
        RestRequest request = new RestRequest("toggle", Method.Post);

        RestResponse response = await restClient.ExecuteAsync(request, cancellationToken);

        return response.IsSuccessful;
    }

    public async Task<bool?> GetBubblesState(CancellationToken cancellationToken = default)
    {
        RestClient restClient = new RestClient(ristoBubblesURI);
        RestRequest request = new RestRequest("", Method.Get);

        RestResponse response = await restClient.ExecuteAsync(request, cancellationToken);

        if (!response.IsSuccessful)
        {
            return null;
        }

        RistoBubblesState? state = JsonSerializer.Deserialize<RistoBubblesState>(response.Content!);

        return state?.Enabled;
    }

    private record RistoBubblesState(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("value")] bool Enabled,
        [property: JsonPropertyName("state")] string State);
}
