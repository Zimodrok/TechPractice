﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace TechPractice {
    public partial class Site1 : System.Web.UI.MasterPage {
        List<string> shortCountryNames = new List<string>();

        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                FetchAndPopulateCountries();
                CheckShortIdsAndAddToChart();
            }
        }
        protected void Timer1_Tick(object sender, EventArgs e) {

            Label1.Text = DateTime.Now.ToString("HH:mm:ss");

        }

        private void FetchAndPopulateCountries() {
            try {
                Debug.WriteLine("Fetching countries synchronously...");
                string content = FetchCountries();
                if (!string.IsNullOrEmpty(content)) {
                    Regex regex = new Regex("\"(\\w+)\":\\s*\\{");
                    MatchCollection matches = regex.Matches(content);
                    foreach (Match match in matches) {
                        string shortID = match.Groups[1].Value;
                        shortCountryNames.Add(shortID);
                    }

                    Debug.WriteLine("Short country names fetched and populated successfully.");
                }
                else {
                    Debug.WriteLine("Failed to fetch countries data.");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private string FetchCountries() {
            using (HttpClient client = new HttpClient()) {
                string url = "https://cdn.amcharts.com/lib/5/geodata/data/countries2.js";
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode) {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else {
                    Debug.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }

        protected void CheckShortIdsAndAddToChart() {
            List<string> locations = ReadCsvAndGetLocations("EventData.csv");

            System.Text.StringBuilder jsCode = new System.Text.StringBuilder();
                    jsCode.AppendLine(
                        "        am5.ready(function () {" +
                        "\r\n            var root = am5.Root.new(\"chartdiv\");" +
                        "\r\n            root.setThemes([" +
                        "\r\n                am5themes_Animated.new(root)" +
                        "\r\n            ]);" +
                        "\r\n            var chart = root.container.children.push(" +
                        "\r\n                am5map.MapChart.new(root, {" +
                        "\r\n                    panX: \"none\"," +
                        "\r\n                    panY: \"none\"," +
                        "\r\n                    wheelX: \"none\"," +
                        "\r\n                    wheelY: \"none\"," +
                        "\r\n                    projection: am5map.geoEquirectangular()," +
                        "\r\n                }));" +
                        "\r\n" +
                        "\r\n            var colorSet = am5.ColorSet.new(root, {});" +
                        "\r\n            var areaSeries = chart.series.push(" +
                        "\r\n                am5map.MapPolygonSeries.new(root, {" +
                        "\r\n                    geoJSON: am5geodata_worldLow," +
                        "\r\n                })" +
                        "\r\n            );" +
                        "\r\n" +
                        "\r\n            var areaPolygonTemplate = areaSeries.mapPolygons.template;" +
                        "\r\n            areaPolygonTemplate.setAll({ fillOpacity: 0.6 });" +
                        "\r\n" +
                        "\r\n            areaPolygonTemplate.adapters.add(\"fill\", function (fill, target) {" +
                        "\r\n                return am5.Color.saturate(" +
                        "\r\n                    colorSet.getIndex(areaSeries.mapPolygons.indexOf(target))," +
                        "\r\n                    0.03" +
                        "\r\n                );" +
                        "\r\n            });" +
                        "\r\n" +
                        "\r\n            areaPolygonTemplate.states.create(\"hover\", { fillOpacity: 0.8 });" +
                        "\r\n" +
                        "\r\n            var zoneSeries = chart.series.push(" +
                        "\r\n                am5map.MapPolygonSeries.new(root, {" +
                        "\r\n                    geoJSON: am5geodata_worldTimeZonesLow," +
                        "\r\n" +
                        "\r\n                })" +
                        "\r\n            );" +
                        "\r\n" +
                        "\r\n            zoneSeries.mapPolygons.template.setAll({" +
                        "\r\n                fill: am5.color(0x000000)," +
                        "\r\n                fillOpacity: 0.08," +
                        "\r\n            });" +
                        "\r\n" +
                        "\r\n            var zonePolygonTemplate = zoneSeries.mapPolygons.template;" +
                        "\r\n" +
                        "\r\n            zonePolygonTemplate.setAll({ interactive: true, tooltipText: \"{id}\" });" +
                        "\r\n            zonePolygonTemplate.states.create(\"hover\", { fillOpacity: 0.3 });" +
                        "\r\n            zonePolygonTemplate.events.on(\"click\", function (ev) {" +
                        "\r\n                console.log(ev.target.dataItem.dataContext.id);" +
                        "\r\n            });" +
                        "\r\n" +
                        "            var pointSeries = chart.series.push(" +
                        "              am5map.MapPointSeries.new(root, {" +
                        "                polygonIdField: \"country\"" +
                        "              })" +
                        "            );"

                    );
            Dictionary<string, List<string>> locationEvents = new Dictionary<string, List<string>>();
            foreach (string location in locations) {
                string[] parts = location.Split(';');
                if (parts.Length == 2) {
                    string country = parts[0].Trim();
                    string eventName = parts[1].Trim();
                    if (shortCountryNames.Contains(country)) {
                        if (!locationEvents.ContainsKey(country)) {
                            locationEvents[country] = new List<string>();
                        }
                        if (eventName.Length > 16) {
                            eventName = eventName.Substring(0, 17) + "...";
                        }
                        locationEvents[country].Add(eventName);

                    }
                }
            }
            foreach (var kvp in locationEvents) {
                string country = kvp.Key;
                List<string> events = kvp.Value;
                jsCode.AppendLine($"pointSeries.pushDataItem({{ polygonId: \"{country}\", name: \"{string.Join(";  ", events)}\" }});");
            }
            jsCode.AppendLine("pointSeries.bullets.push(function() {\r\n  return am5.Bullet.new(root, {\r\n    sprite: am5.Circle.new(root, {\r\n      radius: 3,\r\n      fill: am5.color(0xff0000),   fillOpacity: 0.7,  tooltipText: \"{name}\"\r\n    })\r\n  });\r\n});" +
                        "\r\n            chart.appear(1000, 100);" +
                        "\r\n" +
                        "\r\n        });");
                    ScriptManager.RegisterStartupScript(this, GetType(), "AddToChartScript", jsCode.ToString(), true);
        }

        protected List<string> ReadCsvAndGetLocations(string fileName) {
            List<string> locations = new List<string>();
            string filePath = Server.MapPath($"~/EventData.csv");
            using (StreamReader reader = new StreamReader(filePath)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(';');
                    if (parts.Length >= 4) {
                        string location = parts[3].Trim();
                        string eventValue = parts[1].Trim();
                        string locationAndEvent = $"{location}; {eventValue}";
                        locations.Add(locationAndEvent);
                    }
                }
            }

            return locations;
        }
    }
}
