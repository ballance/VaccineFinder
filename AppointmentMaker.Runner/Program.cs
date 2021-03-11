using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AppointmentMaker.Runer;
using log4net;
using log4net.Config;

namespace AppointmentMaker
{
	class Program
	{
		static void Main(string[] args)
		{
			var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
			XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

			Runner runner = new Runner();
			Task.Run(async () => await runner.Run());

			Console.ReadKey();
		}
	}
}
