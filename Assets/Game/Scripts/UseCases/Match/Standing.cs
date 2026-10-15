using Bw.UseCases.Character;

namespace Bw.UseCases.Match
{
    public readonly struct Standing
    {
        public readonly string Nickname;
        public readonly CharacterDescription Character;
        public readonly Score Score;
        public readonly bool IsMine;

        public Standing(
            string nickname,
            CharacterDescription character,
            Score score,
            bool isMine)
        {
            Nickname = nickname;
            Character = character;
            Score = score;
            IsMine = isMine;
        }
    }
}
