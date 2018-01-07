using System;
using System.Diagnostics;
using PostSharp.Aspects;

namespace Intranet.Aspects
{
    [Serializable]
    [ProfilerAspect(AttributeExclude = true)]
    public class LoggingAspect : OnMethodBoundaryAspect
    {
        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public override void OnEntry(MethodExecutionArgs args)
        {
            var message = args.Method.Name + " started.";
            Log.Debug(message);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var message = args.Method.Name + " finished.\n";
            Log.Debug(message);
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            var message = args.Method.Name + " finished with no errors.";
            Log.Debug(message);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            var message = args.Method.Name + " failed with errors!";
            Log.Debug(message);
            Log.Error(message);
        }

        // Forced change for Jay
    }
}
