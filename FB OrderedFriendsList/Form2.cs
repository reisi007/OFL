using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace FB_OrderedFriendsList
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            
            InitializeComponent();
            
        }
        
        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
          string  html_txt = textBox1.Text;
          int i = html_txt.IndexOf("OrderedFriends") +44 ;
            html_txt = html_txt.Remove(0, i);
            i = html_txt.IndexOf("]")-1;
            html_txt =  html_txt.Remove(i);
            html_txt = html_txt.Replace("\"" +"," +"\"", ",");
            string[] seperator = new string[] { ","};
            string[] sfb_id = html_txt.Split(seperator, System.StringSplitOptions.RemoveEmptyEntries);
// Testing 
          /*  string output = "";
            for (int j = 0; j < sfb_id.Length; j++)
            {
               // fb_id[j] = Convert.ToInt32(sfb_id[j]);
                output += sfb_id[j] + Environment.NewLine;
            }

            MessageBox.Show(output);*/

            string html = "<html> <head>  <meta content=\"text/html; charset=ISO-8859-1\" http-equiv=\"content-type\"><title>Your Friends</title></head><body>";

            i = 1;
            foreach (string s in sfb_id)
            {
                html += "<a href=https://www.facebook.com/" + s +">"+ i + ". Friend" +"</a><br>" + Environment.NewLine;
                i++;
            }
            html += "</body></html>";
            string path = System.IO.Path.GetTempPath() + "\\file.html";
            System.IO.File.WriteAllText(path, html);
            Process.Start(path);

            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.facebook.com/");
        }
    }
}
