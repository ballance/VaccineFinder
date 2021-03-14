using System.Collections.Generic;

namespace AppointmentMaker.Runer
{
	public interface IInputManager
	{
		List<ZipWithCoordinates> LoadZipsFromCSV(string inputPath);
		List<string> LoadZipsFromFile(string inputPath);
	}
}