using System;
using System.Globalization;
using System.Text;

namespace Bw.Entities.Network
{
    public static class ConnectionRequestPayload
    {
        private const string Separator = "\u001F";
        private const string TokenFormat = "N";
        private const int MaxBytes = 256;

        public static byte[] Of(ConnectionRequest request) =>
            Encoding.UTF8.GetBytes(
                request.CharacterKey.ToString(CultureInfo.InvariantCulture)
                + Separator + request.Token.ToString(TokenFormat)
                + Separator + request.HostKey.ToString(TokenFormat)
                + Separator + request.Nickname);

        public static bool TryRead(byte[] payload, out ConnectionRequest request)
        {
            request = default;
            if (payload.Length > MaxBytes)
                return false;

            var text = Encoding.UTF8.GetString(payload);
            var keyEnd = text.IndexOf(Separator, StringComparison.Ordinal);
            if (keyEnd < 0 || !int.TryParse(text.Substring(0, keyEnd), NumberStyles.None, CultureInfo.InvariantCulture, out var key))
                return false;

            var tokenStart = keyEnd + Separator.Length;
            if (!TryReadGuid(text, tokenStart, out var token, out var tokenEnd))
                return false;

            var hostKeyStart = tokenEnd + Separator.Length;
            if (!TryReadGuid(text, hostKeyStart, out var hostKey, out var hostKeyEnd))
                return false;

            request = new ConnectionRequest(text.Substring(hostKeyEnd + Separator.Length), key, token, hostKey);
            return true;
        }

        private static bool TryReadGuid(
            string text,
            int start,
            out Guid guid,
            out int end)
        {
            guid = default;
            end = text.IndexOf(Separator, start, StringComparison.Ordinal);
            return end >= 0 && Guid.TryParseExact(text.Substring(start, end - start), TokenFormat, out guid);
        }
    }
}
