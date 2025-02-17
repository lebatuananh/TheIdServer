﻿using Honeycomb.OpenTelemetry;
using OpenTelemetry.Exporter;
using System;
using System.Collections.Generic;

namespace Aguacongas.TheIdServer.Options.OpenTelemetry
{
    public class TraceOptions
    {
        public ServiceOptions Service { get; set; }

        public InstrumentationOptions Instrumentation { get; set; }

        public bool ConsoleEnabled { get; set; }

        public OtlpExporterOptions OpenTelemetryProtocol { get; set; }
        public JaegerExporterOptions Jaeger { get; set; }
        public ZipkinExporterOptions Zipkin { get; set; }
        public HoneycombOptions Honeycomb { get; set; }

        public IEnumerable<string> Sources { get; set; }

    }
}
