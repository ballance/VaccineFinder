using Newtonsoft.Json;

namespace AppointmentMaker.Common.models
{
	public class AppointmentErrors
	{
		[JsonProperty("errors")]
		public AppointmentError[] Errors { get; set; }
	}
}
