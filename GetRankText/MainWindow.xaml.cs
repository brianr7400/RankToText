using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Threading;

namespace GetRankText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string _apikey;
        static string _SummonerName;
        static int Minutes;
        static string Location;
        static int TimeNext;
        static bool toggle = false;

        static DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            LocationBox.Text = path;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Stop();
        }
        public void SaveFile(string text)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LeagueRank.txt";
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(text);
                }
        }
        public string GetRank()
        {
            string SumID = GetSummonerID();
            string Rank = "";
            try
            {
                //creates web client
                var Client = new WebClient();
                Client.Proxy = null;
                //makes the url and downloads the json string
                string url = ("https://na.api.pvp.net/api/lol/na/v2.5/league/by-summoner/" + SumID + "?api_key=" + _apikey);
                var rankedData = Client.DownloadString(url);
                //creates an object to hold the json string and parses it
                //If you ever need to fix this just rewrite it (logic too complicated to comment)
                JObject tempjsonLeagueData = JObject.Parse(rankedData);
                JArray jsonLeagueData = JArray.Parse(tempjsonLeagueData[SumID].ToString());
                JArray jsonRankData = JArray.Parse(jsonLeagueData[0]["entries"].ToString());

                //Get array line for data
                int arrayLength = jsonRankData.Count();
                int correctArrayLine = 0;

                for (int line = 0; line < arrayLength; line++)
                {
                    if ((string)jsonRankData[line]["playerOrTeamId"] == SumID)
                    {
                        correctArrayLine = line;
                        break;
                    }
                }

                string tier = (string)jsonLeagueData[0]["tier"];
                string division = (string)jsonRankData[correctArrayLine]["division"];
                string leaguePoints = (string)jsonRankData[correctArrayLine]["leaguePoints"];
                if (Convert.ToInt32(leaguePoints) == 100)
                {
                    string series = (string)jsonRankData[correctArrayLine]["miniSeries"]["progress"];
                }
                Rank = String.Format("{0} {1} {2}LP", tier, division, leaguePoints);
                //Closes Client session
                Client.Dispose();
            }
            catch
            {
                
            }
            
            return Rank;
        }
        public string GetSummonerID()
        {
            //Creates WebClient
            using (var Client = new WebClient())
            {
                Client.Proxy = null;

                //Gets Summoner information | name, id, profileIconId, summonerLevel, revisionDate
                string url = ("https://na.api.pvp.net/api/lol/na/v1.4/summoner/by-name/" + _SummonerName.Replace(" ", "%20") + "?api_key=" + _apikey);
                var SummonerData = Client.DownloadString(url);

                //Puts summoner information into json
                JObject jsonSummonerData = JObject.Parse(SummonerData);
                //Removes spaces from variable _SummonerName 
                _SummonerName = _SummonerName.Replace(" ", "");

                //Sets currentusers properties to the correct json ones;
                string SummonerID = (string)(jsonSummonerData[_SummonerName.ToLower()]["id"]);
                return SummonerID;
            }
        }

        //Timer
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            TimeNext--;
            if (TimeNext <= 0)
            {
                SaveFile(GetRank());
                TimeNext = 300;
            }
            TimeLabel.Text = String.Format("Time till next update: {0} Sec", TimeNext);
        }
        //Controlls
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            toggle = !toggle;
            if (toggle == true)
            {
                ToggleButton.Content = "Stop";
                _apikey = ApiBox.Text;
                _SummonerName = SummonerBox.Text;
                Location = LocationBox.Text;
                SaveFile(GetRank());
                dispatcherTimer.Start();
                TimeNext = 300;
            }
            else
            {
                ToggleButton.Content = "Start";
                dispatcherTimer.Stop();
            }
            
        }



    }

}
