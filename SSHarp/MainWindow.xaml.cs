using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace SSHarp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Session> sessions;
        string appDataFolder;

        public MainWindow()
        {
            InitializeComponent();
            InitEtc();
            LoadSavedSessions();
            PopulateSavedSessionsListBox();
        }

        private void InitEtc()
        {
            // Check if the folder exists if not create it
            /*
             * string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XaviFortes", "Terminal");
             * Directory.CreateDirectory(appDataFolder);
            */

            appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XaviFortes", "Terminal");

            if (!Directory.Exists(appDataFolder) || !Directory.Exists(Path.Combine(appDataFolder, "TempFiles")))
            {
                Directory.CreateDirectory(appDataFolder);
                Directory.CreateDirectory(Path.Combine(appDataFolder, "TempFiles"));
            }

        }

        private void LastUsedSessionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateSessionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BSave_Click(object sender, RoutedEventArgs e)
        {
            string name = tbName.Text;
            string ip = tbIP.Text;
            string port = tbPort.Text;
            string user = tbUser.Text;
            string password = passwordBox.Password;

            // Create a session object or a data structure to hold the session information
            // For example, you can create a Session class or use a Dictionary or List

            // Add the session information to the session object or data structure
            // For example, if you use a Session class:
            Session session = new Session
            {
                Name = name,
                IP = ip,
                Port = port,
                User = user,
                Password = password
            };

            // Load existing sessions from the file
            List<Session> existingSessions = LoadSessionsFromFile();

            // Add the new session to the existing sessions
            existingSessions.Add(session);

            // Save the updated sessions to the file
            SaveSessionsToFile(existingSessions);

            // Refresh the saved sessions list
            PopulateSavedSessionsListBox();
        }

        private void SaveSessionsToFile(List<Session> sessions)
        {
            // Serialize the session object to a JSON or XML format
            // and save it to a file using a file-saving mechanism of your choice
            // For example, you can use the Newtonsoft.Json NuGet package to serialize the session object to JSON:
            string sessionsJson = Newtonsoft.Json.JsonConvert.SerializeObject(sessions);
            // Save the sessionJson string to a file using System.IO.File.WriteAllText or other file-saving methods
            // For example:
            // string filePath = "sessions.json";
            string filePath = Path.Combine(appDataFolder, "sessions.json");
            System.IO.File.WriteAllText(filePath, sessionsJson);
        }

        private void LoadSavedSessions()
        {
            // Read the sessions from the file
            sessions = LoadSessionsFromFile();
        }

        private List<Session> LoadSessionsFromFile()
        {
            string filePath = Path.Combine(appDataFolder, "sessions.json");

            if (File.Exists(filePath))
            {
                // Read the file contents
                string sessionJson = File.ReadAllText(filePath);

                // Deserialize the sessions from the JSON format
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Session>>(sessionJson);
            }
            else
            {
                // Return an empty list if the file doesn't exist or couldn't be read
                return new List<Session>();
            }
        }

        private void PopulateSavedSessionsListBox()
        {
            // Clear the existing items in the list box
            savedSessionsListBox.Items.Clear();

            // Add the sessions to the list box
            foreach (Session session in sessions)
            {
                savedSessionsListBox.Items.Add(session.Name);
            }
        }

        private void savedSessionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected session from the list box
            string selectedSessionName = savedSessionsListBox.SelectedItem as string;
            if (selectedSessionName != null)
            {
                // Load existing sessions from the file
                List<Session> sessions = LoadSessionsFromFile();

                // Find the selected session by name
                Session selectedSession = sessions.FirstOrDefault(session => session.Name == selectedSessionName);
                if (selectedSession != null)
                {
                    // Open the Terminal window with the session details
                    Terminal terminalWindow = new Terminal(selectedSession);
                    terminalWindow.Show();

                    Close();
                }
            }
        }
    }
}
