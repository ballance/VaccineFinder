using Newtonsoft.Json;

namespace AppointmentMaker.Common.models
{
	public class AppointmentResponse
	{
		public bool appointmentsAvailable { get; set; }
		public string stateName { get; set; }
		public string stateCode { get; set; }
		public string zipCode { get; set; }
		public int radius { get; set; }
		public int days { get; set; }

		public bool success { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AppointmentError Error { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
