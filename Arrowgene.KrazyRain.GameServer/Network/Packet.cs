using Arrowgene.Buffers;

namespace Arrowgene.KrazyRain.GameServer.Network
{
    public class Packet
    {
        public Packet(in ushort id, IBuffer data)
        {
            Id = id;
            Data = data;
        }

        public ushort Id { get; }
        public IBuffer Data { get; }
    }
}