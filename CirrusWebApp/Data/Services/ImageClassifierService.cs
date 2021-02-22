using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Threading.Tasks;
using CirrusWebApp.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;

namespace CirrusWebApp.Data.Services
{
    public class ImageClassifierService
    {
        public async Task<List<string>> InvokeRequestResponseService(IBrowserFile File)
        {
            List<string> TagList = new();
            var handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
            };
            using (var MS = new MemoryStream())
            {
                await File.OpenReadStream(10485760).CopyToAsync(MS);
                MS.Position = 0;
                string base64ImageData = Convert.ToBase64String(MS.ToArray());
                using (var client = new HttpClient(handler))
                {
                    var scoreRequest = new
                    {
                        Inputs = new Dictionary<string, List<Dictionary<string, string>>>()
                    {
                        {
                            "WebServiceInput0",
                            new List<Dictionary<string, string>>()
                            {
                                new Dictionary<string, string>()
                                    {
                                    {
                                    "image", "data:image/png;base64," + base64ImageData
                                    },
                                    {
                                        "id", "0"
                                    },
                                    {
                                        "category", ""
                                    },
                            }
                        }
                    },
                },
                        GlobalParameters = new Dictionary<string, string>()
                        {
                        }
                    };


                    const string apiKey = "W1DzhcvAGLSqNzc1I2I2jvtQb1qGnbwO"; // Replace this with the API key for the web service
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    client.BaseAddress = new Uri("http://20.76.24.174:80/api/v1/service/imagemlendpoint/score");

                    // WARNING: The 'await' statement below can result in a deadlock
                    // if you are calling this code from the UI thread of an ASP.Net application.
                    // One way to address this would be to call ConfigureAwait(false)
                    // so that the execution does not attempt to resume on the original context.
                    // For instance, replace code such as:
                    //      result = await DoSomeTask()
                    // with the following:
                    //      result = await DoSomeTask().ConfigureAwait(false)

                    var requestString = JsonConvert.SerializeObject(scoreRequest);
                    var content = new StringContent(requestString);

                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    HttpResponseMessage response = await client.PostAsync("", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        ImageResponse resultObj = JsonConvert.DeserializeObject<ImageResponse>(result);
                        Console.WriteLine("Normalizing score list");
                        resultObj.Response.SetScoreValues();
                        Console.WriteLine("Fetching top 5 result");
                        var top5Probabilities = (from entry in resultObj.Response.ScoreValues orderby entry.Value descending select entry).Take(5);
                        TagList = top5Probabilities.ToDictionary(k => k.Key, v => v.Value).Keys.ToList();
                    }
                    else
                    {
                        Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                        // Print the headers - they include the requert ID and the timestamp,
                        // which are useful for debugging the failure
                        Console.WriteLine(response.Headers.ToString());

                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseContent);
                    }
                }
            }
            return TagList;
        }
    }
}
