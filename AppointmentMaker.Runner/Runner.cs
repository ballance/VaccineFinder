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
		private Stopwatch stopWatch;
		private IAppointmentChecker _appointmentChecker;
		private IPublishingManager _publishingManager;
		private Configuration _configuration;

		public Runner(IAppointmentChecker appointmentChecker, IPublishingManager publishingManager)
		{
			var configTextRaw = File.ReadAllText(@"./config/configuration.json");
			_configuration = JsonConvert.DeserializeObject<Configuration>(configTextRaw);
			_zipConfigPath = _configuration.zipFilePath;
			stopWatch = new Stopwatch();
			stopWatch.Start();

			_appointmentChecker = appointmentChecker;
			_publishingManager = publishingManager;
			AppointmentChecker.SetAppointmentEndpointUrl(_configuration.endpoint);

			_appointmentChecker.SetHeader("accept-language", "en-US,en;q=0.9");
			_appointmentChecker.SetHeader("cookie", _configuration.cookie);
			_appointmentChecker.SetHeader("x-xsrf-token", _configuration.xsrfToken);
		}
		public async Task Run()
		{
			List<Task> tasksToAwait = new List<Task>();

			List<string> zipCodesToTry = LoadZipsFromFile(_zipConfigPath);

			foreach (string zip in zipCodesToTry.Distinct())
			{
				var startDate = DateTime.Now.AddDays(1);
				for (int j = 0; j <= daysAheadToLook; j++)
				{
					var task = CheckSingleAppointment(0, 0, startDate.AddDays(j), zip);
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
			StringBuilder csvData = GenerateCSV(_successfulChecks);
			StringBuilder defaultCSS = CreateDefaultCSS();
			StringBuilder htmlData = GenerateHTML(_successfulChecks, csvFilename);



			
			_publishingManager.Configure(_configuration.bucket, _configuration.awsAccessKey, _configuration.awsSecretKey);
			await _publishingManager.PublishAsync(csvData.ToString()
				, csvFilename);

			await _publishingManager.PublishAsync(htmlData.ToString(), "index.html");
			await _publishingManager.PublishAsync(defaultCSS.ToString(), "default.css");


			File.WriteAllText(
				$"C:\\temp\\currentZipAppointmentsAvailable_{now.Hour}_{now.Minute}_{now.Second}.csv"
				, csvData.ToString());
		}

		private StringBuilder GenerateCSV(IEnumerable<AppointmentResponse> successfulChecks)
		{
			StringBuilder zipos = new StringBuilder();
			zipos.AppendLine("zip,state,map");
			foreach (var success in successfulChecks)
			{
				log.Info($"{success.zipCode},{success.stateName}");
				zipos.Append($"{success.zipCode},{success.stateName},https://www.google.com/maps/place/{success.zipCode}/{Environment.NewLine}");
			}

			return zipos;
		}

		private StringBuilder GenerateHTML(IEnumerable<AppointmentResponse> successfulChecks, string csvFilename)
		{
			StringBuilder htmlStringBuilder = new StringBuilder();
			htmlStringBuilder.Append("<html><header>")
				.Append($"<title>Vaccination appointments available by ZIP Code at {DateTime.UtcNow} UTC. </title>")
				.Append("<link rel='stylesheet' href='default.css'>")
				.Append("</header><body>")
				.Append("<h2>Walgreens appointments available by ZIP Code<h2>")
				.Append($"<p>Last updated: {DateTime.UtcNow} UTC.<br />")
				.Append($"<a href='{csvFilename}'>Download as CSV</a></p>")
				.Append("<table><th>zip</th><th>State</th><th>Map</th><th>Appointment Link</th>");
			foreach (var success in successfulChecks)
			{
				htmlStringBuilder
				.Append("<tr>")
				.Append($"<td>{success.zipCode}</td>")
				.Append($"<td>{success.stateName}</td>")
				.Append($"<td><a target='_blank' href='")
				.Append($"https://www.google.com/maps/place/{success.zipCode},%20{success.stateCode},%20USA")
				.Append($"'>{success.zipCode}</a></td>")
				.Append($"<td><a href='https://www.walgreens.com/findcare/vaccination/covid-19/location-screening'>Book Appointment</td>");
			}
			htmlStringBuilder
				.Append("</tr>")
				.Append("</table>")
				.Append("</body></html>");
			return htmlStringBuilder;
		}

		private static StringBuilder CreateDefaultCSS()
		{
			var cssBuilder = new StringBuilder();
			cssBuilder.AppendLine("tbody tr:nth-child(odd) { background-color: #ccc;}");
			cssBuilder.AppendLine("table { width:50%; }");
			cssBuilder.AppendLine(@"td 
{
			height: 50px;
			width: 50px;
			}

			td 
			{
				text-align: center;
				vertical-align: middle;
			}
			");
	 		return cssBuilder;
		}

		private static StringBuilder CreateIndexPage(string successObjectName)
		{
			var indexContentbBuilder = new StringBuilder();
			indexContentbBuilder.Append("<html><header><title>Available Vaccination Appointments List as of ")
				.Append($"{DateTime.UtcNow} </title>")
				.Append("</header><body>")
				.Append("<ul>")
				.Append($"<a href='")
				.Append(successObjectName)
				.Append("'>Available Appointments List as of ")
				.Append(DateTime.UtcNow)
				.Append(" UTC</a>")
				.Append("</ul>")
				.Append("</body></html>");
			return indexContentbBuilder;
		}

		private List<string> LoadZipsFromFile(string logPath)
		{
			try
			{
				var logFile = File.ReadAllLines(logPath);
				return new List<string>(logFile);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private async Task CheckSingleAppointment(double latitude, double longitude, DateTime startDate, string zipCode = "")
		{
			AppointmentResponse result = new AppointmentResponse() { success = false };

			try
			{
				var appointmentRequest = new AppointmentRequest();

				if (string.IsNullOrEmpty(zipCode))
				{
					appointmentRequest.Build("99", latitude, longitude, startDate.ToString("yyyy-MM-dd"), 25);
				}
				else
				{
					appointmentRequest.Build("99", zipCode, startDate.ToString("yyyy-MM-dd"), 25);
				}
				result = await _appointmentChecker.CheckForAppointmentAsync(appointmentRequest.ToString());
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
	}
}
