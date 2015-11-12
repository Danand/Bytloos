#if NET45
using System;
using System.Runtime.CompilerServices;
using Bytloos.Extensions;
#endif

namespace Bytloos
{
    /// <summary>
    /// Tools for calls.
    /// </summary>
    public static class Invoker
    {
        #if NET45

        private static string previousCallID;

        /// <summary>
        /// Calls action once per loop.
        /// </summary>
        /// <param name="action">Target action.</param>
        /// <param name="sourceFilePath">Passes CallerFilePathAttribute value.</param>
        /// <param name="memberName">Passes CallerMemberNameAttribute value.</param>
        /// <param name="sourceLineNumber">Passes CallerLineNumberAttribute value.</param>
        public static void CallOnce(
                                Action  action,
            [CallerFilePath]    string  sourceFilePath      = "",
            [CallerMemberName]  string  memberName          = "",
            [CallerLineNumber]  int     sourceLineNumber    = 0)
        {
            var callerInfoHash = (sourceFilePath + memberName + sourceLineNumber).MD5();

            if (callerInfoHash != previousCallID)
                action();

            previousCallID = callerInfoHash;
        }

        #endif
    }
}
