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
                request.CharacterKey.ToString(CultureInfo.InvariantCulture) + Separator + request.Token.ToString(TokenFormat) + Separator + request.Nickname);

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
            var tokenEnd = text.IndexOf(Separator, tokenStart, StringComparison.Ordinal);
            if (tokenEnd < 0 || !Guid.TryParseExact(text.Substring(tokenStart, tokenEnd - tokenStart), TokenFormat, out var token))
                return false;

            request = new ConnectionRequest(text.Substring(tokenEnd + Separator.Length), key, token);
            return true;
        }
    }
}
