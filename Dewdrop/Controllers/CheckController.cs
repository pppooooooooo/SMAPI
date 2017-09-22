using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Dewdrop.Models;

namespace Dewdrop.Controllers
{
    /// <summary>Provides an API to perform mod update checks.</summary>
    [Produces("application/json")]
    [Route("api/check")]
    public class CheckController : Controller
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="mods">The mods for which to fetch update metadata.</param>
        [HttpPost]
        public async Task<string> Post([FromBody] NexusResponseModel[] mods)
        {
            using (var client = new HttpClient())
            {
                // the return array of mods
                var modList = new List<ModGenericModel>();

                foreach (var mod in mods)
                {
                    try
                    {
                        // create request with HttpRequestMessage
                        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://www.nexusmods.com/stardewvalley/mods/{mod.ID}"));

                        // add the Nexus Client useragent to get JSON response from the site
                        request.Headers.UserAgent.ParseAdd("Nexus Client v0.63.15");

                        // send the request out
                        var response = await client.SendAsync(request);
                        // ensure the response is valid (throws exception)
                        response.EnsureSuccessStatusCode();

                        // get the JSON string of the response
                        var stringResponse = await response.Content.ReadAsStringAsync();

                        // create the mod data from the JSON string
                        var modData = JsonConvert.DeserializeObject<NexusResponseModel>(stringResponse);

                        // add to the list of mods
                        modList.Add(modData.ModInfo());
                    }
                    catch (Exception ex)
                    {
                        var modData = mod.ModInfo();
                        modData.Valid = false;

                        modList.Add(modData);
                    }
                }

                return JsonConvert.SerializeObject(modList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
        }
    }
}