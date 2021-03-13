using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace AppointmentMaker.Common
{
	public class PublishingManager : IPublishingManager
	{
		private string bucket;
		private IAmazonS3 _s3Client;

		public void Configure(string bucket, string accessKey, string secretKey)
		{
			try
			{
				this.bucket = bucket;

				var creds = new BasicAWSCredentials(accessKey, secretKey);
				var s3Config = new AmazonS3Config();
				s3Config.RegionEndpoint = RegionEndpoint.USEast1;
				_s3Client = new AmazonS3Client(creds, s3Config);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public async Task<bool> PublishAsync(string inputData, string outputName)
		{
			try
			{
				byte[] byteArray = Encoding.UTF8.GetBytes(inputData);

				using (MemoryStream stream = new MemoryStream(byteArray))
				{
					var putRequest = new PutObjectRequest
					{
						InputStream = stream,
						Key = outputName,
						BucketName = bucket,
						CannedACL = S3CannedACL.PublicRead
					};

					var result = await _s3Client.PutObjectAsync(putRequest);
					return (result.HttpStatusCode.Equals("OK"));
				}

				return false;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
