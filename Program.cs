using ETW;
using System;
using System.ComponentModel;
using System.Diagnostics.Tracing;

namespace ETWTrigger
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid triggerProvider;
            uint triggerLevel = (uint)EventLevel.Error;
            uint triggerKeyword = 0;
            int res = 0;
            try
            {
                triggerProvider = Guid.Parse(args[0]);
                circularSessionName = args[1];
                logFileName = args.Length > 2 ? args[2] : "Flush.etl";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Help();
                return;
            }

            Console.WriteLine($"Start {triggerSessionName}");
            Console.CancelKeyPress += (s, a) => StopTrace();
            var triggerProperties = new EVENT_TRACE_PROPERTIES()
            {
                LogFileMode = LogFileMode.REAL_TIME_MODE | LogFileMode.USE_MS_FLUSH_TIMER,
                FlushTimer = 1,
            };
            res = Native.StartTrace(out var triggerSessionHandle, triggerSessionName, triggerProperties);
            if (res != 0)
            {
                throw new Win32Exception(res);
            }
            res = Native.EnableTrace(1, triggerKeyword, triggerLevel, triggerProvider, triggerSessionHandle);

            Console.WriteLine($"Process {triggerSessionName}");
            var triggerLog = new EVENT_TRACE_LOGFILEW
            {
                LoggerName = triggerSessionName,
                ProcessTraceMode = ProcessTraceMode.REAL_TIME | ProcessTraceMode.EVENT_RECORD,
                EventRecordCallback = EventRecordCallback,
            };
            var triggerLogHandle = Native.OpenTraceW(ref triggerLog);
            res = Native.ProcessTrace(new[] { triggerLogHandle }, 1, 0, 0);
            if (res != 0)
            {
                Console.WriteLine(new Win32Exception(res).Message);
            }
            Native.CloseTrace(triggerLogHandle);
            StopTrace();
        }

        static void Help()
        {
            Console.Write(Properties.Resources.README);
        }

        static void StopTrace()
        {
            var stopProperties = new EVENT_TRACE_PROPERTIES();
            Native.StopTrace(0, triggerSessionName, stopProperties);
        }

        const string triggerSessionName = "TriggerSession";
        static string circularSessionName;
        static string logFileName;
        static int count;

        static void EventRecordCallback(in EVENT_RECORD eventRecord)
        {
            if (eventRecord.EventHeader.EventDescriptor.Level > 0 && count < 1)
            {
                count++;
                var circularProperties = new EVENT_TRACE_PROPERTIES()
                {
                    LogFileName = logFileName,
                };
                int res = Native.ControlTraceW(0, circularSessionName, circularProperties, ControlCode.Flush);
                StopTrace();
                ref readonly var header = ref eventRecord.EventHeader;
                ref readonly var descriptor = ref header.EventDescriptor;
                Console.WriteLine($"Event {(EventLevel)descriptor.Level} Id:{descriptor.Id} Provider:{header.ProviderId}");
                Console.WriteLine($"Flush {circularSessionName} to {logFileName}");
                if (res != 0)
                {
                    Console.WriteLine(new Win32Exception(res).Message);
                }
            }
        }
    }
}
