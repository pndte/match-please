using System;
using Zenject;

namespace Bw.Injection
{
    public static class WriterInjectionExtensions
    {
        public static CopyNonLazyBinder WriterOnlyInto<TWriter>(this ConditionCopyNonLazyBinder binder, params Type[] writers) =>
            binder.When(context => context.MemberType != typeof(TWriter) || Array.IndexOf(writers, context.ObjectType) >= 0);
    }
}
