namespace AppointmentMaker.Common.models
{
	public class Configuration
	{
		public string cookie { get; set; }
		public string xsrfToken { get; set; }
		public string endpoint { get; set; }
		public string awsAccessKey { get; set; }
		public string awsSecretKey { get; set; }
		public string bucket { get; set; }
		public string zipFilePath { get; set; }
	}
}
