using System;
using System.Diagnostics;
using PostSharp.Aspects;

namespace Intranet.Aspects
{
    /// <summary>
    /// Profiles a method.
    /// Records the start and end times of a method.
    /// </summary>
    [Serializable]
    [ProfilerAspect(AttributeExclude = true)]
    public class ProfilerAspect : OnMethodBoundaryAspect
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnEntry(MethodExecutionArgs args)
        {
            args.MethodExecutionTag = Stopwatch.StartNew();
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Stopwatch sw = (Stopwatch)args.MethodExecutionTag;
            sw.Stop();

            string output = string.Format("{0} Executed in {1} second(s)", args.Method.Name, sw.ElapsedMilliseconds / 1000);
            log.Info(output);
        }

        // Forced change for Jay
    }
}
