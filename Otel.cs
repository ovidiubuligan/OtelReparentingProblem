using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ovidiu
{
    public class Otel
    {
        private static readonly string  _activitySourceName = "activitysourcename";
        public static ActivitySource ActivitySource = new ActivitySource (
                        _activitySourceName);
        public static TracerProvider TraceProvider = null;
                        //"semver1.0.0");
        public static  void InitOtelTracing()
        {
            // See: https://docs.microsoft.com/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch ("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            Sdk.SetDefaultTextMapPropagator (new CompositeTextMapPropagator (
                new TextMapPropagator[]{
                    new B3Propagator(),
                    new TraceContextPropagator()
                }
            ));
            var builder = Sdk.CreateTracerProviderBuilder ();
            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault ().AddService ("vbus-console");
            builder.SetResourceBuilder (resourceBuilder)
                .AddSource (_activitySourceName)
                .SetSampler (new AlwaysOnSampler());// MySampler ());
            // builder.AddGrpcCoreInstrumentation ();
            builder.AddHttpClientInstrumentation ();


            builder.AddOtlpExporter(o =>
            {
               o.Endpoint = new Uri("http://localhost:4317");
               o.ExportProcessorType = ExportProcessorType.Simple;
            });
            //builder.AddJaegerExporter (o =>
            // {
            //     o.AgentHost = "localhost";
            //     o.AgentPort = 6831;
            // });
            builder.AddConsoleExporter();

            TraceProvider = builder.Build ();

        }

    }
}
