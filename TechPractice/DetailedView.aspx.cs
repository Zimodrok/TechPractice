using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static TechPractice.WebForm1;

namespace TechPractice {
    public partial class DetailedView : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                // Get the ID from the query string
                string id = Request.QueryString["id"];

                // Path to the CSV file
                string filePath = Server.MapPath("~/EventData.csv");

                // Read the CSV file and search for the record with the given ID
                string[] lines = File.ReadAllLines(filePath);
                string eventData = null;

                foreach (string line in lines) {
                    if (line.StartsWith(id + ";")) {
                        eventData = line;
                        break;
                    }
                }

                if (eventData != null) {
                    // Split the eventData by ";" delimiter
                    string[] eventDataArray = eventData.Split(';');

                    // Extract data from the eventDataArray
                    string name = eventDataArray[1];
                    int ttl = int.Parse(eventDataArray[2]);
                    string[] timezones = eventDataArray[3].Split(',');
                    string publiclink = eventDataArray[4];
                    string ownerId = eventDataArray[5];

                    // Display the data on the page
                    lblName.Text = name;
                    lblTTL.Text = ttl.ToString();
                    lblTimeZones.Text = string.Join(", ", timezones);
                    lblPublicLink.Text = publiclink;
                }
                else {
                    // Handle the case when the record with the given ID is not found
                    lblName.Text = "Event not found";
                    lblTTL.Text = "-";
                    lblTimeZones.Text = "-";
                    lblPublicLink.Text = "-";
                }
            }
        }
    }
}