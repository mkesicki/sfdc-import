using System;
using System.Collections.Generic;
using RestSharp;
using System.Net;
using System.Security.Authentication;
using SFDCImport.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SFDCImport.Salesforce
{
    class Salesforce
    {
        private readonly String ApiVersion = "v48.0";
        private readonly int BatchSize = 200;
        private String Token { get; set; }
        private String ClientId { get; set; }
        private String ClientSecret { get; set; }
        private String Username { get; set; }
        private String Password { get; set; }
        private String LoginUrl { get; set; }
        private String InstanceUrl { get; set; }
        private List<ObjectPayload> Payload { get; set; }

        private Dictionary<String, Metadata> Meta { get; set; }
        RestClient Client;

        public Salesforce(String ClientId, String ClientSecret, String Username, string Password, String LoginUrl)
        {
            this.ClientId = ClientId;
            this.ClientSecret = ClientSecret;
            this.Username = Username;
            this.Password = Password;
            this.LoginUrl = LoginUrl;

            Meta = new Dictionary<String, Metadata>();
            Payload = new List<ObjectPayload>();

            Login();
        }

        public void Login() {

            Console.WriteLine("Login to salesforce: " + LoginUrl);

            Client = new RestClient(LoginUrl);

            RestRequest request = new RestRequest(LoginUrl + "/services/oauth2/token", Method.POST);
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", ClientId);
            request.AddParameter("client_secret", ClientSecret);
            request.AddParameter("username", Username);
            request.AddParameter("password", Password);

            IRestResponse response = Client.Execute(request);

            if (HttpStatusCode.OK == response.StatusCode)
            {
                RestSharp.Serialization.Json.JsonDeserializer deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                Dictionary<String, String> body = deserializer.Deserialize<Dictionary<String, String>>(response);

                Token = "Bearer " + body["access_token"];
                InstanceUrl = body["instance_url"];
                Client = new RestClient(InstanceUrl);

                Console.WriteLine("Connected to instance : {0}", InstanceUrl);

                return;
            }

            throw new AuthenticationException("Login error");
        }

        public void RetrieveMetadata(String ObjectName) {

            Console.WriteLine("Get metadata for {0}", ObjectName);

            RestRequest request = new RestRequest(InstanceUrl + "/services/data/" + ApiVersion + "/sobjects/" + ObjectName + "/describe", Method.GET);
            request.AddHeader("Authorization", Token);

            IRestResponse response = Client.Execute(request);

            if (HttpStatusCode.OK == response.StatusCode)
            {
                RestSharp.Serialization.Json.JsonDeserializer deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                Metadata desc = deserializer.Deserialize<Metadata>(response);
                Meta.Add(ObjectName, desc);

                return;
            }

            throw new ApplicationException("Error getting object Metadata");
        }

        public void PrintPayload(List<ObjectPayload> Payload)
        {

            foreach (ObjectPayload obj in Payload)
            {
                Console.WriteLine("payload object {0}", obj.Name);
                foreach (KeyValuePair<string, object> entry in obj.Fields)
                {
                    Console.WriteLine("field  {0} : {1}: ref {2}", entry.Key, entry.Value, obj.Reference);
                    foreach (var child in obj.Children)
                    {
                        Console.WriteLine("Child object: {0}", child.Name);
                        foreach (KeyValuePair<string, object> field in child.Fields)
                        {
                            Console.WriteLine("field  {0} : {1}: ref {2}", field.Key, field.Value.ToString(), child.Reference);
                        }
                    }
                }
            }
        }

        public void PreparePayload(Dictionary<string, List<string>> Relations, Dictionary<string, List<string>> Header, String[] data) {

            int referenceNumber = Payload.Count + 1;
            List<ObjectPayload> Children = new List<ObjectPayload>();
            ObjectPayload parent = new ObjectPayload();
            int i = 0;
            foreach (KeyValuePair<string, List<String>> entry in Header)
            {
                
                Dictionary<String, object> fields = new Dictionary<String, object>();
                
                foreach (String column in entry.Value)
                {
                    fields.Add(column, data[i]);
                    i++;
                }

                if (Relations.ContainsKey(entry.Key)) {
                    parent = new ObjectPayload { Name = entry.Key, Fields = fields, Reference = referenceNumber };
                } else {
                    Children.Add(new ObjectPayload { Name = entry.Key, Fields = fields, Reference = referenceNumber });
                }
            }

            if (parent != null)
            {
                Payload.Add(
                     new ObjectPayload { Name = parent.Name, Fields = parent.Fields, Reference = parent.Reference, Children = Children }
                ); 
            }
            else {
                foreach (ObjectPayload body in Children) {

                    Payload.Add(
                     new ObjectPayload { Name = body.Name, Fields = body.Fields, Reference = body.Reference }
                );
                }
            }

            PrintPayload(Payload);
            PushData(Relations, Payload);
        }

        public void PushData(Dictionary<string, List<string>> Relation, List<ObjectPayload> Payload)
        {
            List<Record> records = new List<Record>();
            SalesforceBody body = new SalesforceBody();
            foreach (ObjectPayload PayloadObject in Payload)
            {
                Dictionary<string, string> Attributes = new Dictionary<string, string>();
                Attributes.Add("type", PayloadObject.Name);
                Attributes.Add("referenceId", "ref" + PayloadObject.Reference);

                Dictionary<string, SalesforceBody> children = new Dictionary<string, SalesforceBody>();
                List<Record> childrenObjects;

                Boolean isChildExists = false;

                foreach (ObjectPayload Child in PayloadObject.Children)
                {
                    String keyName = Child.Name; //get name from metadata

                    if (children.ContainsKey(keyName)) {
                        childrenObjects = children[keyName].records;
                        isChildExists = true;
                    } else {
                        childrenObjects = new List<Record>();
                    };

                    Dictionary<string, string> ChildAttributes = new Dictionary<string, string>();
                    
                    ChildAttributes.Add("type", Child.Name);
                    ChildAttributes.Add("referenceId", "ref" + Child.Reference);

                    if (childrenObjects.Count == 0)
                    {
                        childrenObjects.Add(new Record { attributes = ChildAttributes, fields = Child.Fields });
                    }
                    else {
                        childrenObjects.Add(new Record { fields = Child.Fields });
                    }

                    if (!isChildExists)
                    {
                        children.Add(keyName, new SalesforceBody { records = childrenObjects });
                    }
                    else {
                        children[keyName].records = childrenObjects;
                    }
                }

                records.Add(
                    new Record { attributes = Attributes, fields = PayloadObject.Fields, children = children }
                );

                body.records = records;
                string jsonBody = JsonConvert.SerializeObject(body, Formatting.None, new RecordObjectConverter());
                Console.WriteLine("Salesforce payload: {0}", jsonBody);
            }
        }
    }

    public class ObjectPayload {
        public String Name { get; set; }
        public Dictionary<string, object> Fields { get; set; }
        public int Reference {get; set;}
        public List<ObjectPayload> Children { get; set; }
    }

    public class Record
    {
        public Dictionary<string, string> attributes { get; set; }

        public Dictionary<String, object> fields { get; set; }

        public Dictionary<string, SalesforceBody> children { get; set; }
    }

    public class SalesforceBody
    {
        public List<Record> records { get; set; }
    }

    internal class RecordObjectConverter : CustomCreationConverter<Record>
    {
        public override Record Create(Type objectType)
        {
            return new Record
            {
                children = new Dictionary<string, SalesforceBody>(),
                fields = new Dictionary<string, object>(),
                attributes = new Dictionary<string, string>()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            // Write properties.
            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                // Skip the children & fields property.
                if (propertyInfo.Name == "children" || propertyInfo.Name == "fields")
                    continue;

                writer.WritePropertyName(propertyInfo.Name);
                var propertyValue = propertyInfo.GetValue(value);
                serializer.Serialize(writer, propertyValue);
            }

            // Write dictionary key-value pairs.
            var record = (Record)value;
            if (record.children != null)
            {
                foreach (var kvp in record.children)
                {
                    writer.WritePropertyName(kvp.Key);
                    serializer.Serialize(writer, kvp.Value);
                }
            }

            if (record.fields != null)
            {
                foreach (var kvp in record.fields)
                {
                    writer.WritePropertyName(kvp.Key);
                    serializer.Serialize(writer, kvp.Value);
                }
            }
            writer.WriteEndObject();
        }
        public override bool CanWrite
        {
            get { return true; }
        }

    }
}

