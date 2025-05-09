using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System;
public class ClientInfo
{
    public string NickName { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public long UnixTime { get; set; }
}

static public class Global
{
    static public readonly float PingInterval = 5f;
    static public readonly float Timeout = 10f;
    static public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1"; // ±âº»°ª
    }
    static public string GetPublicIPAddress()
    {
        using (WebClient client = new WebClient())
        {
            return client.DownloadString("https://api64.ipify.org").Trim();
        }
    }
    static public IPEndPoint StringToIPEndPoint(string ipEndPoint)
    {
        var parts = ipEndPoint.Split(':');
        if (parts.Length != 2)
        {
            throw new FormatException("Invalid endpoint format. Expected format: IP:Port");
        }

        if (!IPAddress.TryParse(parts[0], out IPAddress ipAddress))
        {
            throw new FormatException("Invalid IP address format.");
        }

        if (!int.TryParse(parts[1], out int port) || port < 0 || port > 65535)
        {
            throw new FormatException("Invalid port number.");
        }

        return new IPEndPoint(ipAddress, port);
    }
}
