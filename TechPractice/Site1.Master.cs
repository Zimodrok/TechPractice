using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TechPractice {
    public partial class Site1 : System.Web.UI.MasterPage {
        protected void Page_Load(object sender, EventArgs e) {

        }
        protected void Timer1_Tick(object sender, EventArgs e) {

            Label1.Text = DateTime.Now.ToString("HH:mm:ss");

        }
    }
}