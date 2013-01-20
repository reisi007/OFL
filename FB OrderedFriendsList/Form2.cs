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
        private int progress_max { get; set; }
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
            { MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + "Check your internet connection!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); Application.Exit(); }
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
                person.name = person.name.Replace("\\u00fc", "&uuml;"); //ü
                person.name = person.name.Replace("\\u00f6", "&ouml;"); // ö
                person.name = person.name.Replace("\\u00e4", "&auml;"); // ä
                person.name = person.name.Replace("\\u00f6", "&Üuml;"); // Ü
                person.name = person.name.Replace("\u00d6", "&Ouml;"); // Ö
                person.name = person.name.Replace("\\u00c4", "&Auml;"); // Ä
                person.name = person.name.Replace("\\u00df", "&szlig;"); // ß
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
        private void set_progress_max()
        {
            progress.Maximum = progress_max;
        }
        private void reset_process()
        {
            progress.Value = 0;
        }

        private void worker_ready(object sender, RunWorkerCompletedEventArgs e)
        {

            if (progress.Value == progress.Maximum)
            {
                progress.Invoke(new MethodInvoker(reset_process));
                Dictionary<int, string> sd_index_url = new Dictionary<int, string>();
                Dictionary<int, string> sd_index_name = new Dictionary<int, string>();
                Dictionary<int, bool> sd_index_male = new Dictionary<int, bool>();
                List<int> l_indexes = new List<int>();

                foreach (people p in storage.warehouse)
                {
                    if (p.name != null)
                    {
                        sd_index_url.Add(p.index, p.url);
                        sd_index_name.Add(p.index, p.name);
                        sd_index_male.Add(p.index, p.male);
                        l_indexes.Add(p.index);
                    }
                }
                progress_max = l_indexes.Count;
                progress.Invoke(new MethodInvoker(set_progress_max));
                l_indexes.Sort();
                int[] indexes = l_indexes.ToArray();
                int index;
                List<people> tmp = new List<people>();
                for (int k = 0; k < progress_max; k++)
                {
                    progress.BeginInvoke(new MethodInvoker(increase_progress));
                    index = indexes[k];
                    people tmp_people = new people();
                    tmp_people.index = k + 1;
                    sd_index_male.TryGetValue(index, out tmp_people.male);
                    sd_index_name.TryGetValue(index, out tmp_people.name);
                    sd_index_url.TryGetValue(index, out tmp_people.url);
                    tmp.Add(tmp_people);
                }
                string head1 = "<html> <head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />  <title>Your Friends</title>";
                string head2 = "</head>\n";
                string body1 = "<body><table border = \"1\"";
                body1 += @"<tr><td><h2>Number of Friends</h2></td> <td><h2>Picture</h2></td> <td><h2>Name</h2></td> </tr>";
                progress_max = progress_max + tmp.Count;
                progress.Invoke(new MethodInvoker(set_progress_max));
                string items = "";
                string id;
                foreach (people s in tmp)
                {
                    if (s.male)
                        id = "m";
                    else
                        id = "f";
                    items += Environment.NewLine + "<tr class=\""+id+"\">";
                    items += "<td> " + s.index + " </td>\t";
                    items += "<td><a href=\"https://graph.facebook.com/" + s.url + "/picture?type=large\"title=\"" + s.name + "\" class=\"preview\"><img src=\"https://graph.facebook.com/" + s.url + "/picture\" alt=\"" + s.name + "'s profile picture\"/></a></td>";
                    items += "\t\t<td><a id=\"" + id + "\" href=https://www.facebook.com/" + s.url + ">" + s.name + "</a></td>";
                    progress.BeginInvoke(new MethodInvoker(increase_progress));
                }
                string body2 = "</tr> </table>";
                body2 += copyright();
                body2 += "</body></html>";
                string path = System.IO.Path.GetTempPath() + "\\file.html";
                System.IO.File.WriteAllText(path, head1 + Environment.NewLine + js() + Environment.NewLine + css() + Environment.NewLine + head2 + Environment.NewLine + body1 + Environment.NewLine + items + Environment.NewLine + body2);
                Process.Start(path);
            }
        }
        /* Parts of the webpage
         * head1
         * js
         * css
         * heads2
         * body1
         * items
         * body2
         * end*/
        private string js()
        {
            string html = "</script><script type=\"text/javascript\" src=\"http://cssglobe.com/lab/tooltip/02/jquery.js \"></script>" + Environment.NewLine + Environment.NewLine;
            ;
            html += "<script type=\"text/javascript\" src=\"http://cssglobe.com/lab/tooltip/02/main.js \"></script>";

            return html;
        }
        private string copyright()
        {
            string year = DateTime.Now.ToString("yyyy");
            string copy = "&copy; 2012-" + year + " Reisisoft";
            return "<p><div align=\"center\">" + copy + "</div>";
        }
        private string css()
        {
            return "   <style>" + Environment.NewLine +
      "body {" + Environment.NewLine +
          "margin:0;" + Environment.NewLine +
          "padding:40px;" + Environment.NewLine +
          "background:#ffffff;" + Environment.NewLine +
          "font:80% Arial, Helvetica, sans-serif;" + Environment.NewLine +
          "align:\"center\";" + Environment.NewLine +
          "color:#ffffff;" + Environment.NewLine +
          "line-height:180%;" + Environment.NewLine +
      "}" + Environment.NewLine +

      "h2{" + Environment.NewLine +
          "font-size:120%;" + Environment.NewLine +
          "font-weight:normal;" + Environment.NewLine +
          "color:#1b1b1b;" + Environment.NewLine +
          "text-align: center;" + Environment.NewLine +
      "}" + Environment.NewLine +
      "td{text-align: center;}" + Environment.NewLine +
      "a{" + Environment.NewLine +
          "text-decoration:none;" + Environment.NewLine +
      "}" + Environment.NewLine +
      "p{" + Environment.NewLine +
          "clear:both;" + Environment.NewLine +
          "margin:0;" + Environment.NewLine +
          "padding:.5em 0;" + Environment.NewLine +
      "}" + Environment.NewLine +
      "img{border:none;}" + Environment.NewLine +
      
      "/*  */" + Environment.NewLine +
      ".m {background-color: #00A0FC; text-decoration: underline;}" + Environment.NewLine +
      ".f {background-color: #E327FF; text-decoration: underline;}" + Environment.NewLine +
      "a { color: ffffff;}"+ Environment.NewLine +
      "a#m:hover {color:#E327FF;}" + Environment.NewLine +
      "a#f:hover {color:#00A0FC;}" + Environment.NewLine +
      "#preview{" + Environment.NewLine +
      "	position:absolute;" + Environment.NewLine +
      "	border:1px solid #ccc;" + Environment.NewLine +
      "	background:#333;" + Environment.NewLine +
      "	padding:5px;" + Environment.NewLine +
      "	display:none;" + Environment.NewLine +
      "	color:#fff;" + Environment.NewLine +
      "	}" + Environment.NewLine +
      "" + Environment.NewLine +
      "/*  */" + Environment.NewLine +
      "</style>";
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
