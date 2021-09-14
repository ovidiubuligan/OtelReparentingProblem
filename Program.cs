using System;
using System.Diagnostics;
using Ovidiu;
using System.Reflection;

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
                        act.Start();
                        act.SetParentId ("00-00000000000000000000000000000000-0000000000000000-00");

                        // Overwrite the activity Id as the one from activity2
                        string? idstr = "00-" + activity2.TraceId + "-" + activity2.SpanId + "-" + "01"; //activity2.ActivityTraceFlags...ToString ();
                        Type typeInQuestion = typeof (Activity);
                        //FieldInfo id = typeInQuestion.GetField ("_id", BindingFlags.NonPublic | BindingFlags.Instance);
                        //id.SetValue (act, idstr);

                        FieldInfo spanId = typeInQuestion.GetField ("_spanId", BindingFlags.NonPublic | BindingFlags.Instance);
                        string? spanidOpt = activity2.SpanId.ToString();
                        spanId.SetValue (act, spanidOpt);

                        FieldInfo traceID = typeInQuestion.GetField ("_traceId", BindingFlags.NonPublic | BindingFlags.Instance);
                        string? traceiDOpt = activity2.TraceId.ToString();
                        traceID.SetValue (act, traceiDOpt);

                        FieldInfo _parentSpanIdField = typeInQuestion.GetField("_parentSpanId", BindingFlags.NonPublic | BindingFlags.Instance);
                        string? parentSpanIdValue = activity2.ParentSpanId.ToString();
                        _parentSpanIdField.SetValue(act, parentSpanIdValue);

                        if (act != null) Activity.Current = act;


                        using (var activity4 = source.StartActivity ("4_ovidiu_parented_to_2"))
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
