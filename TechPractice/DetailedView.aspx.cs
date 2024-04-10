using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
namespace TechPractice {
    public partial class DetailedView : System.Web.UI.Page {
        private string eventId;
        private string filePath;
        private string[] lines;
        private string GMTEventTime;
        private List<string> relatedLocationsList = new List<string>();

        // Нужно будет брать это из БД с привязкой к юзерам и их персональным настройкам
        public static string usersCountrysTimeZone = TimeZoneDictionary.TimeZones["UA"].Item2;
        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                eventId = Request.QueryString["id"];
                if (string.IsNullOrEmpty(eventId)) {
                    Debug.WriteLine("Event ID is null or empty.");
                    Response.Redirect("~/ErrorPage.aspx");
                    return;
                }
                filePath = Server.MapPath("~/EventData.csv");
                FetchAndPopulateCountries();
                LoadEventData();
            }
            UpdateRelatedLocationsPanel();
        }

        private void LoadEventData() {
            eventId = Request.QueryString["id"];
            Session["eventId"] = eventId;

            filePath = Server.MapPath("~/EventData.csv");
            lines = File.ReadAllLines(filePath);

            if (lines != null) {
                foreach (string line in lines) {
                    if (line.StartsWith(eventId)) {
                        string[] eventData = line.Split(';');
                        txtName.Text = eventData[1];
                        txtTTL.Text = eventData[2];
                        ddlTimeZones.SelectedValue = eventData[3];
                        txtDateTime.Value = eventData[6];
                        LinkLabel.Text = eventData[4];
                        if (TimeZoneDictionary.TimeZones[eventData[3]].Item2 != "UTCVariety") {
                            GMTEventTime = AddOrSubtractHours(eventData[6], TimeZoneDictionary.TimeZones[eventData[3]].Item2, true);
                        }
                        else {
                            GMTEventTime = AddOrSubtractHours(eventData[6], eventData[7], true);
                            ViewState["TimeZoneClarification"] = eventData[7];
                        }
                        Session["GMTEventTime"] = GMTEventTime;
                        string[] relatedLocations;
                        try {
                            relatedLocations = eventData[8].Split(',');
                        }
                        catch { relatedLocations = null; }
                        List<string> relatedLocationsList = new List<string>();
                        if (relatedLocations != null) {
                            foreach (string location in relatedLocations) {
                                if (!string.IsNullOrEmpty(location)) {
                                    relatedLocationsList.Add(location.Trim());
                                }
                            }
                        }
                        Session["RelatedLocationsList"] = relatedLocationsList;
                        UpdateRelatedLocationsPanel();

                        UsersLocalEventTimeLabel.Text = $"Event Time by users local time: {AddOrSubtractHours(Session["GMTEventTime"] as string, usersCountrysTimeZone, false)}";
                        ViewState["OriginalName"] = txtName.Text;
                        ViewState["OriginalTTL"] = txtTTL.Text;
                        ViewState["OriginalTimeZones"] = ddlTimeZones.SelectedValue;
                        ViewState["OriginalDateTime"] = txtDateTime.ToString();
                        ViewState["OriginalPublicLink"] = LinkLabel.Text;
                        break;
                    }
                }
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

        private void FetchAndPopulateCountries() {
            try {
                Debug.WriteLine("Fetching countries synchronously...");
                string content = FetchCountries();
                if (!string.IsNullOrEmpty(content)) {
                    Regex regex = new Regex("\"(\\w+)\":\\s*\\{\"country\":\\s*\"([\\w\\s]+)\"");
                    MatchCollection matches = regex.Matches(content);
                    foreach (Match match in matches) {
                        string shortForm = match.Groups[1].Value;
                        string country = match.Groups[2].Value;
                        Debug.WriteLine($"Short Form: {shortForm}, Country: {country}");
                        ddlTimeZones.Items.Add(new ListItem(country, shortForm));
                        ddlRelatedLocations.Items.Add(new ListItem(country, shortForm));
                    }
                }
                else {
                    Debug.WriteLine("Failed to fetch countries data.");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static string AddOrSubtractHours(string dateTimeString, string utcOffset, bool invertSign) {
            string[] eventData = HttpContext.Current.Session["TimeZoneClarification"] as string[];
            if (utcOffset == "UTCVariety" && eventData != null && eventData.Length > 7) {
                utcOffset = eventData[7];
            }
            DateTime dateTime = DateTime.Parse(dateTimeString);
            char sign = utcOffset[3]; 
            int hours = int.Parse(utcOffset.Substring(4, 2));
            int minutes = 0;
            if (utcOffset.Length > 3) {
                minutes = int.Parse(utcOffset.Substring(7, 2));
            }
            int totalOffsetMinutes = (hours * 60 + minutes) * (sign == '-' ? -1 : 1);
            if (invertSign) {
                totalOffsetMinutes *= -1;
            }

            TimeSpan offsetTimeSpan = TimeSpan.FromMinutes(totalOffsetMinutes);
            DateTime resultDateTime = dateTime.Add(offsetTimeSpan);

            return resultDateTime.ToString("yyyy-MM-ddTHH:mm");
        }
        private void UpdateRelatedLocationsPanel() {
            RelatedLocationsButtonsPanel.Controls.Clear();

            if (Session["RelatedLocationsList"] != null) {
                relatedLocationsList = (List<string>)Session["RelatedLocationsList"];
                foreach (var relatedLocation in relatedLocationsList) {
                    var cardDiv = new HtmlGenericControl("div");
                    cardDiv.Attributes["class"] = "card";

                    var CountryParagraph = new HtmlGenericControl("p");
                    CountryParagraph.Attributes["class"] = "person_name";
                    CountryParagraph.InnerText = $"Country: {TimeZoneDictionary.TimeZones[relatedLocation].Item1}";

                    var TimeZoneParagraph = new HtmlGenericControl("p");
                    TimeZoneParagraph.Attributes["class"] = "person_name";
                    TimeZoneParagraph.InnerText = $"Time Zone: {TimeZoneDictionary.TimeZones[relatedLocation].Item2}";

                    var personDesgParagraph = new HtmlGenericControl("p");
                    personDesgParagraph.Attributes["class"] = "person_name";
                    Debug.WriteLine(Session["GMTEventTime"] as string);
                    personDesgParagraph.InnerText = $"Event Time by local time: {AddOrSubtractHours(Session["GMTEventTime"] as string, TimeZoneDictionary.TimeZones[relatedLocation].Item2, false)}";

                    var deleteButton = new Button();
                    deleteButton.CssClass = "delete_btn";
                    deleteButton.Text = "Delete";
                    deleteButton.Click += (sender, e) => {
                        RelatedLocationsButtonsPanel.Controls.Remove(cardDiv);
                        relatedLocationsList.Remove(relatedLocation);
                        Session["RelatedLocationsList"] = relatedLocationsList;
                        UpdateCSVRelatedLocations();
                    };

                    cardDiv.Controls.Add(CountryParagraph);
                    cardDiv.Controls.Add(TimeZoneParagraph);
                    cardDiv.Controls.Add(personDesgParagraph);
                    cardDiv.Controls.Add(deleteButton);
                    RelatedLocationsButtonsPanel.Controls.Add(cardDiv);
                }
                UpdateCSVRelatedLocations();
            }
        }

        private void UpdateCSVRelatedLocations() {
            string updatedLine = string.Join(",", relatedLocationsList);
            string updatedFilePath = Server.MapPath("~/EventData.csv");
            string[] allLines = File.ReadAllLines(updatedFilePath);
            string eventId = Session["eventId"] as string;

            if (eventId == null) {
                Debug.WriteLine("Event ID is null in the session.");
                return;
            }

            for (int i = 0; i < allLines.Length; i++) {
                if (allLines[i].StartsWith(eventId)) {
                    string[] eventData = allLines[i].Split(';');
                    try {
                        if (eventData.Length >= 7) {
                            eventData[8] = updatedLine;
                            allLines[i] = string.Join(";", eventData);
                        }
                    } catch { }
                    break;
                }
            }
            File.WriteAllLines(updatedFilePath, allLines);
        }

        protected void btnSave_Click(object sender, EventArgs e) {
            ToggleEditability(false);
            string newName = txtName.Text;
            string newTTL = txtTTL.Text;
            string newTimeZones = ddlTimeZones.SelectedValue;
            string newDateTime = txtDateTime.Value;
            string newPublicLink = LinkLabel.Text;
            eventId = Request.QueryString["id"];
            filePath = Server.MapPath("~/EventData.csv");

            try {
                Debug.WriteLine("Save button clicked");
                if (eventId == null) {
                    throw new ArgumentNullException(nameof(eventId), "Event ID is null");
                }
                string newLine = $"{eventId};{newName};{newTTL};{newTimeZones};{newPublicLink};user;{newDateTime}";
                string[] allLines = File.ReadAllLines(filePath);
                for (int i = 0; i < allLines.Length; i++) {
                    Debug.WriteLine($"Processing line {i}: {allLines[i]}, Event ID: {eventId}");
                    if (allLines[i].StartsWith(eventId)) {
                        Debug.WriteLine($"Line {i} matches the event ID");
                        allLines[i] = newLine;
                        Debug.WriteLine($"Line {i} replaced with: {newLine}");
                    }
                }
                File.WriteAllLines(filePath, allLines);
                Debug.WriteLine("Changes saved successfully");
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error saving changes: {ex.Message}");
            }
        }

        protected void btnToggleEdit_Click(object sender, EventArgs e) {
            ToggleEditability(true);
        }

        private void ToggleEditability(bool enableEditing) {
            PanelEventDetails.Enabled = !PanelEventDetails.Enabled;
            btnSave.Visible = enableEditing;
            btnToggleEdit.Visible = !enableEditing;
            txtDateTime.Disabled = !enableEditing;
        }

        protected void ddlRelatedLocations_SelectedIndexChanged(object sender, EventArgs e) {
            if (Session["RelatedLocationsList"] == null) {
                relatedLocationsList = new List<string>();
            }
            else {
                relatedLocationsList = (List<string>)Session["RelatedLocationsList"];
            }

            relatedLocationsList.Add(ddlRelatedLocations.SelectedValue);
            Session["RelatedLocationsList"] = relatedLocationsList;
            UpdateRelatedLocationsPanel();
        }

        private void AddTimeZoneClarificationDropDown(string location) {
            DropDownList ddlTimeZoneClarification = new DropDownList();
            ddlTimeZoneClarification.ID = "ddlTimeZoneClarification_" + location;
            List<string> utcOffsets = new List<string>();
            switch (location) {
                case "RU": // Russia
                    utcOffsets.AddRange(new[] { "+02:00", "+03:00", "+04:00", "+05:00", "+06:00", "+07:00", "+08:00", "+09:00", "+10:00", "+11:00", "+12:00" });
                    break;
                case "US": // USA
                    utcOffsets.AddRange(new[] { "-10:00", "-09:00", "-08:00", "-07:00", "-06:00", "-05:00", "-04:00" });
                    break;
                case "MN": // Mongolia
                    utcOffsets.AddRange(new[] { "+07:00", "+08:00" });
                    break;
                case "MX": // Mexico
                    utcOffsets.AddRange(new[] { "-05:00", "-06:00", "-07:00" });
                    break;
                case "KZ": // Kazakhstan
                    utcOffsets.AddRange(new[] { "+05:00", "+06:00" });
                    break;
                case "ID": // Indonesia
                    utcOffsets.AddRange(new[] { "+07:00", "+08:00", "+09:00" });
                    break;
                case "BR": // Brazil
                    utcOffsets.AddRange(new[] { "-05:00", "-04:00", "-03:00", "-02:00" });
                    break;
                case "AU": // Australia
                    utcOffsets.AddRange(new[] { "+10:30", "+10:00", "+09:30", "+08:45", "+08:00" });
                    break;
                default:
                    for (int hour = -12; hour <= 12; hour++) {
                        for (int minute = 0; minute <= 30; minute += 30) {
                            string offset = $"{(hour >= 0 ? "+" : "-")}{Math.Abs(hour).ToString("00")}:{minute.ToString("00")}";
                            utcOffsets.Add(offset);
                        }
                    }
                    break;
            }
            foreach (var timeZone in utcOffsets) {
                ddlTimeZoneClarification.Items.Add(new ListItem(timeZone, timeZone));
            }
            pnlSelectedLocations.Controls.Add(ddlTimeZoneClarification);
        }

        protected void ddlTimeZones_SelectedIndexChanged(object sender, EventArgs e) {
            string location = ddlTimeZones.SelectedItem.Value;
            Session["SelectedLocation"] = location;
            if (TimeZoneDictionary.TimeZones.ContainsKey(location) && TimeZoneDictionary.TimeZones[location].Item2 == "UTCVariety") {
                AddTimeZoneClarificationDropDown(location);
            }
            else {
                RemoveTimeZoneDropdown();
            }
            UpdatePanel1.Update();
        }

        private void RemoveTimeZoneDropdown() {
            DropDownList ddlTimezone = (DropDownList)pnlSelectedLocations.FindControl("ddlTimezone");
            if (ddlTimezone != null) {
                pnlSelectedLocations.Controls.Remove(ddlTimezone);
            }
        }

        protected void RelLocButton_Click(object sender, EventArgs e) {
            Button btn = (Button)sender;
            string locationName = btn.Text.Trim();

            Debug.WriteLine($"Clicked button name: {locationName}");

            if (Session["RelatedLocationsList"] != null) {
                relatedLocationsList = (List<string>)Session["RelatedLocationsList"];
                relatedLocationsList.Remove(locationName);
                Session["RelatedLocationsList"] = relatedLocationsList;

                Debug.WriteLine("Updated list of selected related regions:");
                foreach (var region in relatedLocationsList) {
                    Debug.WriteLine(region);
                }
                UpdateRelatedLocationsPanel();
            }
            UpdateRelatedLocationsPanel();
        }
    }
}
