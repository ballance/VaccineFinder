using System;
using System.Collections.Generic;
using System.Text;
using AppointmentMaker.Common.models;

namespace AppointmentMaker.Runer
{
	public static class VaccineDataGenerator
	{
		public static StringBuilder GenerateCSV(IEnumerable<AppointmentResponse> successfulChecks)
		{
			StringBuilder zipos = new StringBuilder();
			zipos.AppendLine("zip,state,map");
			foreach (var success in successfulChecks)
			{
				zipos.Append($"{success.zipCode},{success.stateName},https://www.google.com/maps/place/{success.zipCode}/{Environment.NewLine}");
			}

			return zipos;
		}

		public static StringBuilder GenerateHTML(IEnumerable<AppointmentResponse> successfulChecks, string csvFilename)
		{
			StringBuilder htmlStringBuilder = new StringBuilder();
			htmlStringBuilder.Append("<html><header>")
				.Append($"<title>Vaccination appointments available by ZIP Code at {DateTime.UtcNow} UTC. </title>")
				.Append("<link rel='stylesheet' href='default.css'>")
				.Append("</header><body>")
				.Append("<h2>Vaccination appointments available by ZIP Code</h2>")
				.Append($"<p>Last updated: {DateTime.UtcNow} UTC.<br />")
				.Append($"<a href='{csvFilename}'>Download as CSV</a></p>")
				.Append("<table><th>ZIP</th><th>State</th><th>Map</th><th>Appointment Link</th>");
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
				.Append("<p>Source code is available with MIT License <a href='https://github.com/ballance/VaccineFinder'>here</a></p>")
				.Append(@"<script async src='https://www.googletagmanager.com/gtag/js?id=G-N1CYCESJX7'></script><script>
  window.dataLayer = window.dataLayer || [];
			function gtag() { dataLayer.push(arguments); }
			gtag('js', new Date());
			gtag('config', 'G-N1CYCESJX7');
</script>")
				.Append("</body></html>");
			return htmlStringBuilder;
		}

		public static StringBuilder CreateDefaultCSS()
		{
			var cssBuilder = new StringBuilder();
			cssBuilder.AppendLine("tbody tr:nth-child(odd) { background-color: #ccc;}");
			cssBuilder.AppendLine("table { width:100%; }");
			cssBuilder.AppendLine(@"td 
{
/*height: 50px;
width: 50px;*/
}

td 
{
	text-align: center;
	vertical-align: middle;
}

#map {
  height: 400px;
  width: 100%;
}
			");
			return cssBuilder;
		}

		public static StringBuilder GenerateMap(IEnumerable<AppointmentResponse> successfulChecks)
		{
			StringBuilder htmlStringBuilder = new StringBuilder();
			htmlStringBuilder.Append("<html><header>")
				.Append($"<title>Vaccination appointments available by ZIP Code at {DateTime.UtcNow} UTC. </title>")
				.Append("<link rel='stylesheet' href='default.css'>")
				.Append("</header><body>")
				.Append("<h2>Vaccination appointments available by ZIP Code</h2>")
				.Append(@"
  <h3>Vaccination Availablility Map</h3>
    <div id='map'></div>")
				.Append("<p>Source code is available with MIT License <a href='https://github.com/ballance/VaccineFinder'>here</a></p>")
				.Append(@"


	<script src='https://maps.googleapis.com/maps/api/js?key=AIzaSyDd2PrS4tXsz60XTglvD3aFjoXaSm7NhlQ&callback=initMap&libraries=&v=weekly' async
	></script>
<script>
function initMap() {
  const uluru = { lat: -25.344, lng: 131.036 };
  // The map, centered at Uluru
  const map = new google.maps.Map(document.getElementById('map'), {
	zoom: 4,
	center: uluru,
  });
  // The marker, positioned at Uluru
  const marker = new google.maps.Marker({
    position: uluru,
	map: map,
  });
}
</script>
")
				.Append(@"<script async src='https://www.googletagmanager.com/gtag/js?id=G-N1CYCESJX7'></script><script>
  window.dataLayer = window.dataLayer || [];
			function gtag() { dataLayer.push(arguments); }
			gtag('js', new Date());
			gtag('config', 'G-N1CYCESJX7');
</script>")
				.Append("</body></html>");
			return htmlStringBuilder;
		}
	}
}
