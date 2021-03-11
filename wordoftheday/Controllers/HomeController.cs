using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wordoftheday.Models;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace wordoftheday.Controllers
{
    public class HomeController : Controller
    {
        //acts as a variable with more of a global scope so that the word we get in the first api call can be used in the second call
        public static string definitionWord { get; set; }

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //create Word object so that we can assign it once the api's have been called and send it to the index page
            Word theWord = new Word();

            //created new HttpClient which allows us to call an api
            using (var client = new HttpClient())
            {
                //sets the base address that will be used
                client.BaseAddress = new Uri("https://random-word-api.herokuapp.com/");
                //HTTP GET - makes the get request and adds the input on to the end of the base address
                var responseTask = client.GetAsync("word?number=1");
                responseTask.Wait();

                var result = responseTask.Result;

                //if the request is successful then it reads it as a string
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var word = readTask.Result;
                    definitionWord = word.ToString();
                }
                //everything from before is not repeated with a second api call to get a definition of the word from the first api call
                using (var client2 = new HttpClient())
                {
                    client2.BaseAddress = new Uri("https://www.dictionaryapi.com/api/v3/references/collegiate/json/");
                    //HTTP GET

                    var responseTask2 = client2.GetAsync(definitionWord + "?key=1a7c4f1b-fab5-4beb-8fe5-3a0ad860b8a8");
                    responseTask.Wait();


                    var result2 = responseTask2.Result;

                    if (result2.IsSuccessStatusCode)
                    {
                        var readTask = result2.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var words = readTask.Result;

                        Console.WriteLine("words" + words);

                        //converts it into a Json Array
                        JArray json = JArray.Parse((string)words);

                        //goes through each attem in the JArray
                        foreach (JObject parsedObject in json.Children<JObject>())
                        {
                            foreach (JProperty parsedProperty in parsedObject.Properties())
                            {
                                //when it finds the property we are looking for, it stores the values in a Word object
                                string propertyName = parsedProperty.Name;
                                if (propertyName.Equals("shortdef"))
                                {
                                    string propertyValue = (string)parsedProperty.Value[0];
                                    Console.WriteLine("Value: {0}", propertyValue);
                                    theWord.shortdef = propertyValue;
                                    theWord.name = definitionWord;
                                   
                                }
                            }
                        }

                    }
                }
            }
            //we then pass the retrieved things in the object to the view page
            return View(theWord);
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
