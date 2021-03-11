using Newtonsoft.Json;

namespace AppointmentMaker.Common.models
{
	public class AppointmentError
	{
		[JsonProperty("code")]
		public string Code { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
