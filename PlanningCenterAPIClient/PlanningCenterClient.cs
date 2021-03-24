using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient.Models;
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

        public async Task<Event> GetActiveEvents()
        {
            var response = await Client.GetStringAsync(requestUri: "check-ins/v2/events?filter=not_archived");
            var responseBody = JsonConvert.DeserializeObject<Event>(value: response);
            return responseBody;
        }
        
        public async Task<ImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack)
        {
            var dateString = DateTime.Today.AddDays(value: -daysLookBack).ToString(format: "yyyy-MM-ddT00:00:00Z");
            var endPoint = $"check-ins/v2/check_ins?include=event,locations,person&order=created_at&per_page=100&where[created_at][gte]={dateString}";
            return await FetchData<CheckIns>(endPoint: endPoint).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<ImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds)
        {
            var endPoint = $"people/v2/people?include=field_data&per_page=100&where[id]={string.Join(separator: ',', values: peopleIds)}";
            return await FetchData<People>(endPoint: endPoint).ConfigureAwait(continueOnCapturedContext: false);
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
        
        private async Task<ImmutableList<T>> FetchData<T>(string endPoint)
        {
            var response = await Client.GetStringAsync(requestUri: endPoint);

            var responseBody = JsonConvert.DeserializeObject<T>(value: response);

            var pages = new List<T> {responseBody};

            while (((IPlanningCenterResponse) responseBody!).NextLink != null)
            {
                var nextResponse = await Client.GetStringAsync(requestUri: ((IPlanningCenterResponse) responseBody!).NextLink);
                responseBody = JsonConvert.DeserializeObject<T>(value: nextResponse);
                pages.Add(item: responseBody);
            }

            return pages.ToImmutableList();
        }
    }
}
