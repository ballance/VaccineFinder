using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AppointmentMaker.Common;
using AppointmentMaker.Common.models;
using Newtonsoft.Json;

namespace AppointmentMaker.Runer
{
	public class Runner : IRunner
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public List<AppointmentResponse> _successfulChecks = new List<AppointmentResponse>();
		public List<AppointmentResponse> _failedChecks = new List<AppointmentResponse>();
		public int daysAheadToLook = 0;
		private string _zipConfigPath;
		private string _csvConfigPath;
		private Stopwatch stopWatch;
		private IAppointmentChecker _appointmentChecker;
		private IPublishingManager _publishingManager;
		private IInputManager _inputManager;
		private Configuration _configuration;

		public Runner(IAppointmentChecker appointmentChecker
			, IPublishingManager publishingManager
			, IInputManager inputManager)
		{
			var configTextRaw = File.ReadAllText(@"./config/configuration.json");
			_configuration = JsonConvert.DeserializeObject<Configuration>(configTextRaw);
			_zipConfigPath = _configuration.zipFilePath;
			_csvConfigPath = _configuration.csvFilePath;
			stopWatch = new Stopwatch();
			stopWatch.Start();

			_appointmentChecker = appointmentChecker;
			_publishingManager = publishingManager;
			_inputManager = inputManager;

			AppointmentChecker.SetAppointmentEndpointUrl(_configuration.endpoint);

			_appointmentChecker.SetHeader("accept-language", "en-US,en;q=0.9");
			_appointmentChecker.SetHeader("cookie", _configuration.cookie);
			_appointmentChecker.SetHeader("x-xsrf-token", _configuration.xsrfToken);
		}
		public async Task Run()
		{
			// TODO: This is super-hacky, but should work for continuous run.
			while (true)
			{
				await SingleRun();
			}
		}

		private async Task SingleRun()
		{
			List<Task> tasksToAwait = new List<Task>();

			List<string> zipCodesToTry = _inputManager.LoadZipsFromFile(_zipConfigPath);

			List<ZipWithCoordinates> zipCodesToTryWithMeta = _inputManager.LoadZipsFromCSV(_csvConfigPath);

			foreach (var zipWithMeta in zipCodesToTryWithMeta.Distinct())
			{
				var startDate = DateTime.Now.AddDays(1);
				for (int j = 0; j <= daysAheadToLook; j++)
				{
					var task = CheckSingleAppointment(zipWithMeta.Latitude, 
						zipWithMeta.Longitude, startDate.AddDays(j), zipWithMeta.Zip, zipWithMeta.LocationText);
					tasksToAwait.Add(task);
				}
			}
			Task.WaitAll(tasksToAwait.ToArray());
			stopWatch.Stop();

			Console.WriteLine();
			Console.WriteLine($"Time elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");
			log.Info($"Time elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");
			Console.WriteLine();

			log.Info($"successes: {_successfulChecks.Count}");
			Console.WriteLine($"successes: {_successfulChecks.Count}");

			log.Info($"failures: {_failedChecks.Count}");
			Console.WriteLine($"failures: {_failedChecks.Count}");

			Console.WriteLine();
			log.Info($"Success Rate: {Math.Round((double)_failedChecks.Count / (_failedChecks.Count + _successfulChecks.Count), 2)}%");
			Console.WriteLine($"Success Rate: {Math.Round((double)_failedChecks.Count / (_failedChecks.Count + _successfulChecks.Count), 2)}%");
			Console.WriteLine();

			var now = DateTime.Now;
			var csvFilename = $"currentZipAppointmentsAvailable_{now.Hour}_{now.Minute}_{now.Second}.csv";
			StringBuilder csvData = VaccineDataGenerator.GenerateCSV(_successfulChecks);
			StringBuilder defaultCSS = VaccineDataGenerator.CreateDefaultCSS();
			StringBuilder htmlData = VaccineDataGenerator.GenerateHTML(_successfulChecks, csvFilename);
			StringBuilder mapHtml = VaccineDataGenerator.GenerateMap(_successfulChecks, _configuration.googleApiKey);


			_publishingManager.Configure(_configuration.bucket, _configuration.awsAccessKey, _configuration.awsSecretKey);
			await _publishingManager.PublishAsync(csvData.ToString()
				, csvFilename);

			await _publishingManager.PublishAsync(htmlData.ToString(), "index2.html");
			await _publishingManager.PublishAsync(defaultCSS.ToString(), "default.css");
			await _publishingManager.PublishAsync(mapHtml.ToString(), "map.html");

			File.WriteAllText(
				$"C:\\temp\\currentZipAppointmentsAvailable_{now.Hour}_{now.Minute}_{now.Second}.csv"
				, csvData.ToString());

			_successfulChecks.Clear();
			_failedChecks.Clear();

		}

		

		private async Task CheckSingleAppointment(double? latitude, double? longitude, DateTime startDate, string zipCode, string locationText)
		{
			AppointmentResponse result = new AppointmentResponse() { success = false };

			try
			{
				var appointmentRequest = new AppointmentRequest();

				//if (string.IsNullOrEmpty(zipCode))
				//{
				//	appointmentRequest.Build("99", latitude, longitude, startDate.ToString("yyyy-MM-dd"), 25);
				//}
				//else
				//{
					appointmentRequest.Build("99", zipCode, startDate.ToString("yyyy-MM-dd"), 25);
				//}
				result = await _appointmentChecker.CheckForAppointmentAsync(appointmentRequest.ToString());
				result = AddLatLongIfAvailable(result, latitude, longitude);
				result.stateName = locationText;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			if (result.success == false || result.appointmentsAvailable == false)
			{
				_failedChecks.Add(result);
				Console.Write(".");
			}
			else
			{
				_successfulChecks.Add(result);
				Console.Write($"«{result.zipCode}/{result.stateCode}»");
			}
		}

		private AppointmentResponse AddLatLongIfAvailable(AppointmentResponse response, double? latitude, double? longitude)
		{
			if (latitude != null)
			{
				response.Latitude = latitude.Value;
			}
			if (longitude != null)
			{
				response.Longitude = latitude.Value;
			}
			return response;
		}
	}
}
