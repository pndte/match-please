using DG.Tweening;

namespace Bw.UseCases.Shooting.View.Recoil
{
    public static class RecoilPunchExtensions
    {
        public const Ease OutEase = Ease.OutQuad;
        public const Ease ReturnEase = Ease.OutBack;

        public static float Duration(this RecoilPunch punch) =>
            punch.OutTime + punch.ReturnTime;

        public static float OffsetAt(this RecoilPunch punch, float seconds)
        {
            if (seconds <= 0f || seconds >= punch.Duration())
                return 0f;

            return seconds < punch.OutTime
                ? DOVirtual.EasedValue(0f, punch.Amount, seconds / punch.OutTime, OutEase)
                : DOVirtual.EasedValue(punch.Amount, 0f, (seconds - punch.OutTime) / punch.ReturnTime, ReturnEase, punch.Overshoot);
        }
    }
}
