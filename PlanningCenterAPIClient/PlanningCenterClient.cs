using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.EventResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CheckInsExtension.PlanningCenterAPIClient
{
    public class PlanningCenterClient : IPlanningCenterClient
    {
        private readonly IConfiguration _configuration;
        private HttpClient? _clientBackingField;

        public PlanningCenterClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private HttpClient Client => _clientBackingField ?? InitClient();

        public async Task<EventResult> GetActiveEvents()
        {
            var response = await Client.GetStringAsync(requestUri: "check-ins/v2/events?filter=not_archived");
            var responseBody = JsonConvert.DeserializeObject<EventResult>(value: response);
            return responseBody;
        }
        
        public async Task<CheckIns> GetCheckedInPeople(int daysLookBack)
        {
            var dateString = DateTime.Today.AddDays(value: -daysLookBack).ToString(format: "yyyy-MM-ddT00:00:00Z");
            var response = await Client.GetStringAsync(requestUri: $"check-ins/v2/check_ins?include=locations,person,event&per_page=1000&where[created_at][gte]={dateString}");

            var responseBody = JsonConvert.DeserializeObject<CheckIns>(value: response);
            return responseBody;
        }

        public async Task<People> GetPeopleUpdates(IImmutableList<long> peopleIds)
        {
            var response = await Client.GetStringAsync(requestUri: $"people/v2/people?include=field_data&per_page=1000&where[id]={string.Join(separator: ',', values: peopleIds)}");
            return JsonConvert.DeserializeObject<People>(value: response);
        }

        private HttpClient InitClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(uriString: "https://api.planningcenteronline.com")
            };

            var authorization = _configuration.GetSection(key: "PlanningCenterClient");
            var byteArray = Encoding.ASCII.GetBytes(s: $"{authorization[key: "ApplicationId"]}:{authorization[key: "Secret"]}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme: "Basic", parameter: Convert.ToBase64String(inArray: byteArray));
            _clientBackingField = client;
            return client;
        }
    }
}
