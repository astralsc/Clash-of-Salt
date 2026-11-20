using System.Threading.Tasks;
using ClashofClans.Protocol;
using ClashofClans.Utilities.Netty;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace ClashofClans.Core.Network.Handlers
{
    public class PacketEncoder : ChannelHandlerAdapter
    {
        public override Task WriteAsync(IChannelHandlerContext context, object msg)
        {
            var message = (PiranhaMessage) msg;

            message.Encode();

            var payload = new byte[message.Writer.ReadableBytes];
            message.Writer.GetBytes(message.Writer.ReaderIndex, payload);
            payload = message.Device.Cipher.Encrypt(payload, message.Id);

            var frame = PooledByteBufferAllocator.Default.Buffer(14 + payload.Length);
            frame.WriteUnsignedShort(message.Id);
            frame.WriteMedium(payload.Length);
            frame.WriteUnsignedShort(message.Version);
            frame.WriteBytes(payload);
            frame.WriteHex("FFFF0000000000");

            return base.WriteAsync(context, frame);
        }
    }
}