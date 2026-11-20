using System;
using System.Security.Cryptography;
using System.Text;
using Sodium;

namespace ClashofClans.Core.Network
{
    public enum PacketCipherMode
    {
        None,
        RC4,
        Pepper
    }

    public sealed class PacketCipher
    {
        private const int KeyLength = 32;
        private const int NonceLength = 24;
        private const int HashLength = NonceLength;

        private static readonly byte[] ServerSecretKey =
            ParseHex("1891d401fadb51d25d3a9174d472a9f691a45b974285d47729c45c6538070d85");
        private static readonly byte[] ServerPublicKey =
            ParseHex("72f1a4a4c48e44da0c42310f800e96624e6dc6a641a9d41c3b5039d8dfadc27e");

        private readonly RC4Stream _rc4Decrypt;
        private readonly RC4Stream _rc4Encrypt;
        private byte[] _clientKey;
        private byte[] _sharedKey;
        private byte[] _decryptNonce;
        private readonly byte[] _encryptNonce;

        public PacketCipher(PacketCipherMode mode)
        {
            Mode = mode;

            if (mode == PacketCipherMode.RC4)
            {
                var key = Encoding.UTF8.GetBytes("fhsd6f86f67rt8fw78fw789we78r9789wer6re");
                var nonce = Encoding.UTF8.GetBytes("nonce");
                _rc4Decrypt = new RC4Stream(key, nonce);
                _rc4Encrypt = new RC4Stream(key, nonce);
            }
            else if (mode == PacketCipherMode.Pepper)
            {
                SodiumCore.Init();
                _encryptNonce = Hash(Encoding.UTF8.GetBytes("hashedpass"));
            }
            else if (mode != PacketCipherMode.None)
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported packet cipher mode.");
            }
        }

        public PacketCipherMode Mode { get; }

        public byte[] Decrypt(byte[] packet, int id)
        {
            if (Mode == PacketCipherMode.None)
                return packet;

            if (Mode == PacketCipherMode.RC4)
                return _rc4Decrypt.Transform(packet);

            if (id == 10100)
                return packet;

            if (id == 10101)
                return DecryptHandshake(packet);

            if (_decryptNonce == null || _sharedKey == null)
                throw new CryptographicException("Pepper packet received before the login handshake.");

            IncrementNonce(_decryptNonce);
            return SecretBox.Open(packet, _decryptNonce, _sharedKey);
        }

        public byte[] Encrypt(byte[] packet, int id)
        {
            if (Mode == PacketCipherMode.None)
                return packet;

            if (Mode == PacketCipherMode.RC4)
                return _rc4Encrypt.Transform(packet);

            if (id == 20100 || (id == 20103 && _clientKey == null))
                return packet;

            if (id == 20103 || id == 20104)
            {
                if (_clientKey == null || _decryptNonce == null || _sharedKey == null)
                    throw new CryptographicException("Pepper response cannot be encrypted before the login handshake.");

                var nonce = ToNonce(Hash(Concat(_decryptNonce, _clientKey, ServerPublicKey)));
                var message = Concat(_encryptNonce, _sharedKey, packet);
                return PublicKeyBox.Create(message, nonce, ServerSecretKey, _clientKey);
            }

            if (_sharedKey == null)
                throw new CryptographicException("Pepper packet cannot be encrypted before the login handshake.");

            IncrementNonce(_encryptNonce);
            return SecretBox.Create(packet, ToNonce(_encryptNonce), _sharedKey);
        }

        private byte[] DecryptHandshake(byte[] packet)
        {
            if (packet.Length < KeyLength)
                throw new CryptographicException("Pepper login packet is shorter than its client key.");

            var clientKey = new byte[KeyLength];
            Buffer.BlockCopy(packet, 0, clientKey, 0, KeyLength);
            var nonce = ToNonce(Hash(Concat(clientKey, ServerPublicKey)));
            var cipherText = new byte[packet.Length - KeyLength];
            Buffer.BlockCopy(packet, KeyLength, cipherText, 0, cipherText.Length);

            var message = PublicKeyBox.Open(cipherText, nonce, ServerSecretKey, clientKey);
            if (message.Length < 48)
                throw new CryptographicException("Pepper login message does not contain the required nonce.");

            _clientKey = clientKey;
            _sharedKey = clientKey;
            _decryptNonce = new byte[NonceLength];
            Buffer.BlockCopy(message, 24, _decryptNonce, 0, NonceLength);

            var payload = new byte[message.Length - 48];
            Buffer.BlockCopy(message, 48, payload, 0, payload.Length);
            return payload;
        }

        private static byte[] Hash(byte[] value)
        {
            return GenericHash.Hash(value, null, HashLength);
        }

        private static byte[] ToNonce(byte[] value)
        {
            var nonce = new byte[NonceLength];
            Buffer.BlockCopy(value, 0, nonce, 0, nonce.Length);
            return nonce;
        }

        private static byte[] ParseHex(string value)
        {
            var result = new byte[value.Length / 2];
            for (var i = 0; i < result.Length; i++)
                result[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            return result;
        }

        private static byte[] Concat(params byte[][] values)
        {
            var length = 0;
            foreach (var value in values)
                length += value.Length;

            var result = new byte[length];
            var offset = 0;
            foreach (var value in values)
            {
                Buffer.BlockCopy(value, 0, result, offset, value.Length);
                offset += value.Length;
            }

            return result;
        }

        private static void IncrementNonce(byte[] nonce)
        {
            var carry = 1;
            for (var i = 0; i < nonce.Length && carry != 0; i++)
            {
                var value = nonce[i] + carry;
                nonce[i] = (byte)value;
                carry = value >> 8;
            }
        }

        private sealed class RC4Stream
        {
            private readonly byte[] _state = new byte[256];
            private int _i;
            private int _j;

            public RC4Stream(byte[] key, byte[] nonce)
            {
                var fullKey = Concat(key, nonce);
                for (var i = 0; i < _state.Length; i++)
                    _state[i] = (byte)i;

                var j = 0;
                for (var i = 0; i < _state.Length; i++)
                {
                    j = (j + _state[i] + fullKey[i % fullKey.Length]) & 0xff;
                    Swap(i, j);
                }

                for (var i = 0; i < fullKey.Length; i++)
                    NextByte();
            }

            public byte[] Transform(byte[] input)
            {
                var output = new byte[input.Length];
                for (var i = 0; i < input.Length; i++)
                    output[i] = (byte)(input[i] ^ NextByte());
                return output;
            }

            private byte NextByte()
            {
                _i = (_i + 1) & 0xff;
                _j = (_j + _state[_i]) & 0xff;
                Swap(_i, _j);
                return _state[(_state[_i] + _state[_j]) & 0xff];
            }

            private void Swap(int first, int second)
            {
                var value = _state[first];
                _state[first] = _state[second];
                _state[second] = value;
            }
        }
    }
}
