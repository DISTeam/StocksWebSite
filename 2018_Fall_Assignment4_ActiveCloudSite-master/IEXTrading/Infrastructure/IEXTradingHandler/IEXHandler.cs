using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using IEXTrading.Models;
using Newtonsoft.Json;

namespace IEXTrading.Infrastructure.IEXTradingHandler
{
    public class IEXHandler
    {
        static string BASE_URL = "https://api.iextrading.com/1.0/"; //This is the base URL, method specific URL is appended to this.
        HttpClient httpClient;

        public IEXHandler()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /****
         * Calls the IEX reference API to get the list of symbols. 
        ****/
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_PATH = BASE_URL + "ref-data/symbols";
            string companyList = "";

            List<Company> companies = null;

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!companyList.Equals(""))
            {
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
            }
            return companies;
        }
        public List<Stats> GetStats(List<Company> companies)
        {
            string symbols = "";
            Dictionary<string, Dictionary<string, Stats>> statsDict = null;
            List<Stats> statsList = new List<Stats>();
            int start = 0;
            int end = 100;
            int iter = 100;
            List<Company> setCompanies = null;
            while (end <= companies.Count)
            {
                int count = 0;
                symbols = "";
                setCompanies = new List<Company>();
                setCompanies = companies.GetRange(start,iter);
                foreach (var company in setCompanies)
                {
                    count++;
                    symbols = symbols + company.symbol + ",";
                }


                string IEXTrading_API_PATH = BASE_URL + "stock/market/batch?symbols=" + symbols + "&types=quote&filter=symbol,companyName,close,week52High,week52Low";
                string statsResponse = "";
                statsDict = new Dictionary<string, Dictionary<string, Stats>>();

                HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    statsResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                
                if (!string.IsNullOrEmpty(statsResponse))
                {
                    statsDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Stats>>>(statsResponse);
                }

                foreach (var item in statsDict)
                {

                    foreach (var i in item.Value)
                    {
                        if (i.Value != null)
                        {
                            statsList.Add(i.Value);
                        }
                    }
                }


                start = end;
                end = end + 100;
                if (end > companies.Count)
                {
                    iter = end - companies.Count;
                }
            }
            return statsList;
    }
    
        /****
         * Calls the IEX stock API to get 1 year's chart for the supplied symbol. 
        ****/
        public List<Equity> GetChart(string symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/batch?types=chart&range=1y";
            string charts = "";
            List<Equity> Equities = new List<Equity>();
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                charts = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            if (!charts.Equals(""))
            {
                ChartRoot root = JsonConvert.DeserializeObject<ChartRoot>(charts, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Equities = root.chart.ToList();
            }
            //make sure to add the symbol the chart
            foreach (Equity Equity in Equities)
            {
                Equity.symbol = symbol;
            }

            return Equities;
        }
        public List<outputModel> output(List<Company> companies)
        {
            List<Stats> stats = new List<Stats>();
            List<outputModel> statsResult = new List<outputModel>();
            outputModel value = null;
            stats = GetStats(companies);
            List<outputModel> statsList = new List<outputModel>();
            foreach (var i in stats)
            {
                value = new outputModel();
                value.symbol = i.symbol;
                value.companyName = i.companyName;
                value.close = i.close;
                if ((i.week52High - i.week52Low) != 0)
                {
                    value.value = ((i.close - i.week52Low) / (i.week52High - i.week52Low));
                }
                statsList.Add(value);
                statsResult = statsList.Where(a => a.value > 0.82).ToList();
            }
            return statsList.OrderByDescending(a => a.value).Take(5).ToList();
        }
    }
}
