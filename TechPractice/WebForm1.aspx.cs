using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            LoadEventData();
        }

        protected void btnClose_Click(object sender, EventArgs e) {
            System.Threading.Thread.Sleep(1000);
            LoadEventData();
        }

        private void LoadEventData() {
            List<EventData> eventDataList = ReadEventDataFromCSV("EventData.csv");
            List<string> buttonIds = ViewState["ButtonIds"] as List<string> ?? new List<string>();
            foreach (EventData eventData in eventDataList) {
                string buttonId = "btn" + eventData.Id.ToString("N");
                if (buttonIds.Contains(buttonId)) {
                    Debug.WriteLine("CSV IS CORRUPTED, UUID IS UNUNIQUE");
                    continue;
                }
                buttonIds.Add(buttonId);
                Button button = new Button();
                button.ID = buttonId;
                button.CssClass = "container-text button-wrap";
                button.Text = eventData.Name;
                button.Click += new EventHandler(btn_Click);

                HtmlGenericControl listItem = new HtmlGenericControl("li");
                listItem.Controls.Add(button);

                container.Controls.Add(listItem);
                eventDataDict.Add(button.ID, eventData);
            }
            ViewState["ButtonIds"] = buttonIds;
        }

        private List<EventData> ReadEventDataFromCSV(string fileName) {
            string filePath = Server.MapPath($"~/{fileName}");

            List<EventData> eventDataList = new List<EventData>();
            using (StreamReader reader = new StreamReader(filePath)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(';');
                    if (parts.Length >= 6)
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