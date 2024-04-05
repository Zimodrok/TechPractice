using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace TechPractice {
    public partial class WebForm1 : System.Web.UI.Page {
        private Dictionary<string, EventData> eventDataDict = new Dictionary<string, EventData>();
        protected void Page_Load(object sender, EventArgs e) {
        }

        public class EventData {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int TTL { get; set; }
            public List<string> Locations { get; set; }
            public string PublicLink { get; set; }
            public string OwnerID { get; set; }
        }
        protected void Page_Init(object sender, EventArgs e) {
            // Read data from the CSV file
            List<EventData> eventDataList = ReadEventDataFromCSV("EventData.csv");
            List<string> buttonIds = ViewState["ButtonIds"] as List<string> ?? new List<string>();
            foreach (EventData eventData in eventDataList) {
                string buttonId = "btn" + eventData.Id.ToString("N");
                if (buttonIds.Contains(buttonId)) {
                    Console.WriteLine("CSV IS CORRUPTED, UUID IS UNUNIQUE");
                    continue; // Skip adding the button if the ID already exists
                }
                buttonIds.Add(buttonId);
                Button button = new Button();
                button.ID = buttonId; button.CssClass = "container-text";
                button.Text = eventData.Name;
                button.Click += new EventHandler(btn_Click);

                HtmlGenericControl listItem = new HtmlGenericControl("li");
                listItem.Controls.Add(button);

                container.Controls.Add(listItem);

                // Store the eventData object in a dictionary with the button ID as the key
                eventDataDict.Add(button.ID, eventData);
            }
            ViewState["ButtonIds"] = buttonIds;
        }

        // Method to read event data from the CSV file
        private List<EventData> ReadEventDataFromCSV(string fileName) {
            string filePath = Server.MapPath($"~/{fileName}");

            List<EventData> eventDataList = new List<EventData>();

            // Read data from the CSV file
            using (StreamReader reader = new StreamReader(filePath)) {
                // Read and process each line
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(';');
                    if (parts.Length >= 6) // Ensure the line has at least 6 parts
                    {
                        EventData eventData = new EventData();
                        eventData.Id = Guid.Parse(parts[0]);
                        eventData.Name = parts[1];
                        eventData.TTL = int.Parse(parts[2]);
                        eventData.Locations = parts[3].Split(',').ToList();
                        eventData.PublicLink = parts[4];
                        eventData.OwnerID = parts[5];
                        eventDataList.Add(eventData);
                    }
                }
            }

            return eventDataList;
        }

        protected void btnCreateEvent_Click(object sender, EventArgs e) {
           // Response.Redirect($"CreateEventView.aspx");
        }
        protected void btn_Click(object sender, EventArgs e) {
            Button clickedButton = (Button)sender;
            string buttonText = clickedButton.Text;
            EventData eventData = eventDataDict[clickedButton.ID];
            string eventName = eventData.Name;
            int eventTTL = eventData.TTL;
            List<string> eventLocations = eventData.Locations;
            string eventPublicLink = eventData.PublicLink;
            Response.Redirect($"DetailedView.aspx?&id={eventData.Id}");
        }
    }
}