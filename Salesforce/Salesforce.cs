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
        private String SessionId { get; set; }

        private readonly String ApiVersion = "v48.0";
        private String Token { get; set; }
        private String ClientId { get; set; }
        private String ClientSecret { get; set; }
        private String Username { get; set; }
        private String Password { get; set; }
        private String LoginUrl { get; set; }
        private String InstanceUrl { get; set; }

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

        public void PreparePayload() { }

        public void PostData(String Payload) { }
    }
}
