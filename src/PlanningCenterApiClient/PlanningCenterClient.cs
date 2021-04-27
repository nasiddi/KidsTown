﻿using System;
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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using People = KidsTown.PlanningCenterApiClient.Models.PeopleResult.People;

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
            var endPoint = $"people/v2/people?include=households,field_data,phone_numbers&per_page=100&where[id]={string.Join(separator: ',', values: peopleIds)}";
            return await FetchData<People>(endPoint: endPoint).ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public async Task<Household?> GetHousehold(long householdId)
        {
            var endPoint = $"people/v2/households/{householdId}?include=people";
            var households = await FetchData<Household>(endPoint: endPoint).ConfigureAwait(continueOnCapturedContext: false);
            return households.SingleOrDefault();
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
            var response = await Client.GetAsync(requestUri: endPoint);
            var responseString = await response.Content.ReadAsStringAsync();

            await CheckRateLimitation(response: response);

            var responseBody = JsonConvert.DeserializeObject<T>(value: responseString);

            var pages = new List<T> {responseBody};

            while (((IPlanningCenterResponse) responseBody!).NextLink != null)
            {
                var nextResponse = await Client.GetAsync(requestUri: ((IPlanningCenterResponse) responseBody!).NextLink);
                var nextResponseString = await nextResponse.Content.ReadAsStringAsync();

                responseBody = JsonConvert.DeserializeObject<T>(value: nextResponseString);
                pages.Add(item: responseBody);
            }

            return pages.ToImmutableList();
        }

        private static async Task CheckRateLimitation(HttpResponseMessage response)
        {
            var countHeader = response.Headers.SingleOrDefault(predicate: h => h.Key == "X-PCO-API-Request-Rate-Count");
            var limitHeader = response.Headers.SingleOrDefault(predicate: h => h.Key == "X-PCO-API-Request-Rate-Limit");
            var periodHeader = response.Headers.SingleOrDefault(predicate: h => h.Key == "X-PCO-API-Request-Rate-Period");

            var count = ParseHeader(header: countHeader);
            if (count == 0)
            {
                return;
            }
            
            var limit = ParseHeader(header: limitHeader);
            var period = ParseHeader(header: periodHeader);

            if (count >= limit - 5)
            {
                await Task.Delay(millisecondsDelay: period * 250).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        private static int ParseHeader(KeyValuePair<string, IEnumerable<string>>? header)
        {
            return int.TryParse(s: header?.Value.FirstOrDefault(), result: out var number) ? number : 0;
        }
    }
}
