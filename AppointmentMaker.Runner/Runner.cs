using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentMaker.Common;
using AppointmentMaker.Common.models;
using Newtonsoft.Json;

namespace AppointmentMaker.Runer
{
	public class Runner
	{
		public List<AppointmentResponse> successfulChecks = new List<AppointmentResponse>();
		public List<AppointmentResponse> failedChecks = new List<AppointmentResponse>();
		public int daysAheadToLook = 0;
		private Stopwatch stopWatch;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private AppointmentChecker appointmentChecker;

		public Runner()
		{
			var headerTextRaw = File.ReadAllText(@"./config/headers.json");
			AuthHeaders authHeaders = JsonConvert.DeserializeObject<AuthHeaders>(headerTextRaw);
			stopWatch = new Stopwatch();
			stopWatch.Start();

			appointmentChecker = new AppointmentChecker();
			AppointmentChecker.SetAppointmentEndpointUrl(authHeaders.endpoint);

			appointmentChecker.SetHeader("accept-language", "en-US,en;q=0.9");
			appointmentChecker.SetHeader("cookie", authHeaders.cookie);
			appointmentChecker.SetHeader("x-xsrf-token", authHeaders.xsrfToken);
		}
		public async Task Run()
		{
			List<Task> tasksToAwait = new List<Task>();

			List<string> zipCodesToTry = LoadZipsFromFile("./config/zipsToSearchUSA20000.txt");
   		
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
			
			log.Info($"successes: {successfulChecks.Count}");
			Console.WriteLine($"successes: {successfulChecks.Count}");
			
			log.Info($"failures: {failedChecks.Count}");
			Console.WriteLine($"failures: {failedChecks.Count}");
			
			Console.WriteLine();
			log.Info($"Success Rate: {Math.Round((double)failedChecks.Count/(failedChecks.Count+successfulChecks.Count),2)}%");
			Console.WriteLine($"Success Rate: {Math.Round((double)failedChecks.Count / (failedChecks.Count + successfulChecks.Count), 2)}%");
			Console.WriteLine();
				
			StringBuilder zipos = new StringBuilder();
			zipos.AppendLine("zip,state,map");
			foreach (var success in successfulChecks)
			{
				log.Info($"{success.zipCode},{success.stateName}");
				zipos.Append($"{success.zipCode},{success.stateName},https://www.google.com/maps/place/{success.zipCode}/{Environment.NewLine}");
			}

			var now = DateTime.Now;
			File.WriteAllText(
				$"C:\\temp\\currentZipAppointmentsAvailable_{now.Hour}_{now.Minute}_{now.Second}.csv"
				, zipos.ToString());
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
				result = await appointmentChecker.CheckForAppointmentAsync(appointmentRequest.ToString());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			if (result.success == false || result.appointmentsAvailable == false)
			{
				failedChecks.Add(result);
				Console.Write(".");
			}
			else
			{
				successfulChecks.Add(result);
				Console.Write($"«{result.zipCode}/{result.stateCode}»");
			}
		}
	}
}
