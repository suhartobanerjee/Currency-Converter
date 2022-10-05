using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace Currency_Data
{
    public class GetExchangeRates
    {
        static readonly HttpClient client = new HttpClient();
        static double prgs = 0;

        // Initialising the exRates Dictionary
        public static Dictionary<string, double> exRates = new Dictionary<string, double>()
        {
            { "INR-EUR", 0.0 },
            { "INR-USD", 0.0 },
            { "EUR-USD", 0.0 },
        };


        public static async Task<Dictionary<string, double>> Main_Exchange(IProgress<double> progress)
        {
            prgs = 0;
            GetExchangeRates xChange = new GetExchangeRates();
            progress.Report(prgs += 25);



            List<Task> task = new List<Task>();

            foreach (var curr in exRates)
            {
                task.Add(xChange.GetData(curr: curr.Key, progress: progress));
                progress.Report(prgs += 5);
            }

            await Task.WhenAll(task);

            return exRates;
        }


        public async Task GetData(string curr, IProgress<double> progress)
        {

            string baseLink = "https://www.google.com/finance/quote/";
            string url = baseLink + curr;
            string page = "";
            const int maxRetries = 3;


            for (int i = 0; i < maxRetries; i++)
            {

                try
                {
                    using HttpResponseMessage response = await client.GetAsync(url);
                    {
                        response.EnsureSuccessStatusCode();
                        page = await response.Content.ReadAsStringAsync();
                    }

                    if (page.Length <= 0)
                    {
                        continue;
                    }
                }
                catch (HttpRequestException e)
                {
                    continue;
                }
            }

            // If the page is not downloaded, keep the rates as 0.0
            if (!(page.Length <= 0))
            {
                exRates[curr] = HtmlParse(page: page);
                progress.Report(prgs += 20);
            }
        }


        public double HtmlParse(string page)
        {

            string pat = @"<div\sclass=\WYMlKec\sfxKbKc\W>(\d\.\d{2,6})<\/div>";
            Regex r = new Regex(pat);
            double _ExRate = 0.0;

            Match m = r.Match(page);

            _ExRate = Convert.ToDouble(m.Groups[1].Value);


            return _ExRate;
        }

    }
}
