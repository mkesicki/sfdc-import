This is fun project which tries to solve annoying issues with importing to Salesforce data from CSV when there are relations in dataset.

This is just and excercise (yes too much time during pandemia... ) and be aware that it is not the best idea to use it on production.

This will work only with CSV files with real comma separated files and specified format of madatory header in format:
- ObjectName.FieldName where there is no relation
- ObjectName.FieldName.RelatedObjectName where there is a relation


#Issues
- it is BLEAH
- Logger with files definetly do not work good with threads
- will work only with comma separated values, this might be configured
- use ReaderWriterLock https://docs.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlock?redirectedfrom=MSDN&view=netframework-4.8
  instead of copy files per thread
