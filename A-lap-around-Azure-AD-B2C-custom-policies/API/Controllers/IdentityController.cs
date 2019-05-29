using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using B2CAPIWebApplication.Models;
using Newtonsoft.Json;

namespace B2CAPIWebApplication.Controllers
{
    public class IdentityController : ApiController
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        public IHttpActionResult CheckPassword(string password)
        {
            string hashSH1 = Hash(password);
            string hashSH1FirstFive = hashSH1.Substring(0, 5);
            string hashSH1Rest = hashSH1.Substring(5, hashSH1.Length - 5);

            string responseFromServer = "";

            string url = "https://api.pwnedpasswords.com/range/" + hashSH1FirstFive;

            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(url);

            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            string status = ((HttpWebResponse)response).StatusDescription;

            // Get the stream containing content returned by the server. 
            // The using block ensures the stream is automatically closed. 
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                responseFromServer = reader.ReadToEnd();
                // Display the content.  
                // Console.WriteLine(responseFromServer);
            }

            // Close the response.  
            response.Close();

            var index = responseFromServer.IndexOf(hashSH1Rest);

            if (index > 0)
            {
                index = index + 36;
                var indexCount = responseFromServer.IndexOf("\r", index);
                var indexLength = indexCount - index;
                var passwordCount = responseFromServer.Substring(index, indexLength);

                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Oh no — pwned! " +
                    "This password has been seen " + passwordCount + " times before", HttpStatusCode.Conflict));
            }
            else
            {
                OutputClaimsModel outputClaims = new OutputClaimsModel();
                outputClaims.passwordPwned = false;
                return Ok(outputClaims);
            }
        }

        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }        
    }
}

