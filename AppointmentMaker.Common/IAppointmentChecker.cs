using System.Threading.Tasks;
using AppointmentMaker.Common.models;

namespace AppointmentMaker.Common
{
	public interface IAppointmentChecker
	{
		Task<AppointmentResponse> CheckForAppointmentAsync(string content);
		void SetHeader(string headerKey, string headerValue);
	}
}