using System.Net;
using System.Runtime.Serialization;
using Arrowgene.Networking.Tcp.Server.AsyncEvent;

namespace Arrowgene.KrazyRain.GameServer
{
    [DataContract]
    public class Setting
    {
        [IgnoreDataMember] public IPAddress ListenIpAddress { get; set; }

        [DataMember(Name = "ListenIpAddress", Order = 0)]
        public string DataListenIpAddress
        {
            get => ListenIpAddress.ToString();
            set => ListenIpAddress = string.IsNullOrEmpty(value) ? null : IPAddress.Parse(value);
        }
        
        [DataMember(Order = 6)] public ushort Port { get; set; }
        
        [DataMember(Order = 100)] public AsyncEventSettings SocketSettings { get; set; }
        
          public Setting()
        {
            ListenIpAddress = IPAddress.Any;
            Port = 28950;
            SocketSettings = new AsyncEventSettings();
            SocketSettings.MaxUnitOfOrder = 2;
        }

        public Setting(Setting setting)
        {
            ListenIpAddress = setting.ListenIpAddress;
            Port = setting.Port;
            SocketSettings = new AsyncEventSettings(setting.SocketSettings);
        }
    }
}