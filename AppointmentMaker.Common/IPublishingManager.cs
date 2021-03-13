using System.Threading.Tasks;

namespace AppointmentMaker.Common
{
	public interface IPublishingManager
	{
		void Configure(string bucket, string accessKey, string secretKey);

		Task<bool> PublishAsync(string inputData, string outputName);
	}
}