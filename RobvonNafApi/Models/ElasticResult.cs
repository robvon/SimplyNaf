using System.Collections.Generic;
namespace RobvonNafApi.Models
{
	public class ElasticResult
	{
		public int took { get; set; }
		public string timed_out { get; set; }
		public object _shards { get; set; }
		public ElasticHits hits { get; set; }
	}
	public class ElasticHits
	{
		public int total { get; set; }
		public string max_score { get; set; }
		public ElasticHit[] hits { get; set; }
	}
	public class ElasticHit
	{
		public string _index { get; set; }
		public string _type { get; set; }
		public string _id { get; set; }
		public object _score { get; set; }
		public Dictionary<string, string> _source { get; set; }
	}
	public class Suggestion
	{
		public string id { get; set; }
		public string text { get; set; }
	}
	public class Query
	{
		public string address_detail_pid { get; set; }
		public string full_address_line { get; set; }
		public string legal_parcel_id { get; set; }
		public string postcode { get; set; }
	}
}
