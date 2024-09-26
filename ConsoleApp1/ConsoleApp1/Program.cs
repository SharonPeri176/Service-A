using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ApiDataFetcher
{
    internal class Program
    {
        private static int minute;
        private static Timer _fetchTimer;
        private static Timer _averageTimer;
        private static double[] _values = new double[10];
        private static int currIndex=0;

        struct bitcoinApi
        {
            public usdPrice bitcoin;
        }

        struct usdPrice
        {
            public double usd;

            public usdPrice(){
                this.usd=0;
            }
        }


        static async Task Main(string[] args)
        {
            minute= 0;
            _fetchTimer = new Timer(async _ => await FetchData(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _averageTimer = new Timer(_ => PrintAverage(), null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

            // Prevent the application from exiting immediately
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        private static async Task FetchData()
        {
            string apiUrl = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd"; // Replace with your API endpoint
            var data = await FetchDataFromApi(apiUrl);
            
            if (data != null)
            {
                bitcoinApi deserialized = JsonConvert.DeserializeObject<bitcoinApi>(data);
                double value= deserialized.bitcoin.usd;
                minute++;
                Console.WriteLine($"The USD value of Bitcoin in minute {minute} is: {value}");
                if(currIndex==10){
                    currIndex=0;
                }
                _values[currIndex]=value;
                currIndex++;
            }

           else
            {
                Console.WriteLine("Could not parse the value.");
            }
            
        }

        private static async Task<string> FetchDataFromApi(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    return e.Message;
                }
            }
            
        }

        private static void PrintAverage()
        {

            double average = 0;
            for(int i = 0; i < 10; i++)
            {
                average = average + _values[i];
            }
            average = average / 10;

            Console.WriteLine($"Average value over the last 10 minutes: {average}");

        }
    }
}

