using System;
using System.Collections.Generic;
using System.IO;

namespace AppointmentMaker.Runer
{
	public class InputManager : IInputManager
	{
		public List<string> LoadZipsFromFile(string inputPath)
		{
			try
			{
				var logFile = File.ReadAllLines(inputPath);
				return new List<string>(logFile);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return new List<string>();
			}
		}
		public List<ZipWithCoordinates> LoadZipsFromCSV(string inputPath)
		{
			try
			{
				var csvLines = File.ReadAllLines(inputPath);
				var zipWithCoordinatesList = new List<ZipWithCoordinates>();
				foreach (var line in csvLines)
				{
					if (line.StartsWith("ZIP_CODE"))
					{
						continue;
					}
					var splitByComma = line.Split(',');
					var zipWithCoordinates = new ZipWithCoordinates();
					zipWithCoordinates.Zip = splitByComma[0];
					zipWithCoordinates.LocationText = $"{splitByComma[1]},{splitByComma[2]}";
					zipWithCoordinates.Latitude = Convert.ToDouble(splitByComma[3]);
					zipWithCoordinates.Longitude = Convert.ToDouble(splitByComma[4]);
					zipWithCoordinatesList.Add(zipWithCoordinates);
				}
				return zipWithCoordinatesList;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return new List<ZipWithCoordinates>();
			}
		}

	}
}
