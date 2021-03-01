using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.FieldDefinitionResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;
using Newtonsoft.Json;

namespace CheckInsExtension.PlanningCenterAPIClient
{
    public class PlanningCenterClient : IPlanningCenterClient
    {
#pragma warning disable 649
        private static readonly HttpClient? ClientBackingField;
#pragma warning restore 649

        private HttpClient Client => ClientBackingField ?? InitClient();
        
        public async Task<CheckIns> GetCheckedInPeople()
        {
            //todo fix date filter
            //var dateString = DateTime.UtcNow.ToString("yyyy-MM-ddT00:00:00Z");
            const string dateString = "2021-02-17:00:00:00Z";
            var response = await Client.GetStringAsync($"check-ins/v2/check_ins?include=locations,person&per_page=1000&where[created_at][gte]={dateString}");

            var responseBody = JsonConvert.DeserializeObject<CheckIns>(response);
            return responseBody;
        }

        public async Task<People> GetPeopleUpdates(IImmutableList<long> peopleIds)
        {
            var response = await Client.GetStringAsync($"people/v2/people?include=field_data&per_page=1000&where[id]={string.Join(',', peopleIds)}");
            return JsonConvert.DeserializeObject<People>(response);
        }

        private HttpClient InitClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.planningcenteronline.com")
            };

            var byteArray = Encoding.ASCII.GetBytes("3d62c557b13a539e987562c79349d5580d1df8af5502adc113bbf8ff53990700:9941749180381f27cf3a22a9dfd547a8cc2014a53f9d84d2d853b553a9b22040");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }
    }
}
