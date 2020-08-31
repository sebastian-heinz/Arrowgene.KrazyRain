using Arrowgene.Buffers;
using Arrowgene.KrazyRain.GameServer.Network;

namespace Arrowgene.KrazyRain.GameServer.Handler
{
    public class UnknownRequest : PacketHandler
    {
        public override ushort Id => 3;

        public UnknownRequest(KrGameServer server) : base(server)
        {
        }

        public override void Handle(Client client, Packet packet)
        {
            IBuffer r = Buffer.Provide();
            r.WriteUInt32(4);
            r.WriteUInt32(4);
            r.WriteUInt32(4);
            r.WriteUInt32(4);
            r.WriteUInt32(4);

            client.Send(4, r);
        }
    }
}