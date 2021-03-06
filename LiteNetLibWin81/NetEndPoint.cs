#if WINRT && !UNITY_EDITOR
using System;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace LiteNetLib
{
    public sealed class NetEndPoint
    {
        public string Host { get { return HostName.DisplayName; } }
        public int Port { get; private set; }
        internal readonly HostName HostName;
        internal readonly string PortStr;

        internal NetEndPoint(int port)
        {
            HostName = null;
            PortStr = port.ToString();
            Port = port;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetEndPoint))
            {
                return false;
            }
            NetEndPoint other = (NetEndPoint) obj;
            return HostName.IsEqual(other.HostName) && PortStr.Equals(other.PortStr);
        }

        public override int GetHashCode()
        {
            return HostName.CanonicalName.GetHashCode() ^ PortStr.GetHashCode();
        }

        internal long GetId()
        {
            //Check locals
            if (HostName == null)
            {
                return ParseIpToId("0.0.0.0");
            }

            if (HostName.DisplayName == "localhost")
            {
                return ParseIpToId("127.0.0.1");
            }

            //Check remote
            string hostIp = string.Empty;
            var task = DatagramSocket.GetEndpointPairsAsync(HostName, "0").AsTask();
            task.Wait();

            //IPv4
            foreach (var endpointPair in task.Result)
            {
                hostIp = endpointPair.RemoteHostName.CanonicalName;
                if (endpointPair.RemoteHostName.Type == HostNameType.Ipv4)
                {
                    return ParseIpToId(hostIp);
                }
            }

            //Else
            return hostIp.GetHashCode() ^ Port;
        }

        private long ParseIpToId(string hostIp)
        {
            long id = 0;
            string[] ip = hostIp.Split('.');
            id |= long.Parse(ip[0]);
            id |= long.Parse(ip[1]) << 8;
            id |= long.Parse(ip[2]) << 16;
            id |= long.Parse(ip[3]) << 24;
            id |= (long)Port << 32;
            return id;
        }

        public override string ToString()
        {
            return HostName.CanonicalName + ":" + PortStr;
        }

        public NetEndPoint(string hostName, int port)
        {
            var task = DatagramSocket.GetEndpointPairsAsync(new HostName(hostName), port.ToString()).AsTask();
            task.Wait();
            HostName = task.Result[0].RemoteHostName;
            Port = port;
            PortStr = port.ToString();
        }

        internal NetEndPoint(HostName hostName, string port)
        {
            HostName = hostName;
            Port = int.Parse(port);
            PortStr = port;
        }
    }
}
#endif
