using Microsoft.AspNetCore.SignalR;

namespace Customer_Mangment.Hubs;

public sealed class QueueMonitorHub(ILogger<QueueMonitorHub> logger) : Hub
{
    private readonly ILogger<QueueMonitorHub> _logger = logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Monitor client connected: {ConnectionId} from {RemoteIp}",
            Context.ConnectionId,
            Context.GetHttpContext()?.Connection.RemoteIpAddress);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Monitor client disconnected: {ConnectionId}",
            Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}