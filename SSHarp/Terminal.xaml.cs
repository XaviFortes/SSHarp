using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using Renci.SshNet.Common;
using System.Windows.Controls;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SSHarp
{
    /// <summary>
    /// Interaction logic for Terminal.xaml
    /// </summary>
    public partial class Terminal : Window
    {
        private Session session;
        private SshClient sshClient;
        private ShellStream shellStream;
        private SftpClient sftpClient;
        private Dictionary<string, bool> modifiedFiles = new Dictionary<string, bool>();
        // private string appDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XaviFortes", "Terminal");
        readonly string appDataFolder = Globals.AppDataFolder;
        public Terminal(Session session)
        {
            InitializeComponent();
            InitContextMenu();

            this.session = session;

            // Connect to the SSH server using the session details
            sshClient = new SshClient(
                session.IP,
                Int32.Parse(session.Port),
                session.User,
                session.Password);
            try
            {
                sshClient.Connect();
                DisplayOutput("Connected to SSH server.");
                Debug.WriteLine(sshClient.ConnectionInfo);

                // Create a shell stream
                shellStream = sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                shellStream.DataReceived += ShellStream_DataReceived;
                shellStream.ErrorOccurred += ShellStream_ErrorOccurred;

                // Start reading the shell output asynchronously
                shellStream.BeginRead(new byte[1024], 0, 1024, ShellStream_ReadCallback, null);

                // ** SFTP ** //

                // Connect to the SFTP server
                sftpClient = new SftpClient(sshClient.ConnectionInfo);
                sftpClient.Connect();

                // Display the root directory in the tree view
                // Display the root directory in the tree view
                var rootTreeViewItem = new TreeViewItem();
                rootTreeViewItem.Header = "/";
                rootTreeViewItem.Tag = "/";
                fileExplorerTreeView.Items.Add(rootTreeViewItem);
                DisplayDirectory("/", rootTreeViewItem);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to connect to the SSH server: {ex.Message}");
                MessageBox.Show($"Failed to connect to the SSH server: {ex.Message}");
                Close();
                return;
            }

            // Display the session details
            Title = $"Terminal - {session.Name}";
            /*
            sessionNameLabel.Content = session.Name;
            ipLabel.Content = session.IP;
            portLabel.Content = session.Port;
            userLabel.Content = session.User;
            */
        }

        private void InitContextMenu()
        {
            ContextMenu fileContextMenu = new ContextMenu();
            MenuItem downloadMenuItem = new MenuItem();
            downloadMenuItem.Header = "Download";
            downloadMenuItem.Click += DownloadMenuItem_Click;
            fileContextMenu.Items.Add(downloadMenuItem);

            MenuItem editMenuItem = new MenuItem();
            editMenuItem.Header = "Edit in Notepad++";
            editMenuItem.Click += EditMenuItem_Click;
            fileContextMenu.Items.Add(editMenuItem);

            fileExplorerTreeView.ContextMenu = fileContextMenu;

        }

        private void ShellStream_DataReceived(object sender, ShellDataEventArgs e)
        {
            string output = Encoding.UTF8.GetString(e.Data);
            DisplayOutput(output);
        }

        private void ShellStream_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            Debug.WriteLine($"Shell stream error: {e.Exception.Message}");
            MessageBox.Show($"Shell stream error: {e.Exception.Message}");
        }

        private void ShellStream_ReadCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = shellStream.EndRead(ar);
                if (bytesRead > 0)
                {
                    byte[] buffer = new byte[bytesRead];
                    shellStream.Read(buffer, 0, bytesRead);
                    string output = Encoding.UTF8.GetString(buffer);
                    DisplayOutput(output);

                    // Continue reading the shell output asynchronously
                    shellStream.BeginRead(new byte[1024], 0, 1024, ShellStream_ReadCallback, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Shell stream read error: {ex.Message}");
                MessageBox.Show($"Shell stream read error: {ex.Message}");
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            // Disconnect from the SSH server when the window is closed
            if (sshClient != null && sshClient.IsConnected)
            {
                sshClient.Disconnect();
                sshClient.Dispose();
            }
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement sending commands functionality
            string command = commandInputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(command))
            {
                shellStream.WriteLine(command);
                shellStream.Flush();
            }
        }

        private void DisplayOutput(string output)
        {
            // Split the output into lines
            string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            Dispatcher.Invoke(() =>
            {
                // Process each line and handle ANSI escape sequences
                foreach (string line in lines)
                {
                    Debug.WriteLine($"Line: {line}");
                    if (line.StartsWith("\u001b]0;")) // Window title sequence
                    {
                        // Extract the window title and set the terminal window title
                        string windowTitle = line.Substring(4);
                        // Also remove "~" and everything after it
                        windowTitle = windowTitle.Substring(0, windowTitle.IndexOf(":~"));
                        Debug.WriteLine($"Window title: {windowTitle}");
                        Dispatcher.Invoke(() =>
                        {
                            Title = windowTitle;
                        });
                    }
                    else if (line.StartsWith("\u001b[")) // ANSI escape sequence
                    {
                        // Process ANSI escape sequences or ignore them
                        // You can add custom logic here to handle specific escape sequences
                        // For example, changing text color, text attributes, cursor position, etc.
                        // You can refer to the ANSI escape code standards for more details

                        // Currently, we'll ignore these escape sequences and display the remaining text
                        string remainingText = line.Substring(line.IndexOf('m') + 1);
                        if (!string.IsNullOrEmpty(remainingText))
                        {
                            // AppendText(remainingText);
                            terminalOutputTextBlock.Inlines.Add(new Run(remainingText));
                        }
                    }
                    else
                    {
                        // Append the line to the terminal output
                        terminalOutputTextBlock.Inlines.Add(new Run(line));
                    }

                    // Append a line break
                    terminalOutputTextBlock.Inlines.Add(new LineBreak());
                    // AppendText(Environment.NewLine);
                }

                // Scroll to the end of the text block
                terminalOutputScrollViewer.ScrollToEnd();
            });
        }

        private void DisplayDirectory(string path, TreeViewItem parentTreeViewItem)
        {
            try
            {
                // Get the list of files and directories in the specified path
                var directoryItems = sftpClient.ListDirectory(path);

                foreach (var item in directoryItems)
                {
                    // Create a new tree view item for each file or directory
                    var childTreeViewItem = new TreeViewItem();
                    childTreeViewItem.Header = item.Name;

                    if (item.IsDirectory)
                    {
                        // If the item is a directory, add a dummy item to indicate it can be expanded
                        childTreeViewItem.Tag = item.FullName;
                        childTreeViewItem.Expanded += DirectoryTreeViewItem_Expanded;
                        childTreeViewItem.Items.Add("*");
                    }
                    else
                    {
                        // If the item is a file, add it to the tree view directly
                        childTreeViewItem.Tag = item.FullName;
                    }

                    // Add the child tree view item to the parent tree view item
                    parentTreeViewItem.Items.Add(childTreeViewItem);
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                Debug.WriteLine($"Failed to display directory {path}: {ex.Message}");
            }
        }




        private void DirectoryTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var treeViewItem = sender as TreeViewItem;
            if (treeViewItem.Items.Count == 1 && treeViewItem.Items[0] is string && treeViewItem.Items[0].ToString() == "*")
            {
                // Remove the dummy item
                treeViewItem.Items.Clear();

                // Get the path of the expanded directory
                string path = treeViewItem.Tag.ToString();

                // Get the parent TreeView
                var treeView = FindParent<TreeView>(treeViewItem);

                // Display the contents of the expanded directory
                DisplayDirectory(path, treeViewItem);
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }


        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTreeViewItem = fileExplorerTreeView.SelectedItem as TreeViewItem;
            if (selectedTreeViewItem != null)
            {
                string remotePath = selectedTreeViewItem.Tag.ToString();
                string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), selectedTreeViewItem.Header.ToString());

                try
                {
                    // Download the file
                    using (var fileStream = File.Create(localPath))
                    {
                        sftpClient.DownloadFile(remotePath, fileStream);
                    }

                    MessageBox.Show("File downloaded successfully!");
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    MessageBox.Show(ex.ToString());
                    Debug.Write(ex.ToString());
                }
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string localPath = openFileDialog.FileName;
                string remotePath = "/"; // Specify the remote directory where you want to upload the file

                try
                {
                    // Upload the file
                    using (var fileStream = File.OpenRead(localPath))
                    {
                        sftpClient.UploadFile(fileStream, remotePath);
                    }

                    MessageBox.Show("File uploaded successfully!");
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    MessageBox.Show(ex.ToString());
                    Debug.Write(ex.ToString());
                }
            }
        }

        private void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedTreeViewItem = fileExplorerTreeView.SelectedItem as TreeViewItem;
            if (selectedTreeViewItem != null)
            {
                string remotePath = selectedTreeViewItem.Tag.ToString();
                string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), selectedTreeViewItem.Header.ToString());

                try
                {
                    // Download the file
                    using (var fileStream = File.Create(localPath))
                    {
                        sftpClient.DownloadFile(remotePath, fileStream);
                    }

                    MessageBox.Show("File downloaded successfully!");
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    MessageBox.Show(ex.ToString());
                    Debug.Write(ex.ToString());
                }
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedTreeViewItem = fileExplorerTreeView.SelectedItem as TreeViewItem;
            if (selectedTreeViewItem != null)
            {
                string remotePath = selectedTreeViewItem.Tag.ToString();
                string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), selectedTreeViewItem.Header.ToString());

                try
                {
                    // Download the file to a temporary location also keep the name and extension same as remote file name
                    // string tempFilePath = Path.GetTempFileName() + selectedTreeViewItem.Header.ToString();
                    string tempFileName = Path.GetFileName(selectedTreeViewItem.Tag.ToString());
                    string tempFilePath = Path.Combine(appDataFolder, "TempFiles", tempFileName);

                    Debug.Write(tempFilePath);
                    using (var fileStream = File.Create(tempFilePath))
                    {
                        sftpClient.DownloadFile(remotePath, fileStream);
                    }
                    string fileHash = CalculateMD5(tempFilePath);

                    // Open the file in Notepad++
                    Process notepadProcess = Process.Start("notepad++.exe", tempFilePath);
                    notepadProcess.EnableRaisingEvents = true;
                    notepadProcess.Exited += (s, args) =>
                    {
                        // Check if the file has been modified
                        bool isModified = fileHash != CalculateMD5(tempFilePath);

                        // Update the modification status in the dictionary
                        if (modifiedFiles.ContainsKey(tempFilePath))
                        {
                            modifiedFiles[tempFilePath] = isModified;
                        }
                        else
                        {
                            modifiedFiles.Add(tempFilePath, isModified);
                        }

                        // Prompt the user to upload the modified file
                        if (isModified)
                        {
                            MessageBoxResult result = MessageBox.Show("The file has been modified. Do you want to upload the modified file to the server?", "File Modified", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                UploadModifiedFile(tempFilePath, remotePath);
                            }
                        }

                        // Delete the temporary file
                        File.Delete(tempFilePath);
                    };
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    MessageBox.Show(ex.ToString());
                    Debug.Write(ex.ToString());
                }
            }
        }

        private string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        private void UploadModifiedFile(string localPath, string remotePath)
        {
            try
            {
                // Upload the modified file
                using (var fileStream = File.OpenRead(localPath))
                {
                    sftpClient.UploadFile(fileStream, remotePath, true);
                }

                MessageBox.Show("Modified file uploaded successfully!");
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show(ex.ToString());
                Debug.Write(ex.ToString());
            }
        }


        private void AppendText(string text)
        {
            // Create a new Run with the text
            Run run = new Run(text);

            // Apply the current text color and font weight to the run
            run.Foreground = terminalOutputTextBlock.Foreground;
            run.FontWeight = terminalOutputTextBlock.FontWeight;

            // Append the run to the terminal output text block
            terminalOutputTextBlock.Inlines.Add(run);
        }


        private void commandInputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // If the user presses the Enter key, send the command
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendButton_Click(null, null);
            }
        }
    }
}
