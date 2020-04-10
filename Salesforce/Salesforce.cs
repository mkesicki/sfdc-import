using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using System.Net;
using System.Security.Authentication;
using SFDCImport.Model;

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
                //Console.WriteLine("Metadata for {0} : {1}", ObjectName, response.Content.ToString());

                RestSharp.Serialization.Json.JsonDeserializer deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                Metadata desc = deserializer.Deserialize<Metadata>(response);
                Meta.Add(ObjectName, desc);

                return;
            }

            throw new ApplicationException("Error getting object Metadata");
        }

        public void PreparePayload(Dictionary<string, List<string>> Relations, Dictionary<string, List<string>> Header, String[] data) {

            int referenceNumber = Payload.Count + 1;
            List<ObjectPayload> Children = new List<ObjectPayload>();
            ObjectPayload parent = new ObjectPayload();
            int i = 0;
            foreach (KeyValuePair<string, List<String>> entry in Header)
            {

                Dictionary<String, String> fields = new Dictionary<String, String>();
                
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

            
        }

        public void PushData(Dictionary<string, List<string>> Relation, List<ObjectPayload> Payload) {

            //int i = 1;
            //foreach (KeyValuePair<string, List<String>> relation in Relation) { 

            //    foreach()
                
            //}
        }


        public void PrintPayload(List<ObjectPayload> Payload) {
           
            foreach (ObjectPayload obj in Payload)
            {

                Console.WriteLine("payload object {0}", obj.Name);
                foreach (KeyValuePair<string, String> entry in obj.Fields)
                {
                    Console.WriteLine("field  {0} : {1}: ref {2}", entry.Key, entry.Value, obj.Reference);
                    foreach (var child in obj.Children)
                    {
                        Console.WriteLine("Child object: {0}", child.Name);
                        foreach (KeyValuePair<string, String> field in child.Fields)
                        {
                            Console.WriteLine("field  {0} : {1}: ref {2}", field.Key, field.Value, child.Reference);

                        }
                    }
                }
            }
        }
    }

    public class ObjectPayload {

        public String Name { get; set; }
        public Dictionary<string, string> Fields { get; set; }
        public int Reference {get; set;}
        public List<ObjectPayload> Children { get; set; }

    }

    public class Record {

        Dictionary<String, string> attributes { get; set; }

        Dictionary<String, string> fields { get; set; }

        Dictionary<string, List<Record>> relations { get; set; }

    }

    public class JsonBody { 

        List<Record> records { get; set; }

    }
}
