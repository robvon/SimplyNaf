using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimplyNaf
{
	class Program
	{
		private static Uri instanceUri = new Uri("http://e4e8d2360842b6d91149453921f4d514.ap-southeast-2.aws.found.io:9200/");
		private static string nafFile = @"\T2_Json_GNAF\brisbane_cbd_area.json";
		static void Main() { RunAsync().Wait(); }
		static async Task RunAsync()
		{

			using (var client = new HttpClient())
			{
				//
				// prepare the client
				//
				client.BaseAddress = Program.instanceUri;
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				//
				// see if our instance is out there...
				// we dont get exceptions on errors - only status codes. Otherwise call "EnsureSuccessStatusCode()"
				//
				var response = await client.GetAsync("");
				if (response.IsSuccessStatusCode) 
				{
					var address = await response.Content.ReadAsStringAsync();
					Console.WriteLine("The instance exists");
				}
				else
				{
					Console.WriteLine("Failed. Status: [" + response.StatusCode + "]");
				}
				//
				// get a stream with the JSON for the NAF file with addresses
				//
				using (var f = new System.IO.StreamReader(Environment.CurrentDirectory + Program.nafFile))
				{
					Console.WriteLine("The JSON file: [" + Program.nafFile + "] exists");
					string line;
					var lCount = 0;
					var template = "{ \"create\": { \"_id\":\"[id]\"} }"; // if the id exists, the post will error but carry on
					var block = "";
					while ((line = f.ReadLine()) != null)
					{
						Console.WriteLine(lCount.ToString().PadLeft(6) + ". Blocking ID: " + line.Substring(8, 16));
						lCount++;
						//if (lCount > 100) break;
						//
						// setup each line in the bulk load format - two lines for each input line with \n on the back
						//
						var id = line.Substring(9, 14); // presume this is a constant. Don't want to parse the line
						var jdata = "{" + line.Substring(25);
						block += template.Replace("[id]", id) + "\n";
						block += jdata + "\n";
						if (lCount % 100 == 0)
						{
							//
							// post the block and start again
							//
							var content = new StringContent(block);
							var postResp = await client.PostAsync("robvon_gnaf/addresses/_bulk", content);
							if (postResp.IsSuccessStatusCode)
							{
								Console.WriteLine("   Block Created");
							}
							else
							{
								Console.WriteLine("  Block Failed. Status: [" + response.StatusCode + "]");
							}
							block = "";
						}
					}
					if (block != "")
					{
						var content = new StringContent(block);
						var postResp = await client.PostAsync("robvon_gnaf/addresses/_bulk", content);
						Console.WriteLine("   Block Created (FINAL)");
					} else
					{
						Console.WriteLine("   No FINAL block");
					}
				}

				//// HTTP POST
				//var gizmo = new Product() { Name = "Gizmo", Price = 100, Category = "Widget" };
				//response = await client.PostAsJsonAsync("api/products", gizmo);
				//if (response.IsSuccessStatusCode)
				//{
				//	Uri gizmoUrl = response.Headers.Location;

				//	// HTTP PUT
				//	gizmo.Price = 80;   // Update price
				//	response = await client.PutAsJsonAsync(gizmoUrl, gizmo);

				//	// HTTP DELETE
				//	response = await client.DeleteAsync(gizmoUrl);
				//}
			}
			Console.Read();
		}
	}
}
