using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AppointmentMaker.Common;
using AppointmentMaker.Runer;
using Autofac;
using log4net;
using log4net.Config;

namespace AppointmentMaker
{
	class Program
	{
		private static IContainer _container;

		static void Main(string[] args)
		{
			var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
			XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

			RegisterServices();

			var runner = _container.Resolve<IRunner>();

			Task.Run(async () => await runner.Run());

			Console.ReadKey();
		}

		private static void RegisterServices()
		{

			var builder = new ContainerBuilder();

			builder.RegisterType<Runner>().As<IRunner>().SingleInstance();
			builder.RegisterType<AppointmentChecker>().As<IAppointmentChecker>().SingleInstance();
			builder.RegisterType<PublishingManager>().As<IPublishingManager>().SingleInstance();
			builder.RegisterType<InputManager>().As<IInputManager>().SingleInstance();


			_container = builder.Build();
		}
	}
}
