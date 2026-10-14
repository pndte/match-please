namespace Bw.Entities.Players
{
    public static class NicknameRules
    {
        public const string Fallback = "Player";
        public const int MinLength = 3;
        public const int MaxLength = 16;

        public static bool IsValid(string nickname)
        {
            if (nickname.Length < MinLength || nickname.Length > MaxLength || nickname.Trim().Length != nickname.Length)
                return false;

            foreach (var symbol in nickname)
                if (!IsAllowed(symbol))
                    return false;

            return true;
        }

        public static string Accepted(string requested)
        {
            var trimmed = requested.Trim();
            return IsValid(trimmed) ? trimmed : Fallback;
        }

        public static bool IsAllowed(char symbol) =>
            symbol is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or ' ' or '_' or '-' or >= 'Ѐ' and <= 'џ';
    }
}
