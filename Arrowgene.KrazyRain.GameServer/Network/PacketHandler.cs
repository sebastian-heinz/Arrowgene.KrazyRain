using Arrowgene.Buffers;

namespace Arrowgene.KrazyRain.GameServer.Network
{
    public abstract class PacketHandler : IPacketHandler
    {
        protected PacketHandler(KrGameServer server)
        {
            Server = server;
            Settings = server.Setting;
        }

        protected IBufferProvider Buffer => KrGameServer.Buffer;
        protected KrGameServer Server { get; }
        protected Setting Settings { get; }

        public abstract ushort Id { get; }
        public abstract void Handle(Client client, Packet packet);
    }
}