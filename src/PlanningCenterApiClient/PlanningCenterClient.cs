using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KidsTown.PlanningCenterApiClient.Models;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using KidsTown.PlanningCenterApiClient.Models.PhoneNumberPatch;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Attributes = KidsTown.PlanningCenterApiClient.Models.PhoneNumberPatch.Attributes;

namespace KidsTown.PlanningCenterApiClient
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

        public async Task<Event?> GetActiveEvents()
        {
            var response = await Client.GetStringAsync("check-ins/v2/events?filter=not_archived");
            var responseBody = JsonConvert.DeserializeObject<Event>(response);
            return responseBody;
        }
        
        public async Task<IImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack)
        {
            var dateString = DateTime.Today.AddDays(-daysLookBack).ToString("yyyy-MM-ddT00:00:00Z");
            var endPoint = $"check-ins/v2/check_ins?include=event,locations,person&order=created_at&per_page=100&where[created_at][gte]={dateString}";
            return await FetchData<CheckIns>(endPoint).ConfigureAwait(false);
        }

        public async Task<IImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds)
        {
            var endPoint = $"people/v2/people?include=households,field_data,phone_numbers&per_page=100&where[id]={string.Join(separator: ',', values: peopleIds)}";
            return await FetchData<People>(endPoint).ConfigureAwait(false);
        }
        
        public async Task<Household?> GetHousehold(long householdId)
        {
            var endPoint = $"people/v2/households/{householdId}?include=people";
            var households = await FetchData<Household>(endPoint).ConfigureAwait(false);
            return households.SingleOrDefault();
        }
        
        public async Task PatchPhoneNumber(long peopleId, long phoneNumberId, string phoneNumber)
        {
            try
            {
                var body = new PhoneNumber
                {
                    Data = new Data
                    {
                        Type = "PhoneNumber",
                        Id = phoneNumberId,
                        Attributes = new Attributes
                        {
                            Number = phoneNumber
                        }
                    }
                };

                var jsonRequest = body.ToJson();
                
                var content = new StringContent(content: jsonRequest, encoding: Encoding.UTF8, mediaType: "application/json-patch+json");

                
                var endPoint = $"people/v2/people/{peopleId}/phone_numbers/{phoneNumberId}";
                await Client.PatchAsync(requestUri: endPoint, content: content);
            }
            catch
            {
                // ignored
            }
        }
        
        public async Task PostPhoneNumber(long peopleId, string phoneNumber)
        {
            try
            {
                var body = new PhoneNumber
                {
                    Data = new Data
                    {
                        Type = "PhoneNumber",
                        Attributes = new Attributes
                        {
                            Number = phoneNumber
                        }
                    }
                };

                var jsonRequest = body.ToJson();
                
                var content = new StringContent(content: jsonRequest, encoding: Encoding.UTF8, mediaType: "application/json-patch+json");

                
                var endPoint = $"people/v2/people/{peopleId}/phone_numbers/";
                await Client.PostAsync(requestUri: endPoint, content: content);
            }
            catch
            {
                // ignored
            }
        }

        private HttpClient InitClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.planningcenteronline.com"),
                Timeout = TimeSpan.FromSeconds(15)
            };

            var authorization = _configuration.GetSection("PlanningCenterClient");
            var byteArray = Encoding.ASCII.GetBytes($"{authorization["ApplicationId"]}:{authorization["Secret"]}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme: "Basic", parameter: Convert.ToBase64String(byteArray));
            _clientBackingField = client;
            return client;
        }
        
        private async Task<IImmutableList<T>> FetchData<T>(string endPoint)
        {
            var response = await Client.GetAsync(endPoint);
            var responseString = await response.Content.ReadAsStringAsync();

            await CheckRateLimitation(response).ConfigureAwait(false);

            var responseBody = JsonConvert.DeserializeObject<T>(responseString);

            var pages = new List<T> {responseBody};

            while (((IPlanningCenterResponse) responseBody!).NextLink != null)
            {
                var nextResponse = await Client.GetAsync(((IPlanningCenterResponse) responseBody!).NextLink);
                var nextResponseString = await nextResponse.Content.ReadAsStringAsync();

                responseBody = JsonConvert.DeserializeObject<T>(nextResponseString);
                pages.Add(responseBody);
            }

            return pages.ToImmutableList();
        }

        private static async Task CheckRateLimitation(HttpResponseMessage response)
        {
            var countHeader = response.Headers.SingleOrDefault(h => h.Key == "X-PCO-API-Request-Rate-Count");
            var limitHeader = response.Headers.SingleOrDefault(h => h.Key == "X-PCO-API-Request-Rate-Limit");
            var periodHeader = response.Headers.SingleOrDefault(h => h.Key == "X-PCO-API-Request-Rate-Period");

            var count = ParseHeader(countHeader);
            if (count == 0)
            {
                return;
            }
            
            var limit = ParseHeader(limitHeader);
            var period = ParseHeader(periodHeader);

            if (count >= limit - 5)
            {
                await Task.Delay(period * 250).ConfigureAwait(false);
            }
        }

        private static int ParseHeader(KeyValuePair<string, IEnumerable<string>>? header)
        {
            return int.TryParse(s: header?.Value?.FirstOrDefault(), result: out var number) ? number : 0;
        }
    }
}
