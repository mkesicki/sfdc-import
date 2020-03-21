This is fun project which tries to solve annoying issues with importing to Salesforce data from CSV when there are relations in dataset.

This is just and excercise (yes too much time during pandemia... ) and be aware that it is not the best idea to use it on production.

This will work only with CSV files with real comma separated files and specified format of madatory header in format:
- ObjectName.FieldName where there is no relation
- ObjectName.FieldName.RelatedObjectName where there is a relation