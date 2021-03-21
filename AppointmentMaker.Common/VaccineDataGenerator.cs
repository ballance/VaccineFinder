using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppointmentMaker.Common.models;

namespace AppointmentMaker.Runer
{
	public static class VaccineDataGenerator
	{
		public static StringBuilder GenerateCSV(IEnumerable<AppointmentResponse> successfulChecks)
		{
			StringBuilder zipos = new StringBuilder();
			zipos.AppendLine("zip,Location,map");
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
				.Append($"<a href='map.html'>View on a map</a></p>")
				.Append("<table><th>ZIP</th><th>Location</th><th>Map</th><th>Appointment Link</th>");
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
  height: 80%;
  width: 100%;
}
			");
			return cssBuilder;
		}

		public static StringBuilder GenerateMap(IEnumerable<AppointmentResponse> successfulChecks, string googleApiKey)
		{
			var distinctSuccessfulChecks = successfulChecks.GroupBy(x => x.zipCode, (key, group) => group.First());


			StringBuilder htmlStringBuilder = new StringBuilder();
			htmlStringBuilder.Append("<html><header>")
				.AppendLine($"<title>Vaccination appointments available by ZIP Code at {DateTime.UtcNow} UTC. </title>")
				.AppendLine("<link rel='stylesheet' href='default.css'>")
				.AppendLine($"<script async defer src='https://maps.googleapis.com/maps/api/js?key={googleApiKey}&callback=initMap'></script>")
				.AppendLine("</header><body>")
				.AppendLine("<h2>Vaccination appointments available by ZIP Code</h2>")
				.AppendLine("<div id='map'></div>")
				.AppendLine("<p>Source code is available with MIT License <a href='https://github.com/ballance/VaccineFinder'>here</a></p>")
				.AppendLine(@"
<script>");

			htmlStringBuilder.AppendLine("var markers = [");

			foreach (var check2 in distinctSuccessfulChecks)
			{
				htmlStringBuilder.AppendLine($"['{check2.zipCode}',{check2.Latitude},{check2.Longitude}],");
			}
			htmlStringBuilder.AppendLine("];");
			htmlStringBuilder.Append(@"

function initMap() {
  ");
			



			htmlStringBuilder.AppendLine(@"
  const centerOfUsa = { lat: 38.90575952495474, lng: -98.34887021172555 };
  const svgMarker = {
    path:
      'M10.453 14.016l6.563-6.609-1.406-1.406-5.156 5.203-2.063-2.109-1.406 1.406zM12 2.016q2.906 0 4.945 2.039t2.039 4.945q0 1.453-0.727 3.328t-1.758 3.516-2.039 3.070-1.711 2.273l-0.75 0.797q-0.281-0.328-0.75-0.867t-1.688-2.156-2.133-3.141-1.664-3.445-0.75-3.375q0-2.906 2.039-4.945t4.945-2.039z',
    fillColor: 'green',
    fillOpacity: 0.6,
    strokeWeight: 0,
    rotation: 0,
    scale: 2,
    anchor: new google.maps.Point(15, 30),
  };

  const map = new google.maps.Map(document.getElementById('map'), {

	zoom: 4,
	center: centerOfUsa,
  });
  var bounds = new google.maps.LatLngBounds();
");
			htmlStringBuilder.AppendLine(@"
	for( i = 0; i < markers.length; i++ ) {
        var position = new google.maps.LatLng(markers[i][1], markers[i][2]);
        bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            map: map,
			icon: svgMarker,
			label: { color: '#00aaff', fontWeight: 'bold', fontSize: '14px', text: markers[i][0] },
            title: markers[i][0]
        });
        
        // Automatically center the map fitting all markers on the screen
        map.fitBounds(bounds);
    }
");

			foreach (var check2 in distinctSuccessfulChecks)
			{
				//htmlStringBuilder.AppendLine($"marker_{check2.zipCode}.setMap(map);");
			}

			htmlStringBuilder.AppendLine("}");


			htmlStringBuilder.AppendLine("</script>");

			htmlStringBuilder.Append(@"<script async src='https://www.googletagmanager.com/gtag/js?id=G-N1CYCESJX7'></script>
  <script>window.dataLayer = window.dataLayer || [];
			function gtag() { dataLayer.push(arguments); }
			gtag('js', new Date());
			gtag('config', 'G-N1CYCESJX7');
</script>").Append(Environment.NewLine)
				.AppendLine("</body></html>");
			return htmlStringBuilder;
		}
	}
}
