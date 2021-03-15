using Newtonsoft.Json;

namespace AppointmentMaker.Common.models
{
	public class AppointmentRequest
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string serviceId { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Position position { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AppointmentAvailability appointmentAvailability { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int radius { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		private string zipCode;

		public void Build(string serviceId, double? latitude, double? longitude, string startDateTime , int radius)
		{
			this.serviceId = serviceId;
			this.radius = radius;
			if (latitude.HasValue && longitude.HasValue)
			{
				this.position = new Position() { Latitude = latitude.Value, Longitude = longitude.Value };
			}
			this.appointmentAvailability = new AppointmentAvailability() { startDateTime = startDateTime };
		}

		public void Build(string serviceId, string zipCode, string startDateTime, int radius)
		{
			this.radius = radius;
			this.zipCode = zipCode;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
