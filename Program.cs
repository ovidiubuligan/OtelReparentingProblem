using System;
using System.Diagnostics;
using Ovidiu;

namespace OtelReparentingProblem
{
    class Program
    {

        static void Main(string[] args)
        {
            Otel.InitOtelTracing();
            var source = Otel.ActivitySource;

            using (var activity = source.StartActivity ("1_ovidiu_start"))
            {
                System.Threading.Thread.Sleep (200);
                using (var activity2 = source.StartActivity ("2_ovidiu"))
                {
                    using (var activity3 = source.StartActivity ("3_ovidiu"))
                    {
                        // resset current activity to null 
                        Activity.Current = null;

                        // set parrent id of the current span to activity2
                        var parentTraceId = activity2.TraceId;
                        var parentSpanId = activity2.SpanId;
                        var act = new Activity ("reparented");//  "reparented" is actually activity2 
                        act.SetParentId (parentTraceId, parentSpanId);
                        act.Start();
                        if (act != null) Activity.Current = act;


                        using (var activity4 = source.StartActivity ("4_ovidiu"))
                        {
                            // problem HERE  , activity 4 is parented to root trace with a jaeger  warning of :
                            // "invalid parent span IDs=44b5e252f9f99e47; skipping clock skew adjustment"
                            System.Threading.Thread.Sleep (200);
                        }
                    }

                }
                System.Threading.Thread.Sleep (200);
            }
            Console.ReadLine ();
        
        }
    }
}
