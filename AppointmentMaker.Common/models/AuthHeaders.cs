namespace AppointmentMaker.Common.models
{
	public class AuthHeaders
	{
		public string cookie { get; set; }
		public string xsrfToken { get; set; }

		public string endpoint { get; set; }
	}
}
