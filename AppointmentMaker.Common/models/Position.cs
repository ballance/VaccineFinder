using Newtonsoft.Json;

namespace AppointmentMaker.Common.models
{
	public class Position
	{
		[JsonProperty("latitude")]
		public double Latitude { get; set; }
		[JsonProperty("longitude")]
		public double Longitude { get; set; }	
		[JsonProperty("zipCode")]
		public string ZipCode { get; set; }
	}
}
