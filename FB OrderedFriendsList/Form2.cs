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
using System.IO;

namespace FB_OrderedFriendsList
{
    public partial class Form2 : Form
    {
        public manage_people storage = new manage_people();
        private BackgroundWorker init_bg_worker()
        {
            BackgroundWorker bg_w = new BackgroundWorker();
            bg_w.DoWork += get_urls;
            bg_w.RunWorkerCompleted += worker_ready;
            return bg_w;
        }
        private CookieCollection cookies { get; set; }
        public Form2()
        {

            InitializeComponent();
            cookies = new CookieCollection();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.facebook.com/");
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
                //Get the response from the server and save the cookies from the first request..
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                cookies = response.Cookies;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); Application.Exit(); }
        }
        private string getfbpage(string url)
        {
            progress.Maximum = 4;
            progress.Value = 1;
            string getUrl = url;
            string postData = String.Format("email={0}&pass={1}", user.Text, pass.Text);
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(getUrl);
            progress.Value = 2;
            getRequest.CookieContainer = new CookieContainer();
            getRequest.CookieContainer.Add(cookies); //recover cookies First request
            getRequest.Method = WebRequestMethods.Http.Post;
            getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            getRequest.AllowWriteStreamBuffering = true;
            getRequest.ProtocolVersion = HttpVersion.Version11;
            getRequest.AllowAutoRedirect = true;
            getRequest.ContentType = "application/x-www-form-urlencoded";
            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            getRequest.ContentLength = byteArray.Length;
            Stream newStream = getRequest.GetRequestStream(); //open connection
            newStream.Write(byteArray, 0, byteArray.Length); // Send the data.
            newStream.Close();
            HttpWebResponse getResponse = (HttpWebResponse)getRequest.GetResponse();
            progress.Value = 3;
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }

        }
        private void getfbpage(string url, out people person)
        {
            person = new people();
            try
            {
                WebClient web = new WebClient();
                person.url = url;
                url = "https://graph.facebook.com/" + url;
                string httpfile = web.DownloadString(url);
                int i_name = httpfile.IndexOf("name") + 7;
                string s_name = httpfile.Remove(0, i_name);
                i_name = s_name.IndexOf(",") - 1;
                person.name = s_name.Remove(i_name);
                int g_index = httpfile.IndexOf("gender") + 5;
                httpfile = httpfile.Remove(0, g_index);
                httpfile = httpfile.Remove(10);
                if (!httpfile.Contains("f"))
                    person.male = true;
                else
                    person.male = false;
            }
            catch (Exception)
            { }

        }

        public void button1_Click(object sender, EventArgs e)
        {
            string html_txt = getfbpage("https://www.facebook.com/login.php?next=https%3A%2F%2Fwww.facebook.com%2Fhome.php");
            progress.Value = 4;
            int i = html_txt.IndexOf("InitialChatFriendsList") + 37;
            html_txt = html_txt.Remove(0, i);
            i = html_txt.IndexOf("]") - 1;
            html_txt = html_txt.Remove(i);
            progress.Value = 0;
            html_txt = html_txt.Replace("\"" + "," + "\"", ",");
            string[] seperator = new string[] { "," };
            string[] sfb_id = html_txt.Split(seperator, System.StringSplitOptions.RemoveEmptyEntries);
            // Testing 
            /*  string output = "";
              for (int j = 0; j < sfb_id.Length; j++)
              {
                 // fb_id[j] = Convert.ToInt32(sfb_id[j]);
                  output += sfb_id[j] + Environment.NewLine;
              }

              MessageBox.Show(output);*/
            progress.Maximum = sfb_id.Length;
            for (int k = 0; k < sfb_id.Length; k++)
            {
                multithread value = new multithread();
                value.array_pos = k;
                value.url = sfb_id[k];
                BackgroundWorker get_url = init_bg_worker();
                get_url.RunWorkerAsync(value);
            }
        }
        private void get_urls(object sender, DoWorkEventArgs e)
        {
            multithread values = (multithread)e.Argument;
            people person;
            getfbpage(values.url, out person);
            person.index = values.array_pos++;
            storage.warehouse.Add(person);
            progress.BeginInvoke(new MethodInvoker(increase_progress));

        }

        private void increase_progress()
        {
            progress.Value++;
        }

        private void worker_ready(object sender, RunWorkerCompletedEventArgs e)
        {
            
           if (progress.Value == progress.Maximum)
            {
               
                string html = "<html> <head>  <meta content=\"text/html; charset=ISO-8859-1\" http-equiv=\"content-type\"><title>Your Friends</title></head><body>";
                foreach (people s in storage.warehouse)
                {

                    html += "<a href=https://www.facebook.com/" + s.url + ">" + s.name+ "("+s.index + ". Friend)" + "</a><br>" + Environment.NewLine;
                   
                }
                html += "</body></html>";
                string path = System.IO.Path.GetTempPath() + "\\file.html";
                System.IO.File.WriteAllText(path, html);
                Process.Start(path);
            }
        }
    }
    public struct multithread
    {
        public int array_pos;
        public string url;
        public people person;
    }
    public class people
    {
        public int index;
        public string name;
        public bool male;
        public string url;
    }
    public class manage_people
    {
        public manage_people()
        {
            warehouse = new List<people>();
        }
       public List<people> warehouse;
    }
}
