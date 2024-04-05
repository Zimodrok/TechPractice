using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
namespace TechPractice {
    public partial class CreateEventView : System.Web.UI.Page {


        protected void Page_Init(object sender, EventArgs e) {
            // Call the method to create location buttons
            CreateLocationButtons();
        }

        public class CountryData {
            public string ShortForm { get; set; }
            public string Country { get; set; }
        }
        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                Session["SelectedLocations"] = new List<string>();
                FetchAndPopulateCountries();
            }
        }
        protected void txtTTL_TextChanged(object sender, EventArgs e) {
            string input = txtTTL.Text.Trim();
            string pattern = @"^\d+$";
            if (!Regex.IsMatch(input, pattern)) {
                txtTTL.Text = string.Empty;
                txtTTL.BorderColor = Color.Red;
            }else
                txtTTL.BorderColor= Color.White;
        }
        private void FetchAndPopulateCountries() {
            try {
                Console.WriteLine("Fetching countries synchronously...");
                string content = FetchCountries();
                if (!string.IsNullOrEmpty(content)) {
                    // Use regex to find the country data
                    Regex regex = new Regex("\"(\\w+)\":\\s*\\{\"country\":\\s*\"(\\w+)\"");
                    MatchCollection matches = regex.Matches(content);

                    // Extract the country data and populate the dropdown list
                    foreach (Match match in matches) {
                        string shortForm = match.Groups[1].Value;
                        string country = match.Groups[2].Value;
                        Console.WriteLine($"Short Form: {shortForm}, Country: {country}");
                        ddlLocation.Items.Add(new ListItem(country, shortForm));
                    }
                }
                else {
                    Console.WriteLine("Failed to fetch countries data.");
                }
            }
            catch (Exception ex) {
                // Handle any errors
                Console.WriteLine($"An error occurred: {ex.Message}");
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
                    Console.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }

        protected void ddlLocation_SelectedIndexChanged(object sender, EventArgs e) {
            List<string> selectedLocations = Session["SelectedLocations"] as List<string>;
            if (selectedLocations == null) {
                selectedLocations = new List<string>();
            }

            foreach (ListItem item in ddlLocation.Items) {
                if (item.Selected && !selectedLocations.Contains(item.Text)) {
                    selectedLocations.Add(item.Text);
                }
            }
            Session["SelectedLocations"] = selectedLocations;
            UpdateSelectedLocationsPanel(selectedLocations);
        }
        protected void btnSubmit_Click(object sender, EventArgs e) {
            List<string> selectedLocations = Session["SelectedLocations"] as List<string>;
            string filePath = Server.MapPath("~/EventData.csv");
            Guid eventId = Guid.NewGuid();
            string publicLink = $"https://localhost:44323/DetailedView.aspx/?id={eventId}";
            string ownerId = "user";
            using (StreamWriter writer = new StreamWriter(filePath, true)) {
                writer.WriteLine($"{eventId};{txtName.Text};{txtTTL.Text};{string.Join(",", selectedLocations)};{publicLink};{ownerId}");
            }
            selectedLocations.Clear();
        }

        protected void LocationButton_Click(object sender, EventArgs e) {
            Button btn = (Button)sender;
            string location = btn.Text;

            List<string> selectedLocations = Session["SelectedLocations"] as List<string>;
            if (selectedLocations != null) {
                selectedLocations.Remove(location);

                Session["SelectedLocations"] = selectedLocations;
                UpdateSelectedLocationsPanel(selectedLocations);
            }
        }


        private void CreateLocationButtons() {
            // Clear the panel before creating buttons
            pnlSelectedLocations.Controls.Clear();

            // Get the selected locations from session
            List<string> selectedLocations = (List<string>)Session["SelectedLocations"];

            // Initialize the list if it's null
            if (selectedLocations == null) {
                selectedLocations = new List<string>();
                Session["SelectedLocations"] = selectedLocations;
            }

            // Add buttons for each selected location
            foreach (string location in selectedLocations) {
                Button button = new Button();
                button.Text = location;
                button.CssClass = "location-button";
                button.Click += LocationButton_Click; // Attach event handler
                pnlSelectedLocations.Controls.Add(button);
            }
        }

        private void UpdateSelectedLocationsPanel(List<string> selectedLocations) {
            pnlSelectedLocations.Controls.Clear();
            foreach (string location in selectedLocations) {
                Button btn = new Button();
                btn.Text = location;
                btn.Click += new EventHandler(LocationButton_Click);
                pnlSelectedLocations.Controls.Add(btn);

                // Add a line break after each button except for the last one
                if (location != selectedLocations.Last()) {
                    pnlSelectedLocations.Controls.Add(new LiteralControl("<br/>"));
                }
            }
        }

    }
}

