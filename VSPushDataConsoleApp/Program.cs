using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//STEP 1.0: Get Authentication Access Token *****START
using Microsoft.IdentityModel.Clients.ActiveDirectory;
//STEP 1.0: Get Authentication Access Token *****END
//STEP 2.0: Create Dataset in Power BI *****START
using System.Net;
using System.IO;
//STEP 2.0: Create Dataset in Power BI *****END
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
//STEP 3.0: Get Dataset in Power BI *****START

//STEP 3.0: Get Dataset in Power BI *****END

namespace VSPushDataConsoleApp
{
    class Program
    {
        //STEP 1.2: Get Authentication Access Token *****START
        private static string token = string.Empty;
        //STEP 1.2: Get Authentication Access Token *****END
        static void Main(string[] args)
        {

            //STEP 1.3: Get Authentication Access Token *****START
            //Get an authentication access token
            token = GetToken();
            Console.WriteLine(token);
            //Console.ReadLine();
            //STEP 1.3: Get Authentication Access Token *****END
            //STEP 2.1: Create Dataset in Power BI *****START
            //Create a dataset in Power BI
            CreateDataset();
            //STEP 2.1: Create Dataset in Power BI *****END
            //STEP 3.1: Get Dataset in Power BI *****START
            //Get a dataset to add rows into a Power BI table
            string datasetId = GetDataset();
            //STEP 3.1: Get Dataset in Power BI *****END

            //STEP 4.1: Add Rows to Table in Dataset in Power BI *****START
            //Add rows to a Power BI table
            //Get a dataset to add rows into a Power BI table
            AddRows(datasetId, "Product");
            //AddRows(datasetId, "Product");
            //STEP 4.1: Add Rows to Table in Dataset in Power BI *****END

            Console.ReadLine();
        }

        //STEP 1.1: Get Authentication Access Token *****START
        #region Get an authentication access token
        private static string GetToken()
        {
            // TODO: Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory -Version 2.21.301221612
            // and add using Microsoft.IdentityModel.Clients.ActiveDirectory

            //The client id that Azure AD created when you registered your client app.
            string clientID = "{Client_ID}";

            //RedirectUri you used when you register your app.
            //For a client app, a redirect uri gives Azure AD more details on the application that it will authenticate.
            // You can use this redirect uri for your client app
            string redirectUri = "https://login.live.com/oauth20_desktop.srf";

            //Resource Uri for Power BI API
            string resourceUri = "https://analysis.windows.net/powerbi/api";

            //OAuth2 authority Uri
            string authorityUri = "https://login.windows.net/common/oauth2/authorize";

            //Get access token:
            // To call a Power BI REST operation, create an instance of AuthenticationContext and call AcquireToken
            // AuthenticationContext is part of the Active Directory Authentication Library NuGet package
            // To install the Active Directory Authentication Library NuGet package in Visual Studio,
            //  run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the nuget Package Manager Console.

            // AcquireToken will acquire an Azure access token
            // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint
            AuthenticationContext authContext = new AuthenticationContext(authorityUri);
            string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken;//For version 2.xx.xxxxxxxxx     
            //return authContext.AcquireTokenAsync(resourceUri, clientID, new Uri(redirectUri), new PlatformParameters()).Result.AccessToken;

            return token;
        }
        #endregion
        //STEP 1.1: Get Authentication Access Token *****END

        //STEP 2.2: Create Dataset in Power BI *****START
        #region Create a dataset in Power BI
        private static void CreateDataset()
        {
            //TODO: Add using System.Net and using System.IO
            string powerBIDatasetsApiUrl = "https://api.powerbi.com/v1.0/myorg/datasets";
            //POST web request to create a dataset.
            //To create a Dataset in a group, use the Groups uri: https://api.PowerBI.com/v1.0/myorg/groups/{group_id}/datasets
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIDatasetsApiUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //Create dataset JSON for POST request
            string datasetJson = "{\"name\": \"SalesMarketing\", \"tables\": " +
                "[{\"name\": \"Product\", \"columns\": " +
                "[{ \"name\": \"ProductID\", \"dataType\": \"Int64\"}, " +
                "{ \"name\": \"Name\", \"dataType\": \"string\"}, " +
                "{ \"name\": \"Category\", \"dataType\": \"string\"}," +
                "{ \"name\": \"IsCompete\", \"dataType\": \"bool\"}," +
                "{ \"name\": \"ManufacturedOn\", \"dataType\": \"DateTime\"}" +
                "]}]}";

            //POST web request
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(datasetJson);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);

                var response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine(string.Format("Dataset {0}", response.StatusCode.ToString()));
                //Console.ReadLine();
            }            
        }
        #endregion
        //STEP 2.2: Create Dataset in Power BI *****END

        //STEP 3.2: Get Dataset in Power BI *****START
        #region Get a dataset to add rows into a Power BI table
        private static string GetDataset()
        {
            string powerBIDatasetsApiUrl = "https://api.powerbi.com/v1.0/myorg/datasets";
            //POST web request to create a dataset.
            //To create a Dataset in a group, use the Groups uri: https://api.PowerBI.com/v1.0/myorg/groups/{group_id}/datasets
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIDatasetsApiUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            string datasetId = string.Empty;
            string datasetName = string.Empty;
            //Get HttpWebResponse from GET request
            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();

                    //TODO: Install NuGet Newtonsoft.Json package: Install-Package Newtonsoft.Json
                    //and add using Newtonsoft.Json
                    var results = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    //STEP 3.3: Get Dataset in Power BI *****START
                    //CUSTOMIZED FOR >1 DATASETS
                    //Get the first id
                    //datasetId = results["value"][0]["id"];

                    Console.WriteLine(String.Format("List of Datasets: \n"));
                    foreach (var item in results["value"])
                    {
                        Console.WriteLine("{0} {1} \n", item["id"], item["name"]);
                        if (item["name"]== "SalesMarketing")
                        {
                            datasetId = item["id"];
                            break;
                        }
                    }
                    //STEP 3.3: Get Dataset in Power BI *****END

                    Console.WriteLine(String.Format("Dataset ID: {0}", datasetId));
                    //Console.ReadLine();

                    return datasetId;
                }
            }
        }
        #endregion
        //STEP 3.2: Get Dataset in Power BI *****END

        //STEP 4.2: Add Rows to Table in Dataset in Power BI *****START
        #region Add rows to a Power BI table
        private static void AddRows(string datasetId, string tableName)
        {
            string powerBIApiAddRowsUrl = String.Format("https://api.powerbi.com/v1.0/myorg/datasets/{0}/tables/{1}/rows", datasetId, tableName);

            //POST web request to add rows.
            //To add rows to a dataset in a group, use the Groups uri: https://api.powerbi.com/v1.0/myorg/groups/{group_id}/datasets/{dataset_id}/tables/{table_name}/rows
            //Change request method to "POST"
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIApiAddRowsUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //JSON content for product row
            string rowsJson = "{\"rows\":" +
                "[{\"ProductID\":1,\"Name\":\"Adjustable Race\",\"Category\":\"Components\",\"IsCompete\":true,\"ManufacturedOn\":\"07/30/2014\"}," +
                "{\"ProductID\":2,\"Name\":\"LL Crankarm\",\"Category\":\"Components\",\"IsCompete\":true,\"ManufacturedOn\":\"07/30/2014\"}," +
                "{\"ProductID\":3,\"Name\":\"HL Mountain Frame - Silver\",\"Category\":\"Bikes\",\"IsCompete\":true,\"ManufacturedOn\":\"07/30/2014\"}]}";

            //POST web request
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(rowsJson);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);

                var response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine("Rows Added");
                //Console.ReadLine();
            }
        }
        #endregion
        //STEP 4.2: Add Rows to Table in Dataset in Power BI *****END
    }
}
