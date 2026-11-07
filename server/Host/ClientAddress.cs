using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace SmartCv.Host;

public interface IClientAddress
{
    string BaseUrl { get; }
}

public class ClientAddress : IClientAddress
{
    private readonly AppOptions _options;
    private readonly IServer _server;

    public ClientAddress(AppOptions options, IServer server)
    {
        _options = options;
        _server = server;
    }

    public string BaseUrl => _options.Mode == AppMode.Dev
        ? AppOptions.DevClientUrl
        : _server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault()?.TrimEnd('/') ?? AppOptions.DevServerUrl;
}
