using System;
using System.Diagnostics;
using PostSharp.Aspects;

namespace Intranet.Aspects
{
    /// <summary>
    /// Intranet Exception Aspect
    /// </summary>
    [Serializable]
    [ProfilerAspect(AttributeExclude = true)]
    public class ExceptionAspect : OnExceptionAspect
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnException(MethodExecutionArgs args)
        {
            string msg = string.Format("{0} had an error @ {1}: {2}\n{3}",
                args.Method.Name, DateTime.Now,
                args.Exception.Message, args.Exception.StackTrace);
            Trace.WriteLine(msg);
            log.Error(msg);

            base.OnException(args);

            // Forced change for Jay
        }
    }
}
