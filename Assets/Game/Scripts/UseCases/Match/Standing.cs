namespace Bw.UseCases.Match
{
    public readonly struct Standing
    {
        public readonly string Nickname;
        public readonly string Figure;
        public readonly Score Score;
        public readonly bool IsMine;

        public Standing(
            string nickname,
            string figure,
            Score score,
            bool isMine)
        {
            Nickname = nickname;
            Figure = figure;
            Score = score;
            IsMine = isMine;
        }
    }
}
