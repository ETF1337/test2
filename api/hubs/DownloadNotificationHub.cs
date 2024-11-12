using Microsoft.AspNetCore.SignalR;

public class DownloadNotificationHub : Hub
{
    public async Task NotifyDownload(string message)
    {
        await Task.Delay(5000);
        
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}