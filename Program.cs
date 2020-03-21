using System;
using System.IO;
using SFDCImport.Logger;

namespace SFDCImport
{
    class Program
    {
        static void Main(string[] args)
        {
            /**
             * client_id
             * client_secret
             * username
             * password
             * path to file
             * login url
             * 
             **/

            //check number of arguments passed to applicaiton
            if (args.Length < 6) {
                Console.WriteLine("You dind't pass all necessary parameters");
                Help();
            }

            Console.WriteLine("Ok, let's rock!");

            //create necessary directories
            if (!Directory.Exists("results")) {
                Directory.CreateDirectory("results");
            }

            if (!Directory.Exists("tmp")) {
                Directory.CreateDirectory("tmp");
            }

            if (!Directory.Exists("logs")) {
                Directory.CreateDirectory("logs");
            }

            String ClientID = args[0];
            String ClientSecret = args[1];
            String Username = args[2];
            String Password = args[3];
            String CSV = args[4];
            String LoginUrl = args[5];

            if (!File.Exists(CSV)) {
                throw new FileNotFoundException("The file was not found!", CSV);
            }

            //create logs 
            FileLog LogSuccess = new FileLog("logs", "success.md");
            FileLog LogError = new FileLog("logs", "errors.md");

            LogSuccess.Info("Tests");


            //parse file labels/header ->  class
            //prepare threads and copy file -> class

            //login salesforce -> salesfore class

            //get metadata of objects in salesforce -> sfdc class

            //parse file in threads -> class

            //store logs -> class
        }

        private static void Help() {
            Console.WriteLine("SFDC Import is a simmple console app to insert objects in Salesforce from CSV file. \n" +
                "It creates object with realations and is parsing file with threads \n" +
                "Was creted for learn and fun but the idea of creating parent and child object in one call might \n" +
                "be useful in real case scenarios \n\n" +
                "Required Parameters (in that order): \n" +
                "--client_id - saleforce application client id \n" +
                "--client_secret - saleforce application client secret \n" +
                "--username - saleforce username \n" +
                "--password saleforce password \n" +
                "--login_url - saleforce login instance url \n" +
                "--path - path to CSV file \n"
             );
            Environment.Exit(0);
        }

    }
}
