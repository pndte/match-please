using System;
using System.Globalization;
using System.Text;

namespace Bw.Entities.Network
{
    public static class ConnectionRequestPayload
    {
        private const string Separator = "\u001F";
        private const int MaxBytes = 256;

        public static byte[] Of(ConnectionRequest request) =>
            Encoding.UTF8.GetBytes(request.CharacterKey.ToString(CultureInfo.InvariantCulture) + Separator + request.Nickname);

        public static bool TryRead(byte[] payload, out ConnectionRequest request)
        {
            request = default;
            if (payload.Length > MaxBytes)
                return false;

            var text = Encoding.UTF8.GetString(payload);
            var cut = text.IndexOf(Separator, StringComparison.Ordinal);
            if (cut < 0 || !int.TryParse(text.Substring(0, cut), NumberStyles.None, CultureInfo.InvariantCulture, out var key))
                return false;

            request = new ConnectionRequest(text.Substring(cut + Separator.Length), key);
            return true;
        }
    }
}
