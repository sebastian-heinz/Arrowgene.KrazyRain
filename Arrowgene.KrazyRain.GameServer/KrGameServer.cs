using Arrowgene.Buffers;
using Arrowgene.KrazyRain.GameServer.Handler;
using Arrowgene.Logging;
using Arrowgene.Networking.Tcp.Server.AsyncEvent;

namespace Arrowgene.KrazyRain.GameServer
{
    public class KrGameServer
    {
        private static readonly ILogger Logger = LogProvider.Logger<Logger>(typeof(KrGameServer));

        private readonly AsyncEventServer _server;
        private readonly Consumer _consumer;

        public static readonly IBufferProvider Buffer = new StreamBuffer();

        public KrGameServer(Setting setting)
        {
            Setting = new Setting(setting);
            _consumer = new Consumer(Setting);

            _consumer.AddHandler(new LoginRequest(this));
            _consumer.AddHandler(new UnknownRequest(this));

            _server = new AsyncEventServer(
                Setting.ListenIpAddress,
                Setting.Port,
                _consumer,
                Setting.SocketSettings
            );
        }

        public Setting Setting { get; }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}