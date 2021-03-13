using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AppointmentMaker.Common.models;
using log4net;
using Newtonsoft.Json;

namespace AppointmentMaker.Common
{
	public class AppointmentChecker : IAppointmentChecker
	{
		private static string appointmentEndpointUrl;

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private HttpClient _reentrantClient;

		public AppointmentChecker()
		{
			_reentrantClient = new HttpClient();

			_reentrantClient.DefaultRequestHeaders
				  .Accept
				  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public static void SetAppointmentEndpointUrl(string endpoint)
		{
			appointmentEndpointUrl = endpoint;
		}

		public async Task<AppointmentResponse> CheckForAppointmentAsync(string content)
		{
			if (!AppointmentEndpointIsSet())
			{
				log.Info("Endpoint was not set.");
				throw new ApplicationException("Endpoint was not set");
			}
			if (string.IsNullOrEmpty(content))
			{
				log.Warn("Body not set for post.");
				return new AppointmentResponse() { success = false };
			}
			try
			{
				var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

				HttpResponseMessage responseBody;
				try
				{
					responseBody = await _reentrantClient.PostAsync(new Uri(appointmentEndpointUrl),
						stringContent);
				}
				catch (Exception ex)
				{
					Console.Write("X");
					return new AppointmentResponse() { Error = new AppointmentError() { Message = ex.Message } };
				}

				string rawResponse = await InterpretReponse(responseBody);
				CheckForErrorsAndIncludeInAppointmentResponse(rawResponse);
				AppointmentResponse appointmentResponse = JsonConvert.DeserializeObject<AppointmentResponse>(rawResponse);
				appointmentResponse.success = true;
				return appointmentResponse;

			}
			catch (HttpRequestException e)
			{
				Console.WriteLine(e);
				return new AppointmentResponse() { success = false };
			}
		}

		private static void CheckForErrorsAndIncludeInAppointmentResponse(string rawResponse)
		{
			if (rawResponse.ToLowerInvariant().Contains("error"))
			{
				try
				{
					var appointmentErrors = JsonConvert.DeserializeObject<AppointmentErrors>(rawResponse);
					var appointmentResponseError = new AppointmentResponse
					{
						Error = appointmentErrors.Errors.FirstOrDefault()
					};
				}
				catch (Exception ex)
				{
					log.Warn("Encountered an error parsing the Error Response.");
					Console.Write("x");
				}
			}
		}

		private static async Task<string> InterpretReponse(HttpResponseMessage responseBody)
		{
			string rawResponse;
			// If the stream is gzipped, decompress the strema before proceeding.
			if (responseBody.Content.Headers.ContentEncoding.ToString().ToLowerInvariant().Equals("gzip"))
			{
				var responseStreamRaw = await responseBody.Content.ReadAsStreamAsync();

				using (var responseStream = new GZipStream(responseStreamRaw, CompressionMode.Decompress))
				{
					using (StreamReader Reader = new StreamReader(responseStream, Encoding.UTF8))
					{
						rawResponse = Reader.ReadToEnd();
					}
				}
			}
			else
			{
				// Stream does not indicate that it is gzipped.
				using (var responseStream = await responseBody.Content.ReadAsStreamAsync())
				{
					using (StreamReader Reader = new StreamReader(responseStream, Encoding.UTF8))
					{
						rawResponse = Reader.ReadToEnd();
					}
				}
			}

			return rawResponse;
		}

		public void SetHeader(string headerKey, string headerValue)
		{
			if (!_reentrantClient.DefaultRequestHeaders.Contains(headerKey))
			{
				_reentrantClient.DefaultRequestHeaders.Add(
					headerKey, headerValue);
			}
		}

		private bool AppointmentEndpointIsSet()
		{
			return !String.IsNullOrWhiteSpace(appointmentEndpointUrl);
		}
	}
}
