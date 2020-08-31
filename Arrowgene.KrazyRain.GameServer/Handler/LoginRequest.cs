using Arrowgene.Buffers;
using Arrowgene.KrazyRain.GameServer.Logging;
using Arrowgene.KrazyRain.GameServer.Network;
using Arrowgene.Logging;

namespace Arrowgene.KrazyRain.GameServer.Handler
{
    public class LoginRequest : PacketHandler
    {
        private static readonly KrLogger Logger = LogProvider.Logger<KrLogger>(typeof(LoginRequest));


        public override ushort Id => 257;

        public LoginRequest(KrGameServer server) : base(server)
        {
        }

        public override void Handle(Client client, Packet packet)
        {
            uint unknown = packet.Data.ReadUInt32();
            string account = packet.Data.ReadFixedString(17); // 16 usable bytes - cmd arg 1
            string hash = packet.Data.ReadFixedString(33); // 32 usable bytes - cmd arg 2
            byte number = packet.Data.ReadByte(); // - cmd arg 3

            Logger.Info($"Unk:{unknown} Acc:{account} Hash:{hash} Num:{number}");

            IBuffer r = Buffer.Provide();
            r.WriteUInt32(unknown);
            r.WriteFixedString(account, 17);
            r.WriteUInt32(3);
            r.WriteUInt32(4);
            r.WriteUInt32(5);

            for (int i = 0; i < 1000; i++)
            {
                client.Send((ushort)i, r);
            }

         //   client.Send(257, r);
          //  client.Send(258, r);
        }
    }
}