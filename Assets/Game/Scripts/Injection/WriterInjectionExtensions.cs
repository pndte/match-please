using System;
using Zenject;

namespace Bw.Injection
{
    public static class WriterInjectionExtensions
    {
        public static CopyNonLazyBinder WriterOnlyInto<TWriter>(this ConditionCopyNonLazyBinder binder, params Type[] writers) =>
            binder.When(context => context.MemberType != typeof(TWriter) || IsWriter(writers, context.ObjectType));

        private static bool IsWriter(Type[] writers, Type type)
        {
            if (type == null)
                return false;

            foreach (var writer in writers)
                if (writer == type || writer.IsGenericTypeDefinition && type.IsGenericType && type.GetGenericTypeDefinition() == writer)
                    return true;

            return false;
        }
    }
}
