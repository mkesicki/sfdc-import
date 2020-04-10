This is fun project which tries to solve annoying issues with importing to Salesforce data from CSV when there are relations in dataset.

This is just and excercise (yes too much time during pandemia... ) and be aware that it is not the best idea to use it on production.

This will work only with CSV files with real comma separated files and specified format of madatory header in format:
- ObjectName.FieldName where there is no relation
- ObjectName.FieldName.RelatedObjectName where there is a relation (for the related column). Do not insert in CSV file column with relation values.
- Only one level relation will be handled so for example Account (parent) & Opportunity (child) 


#Issues
- Improve performance of logger, maybe after all doing log per thread is not so stupid idea
- will work only with comma separated values, this might be configured
- use ReaderWriterLock https://docs.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlock?redirectedfrom=MSDN&view=netframework-4.8
  instead of copy files per thread
