using System;

namespace Setup.Launch
{
    public abstract class LevelAnswer
    {
        private LevelAnswer()
        {
        }

        public abstract TResult Switch<TResult>(Func<string, TResult> given, Func<TResult> missing);

        public sealed class Given : LevelAnswer
        {
            public string Level { get; }

            public Given(string level)
            {
                Level = level;
            }

            public override TResult Switch<TResult>(Func<string, TResult> given, Func<TResult> missing) =>
                given(Level);
        }

        public sealed class Missing : LevelAnswer
        {
            public static readonly Missing Instance = new();

            private Missing()
            {
            }

            public override TResult Switch<TResult>(Func<string, TResult> given, Func<TResult> missing) =>
                missing();
        }
    }
}
