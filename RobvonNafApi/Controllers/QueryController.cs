using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using RobvonNafApi.Models;

namespace RobvonNafApi.Controllers
{
	[Route("api/[controller]")] // note use of [controller] attribute
	public class QueryController : Controller
	{
		private static Uri instanceUri = new Uri("http://e4e8d2360842b6d91149453921f4d514.ap-southeast-2.aws.found.io:9200/");
		private static string elasticPath = "robvon_gnaf/addresses/_search";

		#region Http

		[HttpGet()]
		public async Task<IActionResult> GetQuery([FromQuery] string text, [FromQuery]int count, [FromQuery] string format)
		{
			//
			// /query?text=##&count=##&format=json
			//
			if (count == 0) count = 10;
			if (format == null) format = "json";
			if (!string.IsNullOrEmpty(text))
			{
				var listOfAddresses = await GetAddressesFromElastic(text); // perform a 'match' search 
				var listOfQResults = new List<Query>();
				if (listOfAddresses.hits != null && listOfAddresses.hits.hits != null)
				{
					var hits = listOfAddresses.hits.hits;
					foreach (var hit in hits)
					{
						listOfQResults.Add(new Query()
						{
							address_detail_pid = hit._source["address_detail_pid"],
							full_address_line = hit._source["full_address_line"],
							legal_parcel_id = hit._source["legal_parcel_id"],
							postcode = hit._source["postcode"]
						});
					}
				}
				return Ok(listOfQResults);
			}
			return NotFound();
		}
		private async Task<ElasticResult> GetAddressesFromElastic(string text)
		{
			var result = new ElasticResult();
			using (var client = new HttpClient() { BaseAddress = instanceUri })
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				//
				// The match query should be the standard query that you reach for whenever you want
				// to query for a full-text or exact value in almost any field.
				//
				var jObj = new JObject(
					new JProperty("query",
						new JObject(
							new JProperty("match_phrase_prefix",
								new JObject(
									new JProperty("full_address_line", text)
								)
							)
						)
					),
					new JProperty("_source",
						new JArray("full_address_line", "address_detail_pid", "legal_parcel_id", "postcode")
					)
				);
				var querySpec = new StringContent(jObj.ToString());
				var postResp = await client.PostAsync(elasticPath, querySpec);
				if (postResp.IsSuccessStatusCode)
				{
					var jsonString = await postResp.Content.ReadAsStringAsync();
					try
					{
						result = JsonConvert.DeserializeObject<ElasticResult>(jsonString);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}
				}
			}
			return result;
		}

		#endregion
	}

}
