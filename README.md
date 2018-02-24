The ETWTrigger command fluhes a memory buffered trace

Monitors an ETW provider and flushes a buffered trace to an ETL file when the trigger provider writes an error event.

usage: etwtrigger guid trace etl
- guid: guid of a ETW provider
- trace: buffering trace name
- etl: file name of the ETL flush output

sample: ETWTrigger {e13c0d23-ccbc-4e12-931b-d9cc2eee27e4} "Circular Logger" Flush.etl