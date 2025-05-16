using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Data.Sqlite;
using static WPF_Handover.IssuesMain;

namespace WPF_Handover
{
    public partial class IssuesDisplayListElement : UserControl
    {
        public IssuesDisplayListElement()
        {
            InitializeComponent();
            //Display the issue
            //TextBlock issueTextBlock = new TextBlock
            //{
            //    // Set properties
            //    Name = "genIssueTextBlock",
            //    Text = logList[i].issue + $" ({TranslateSeverity(logList[i].severity)})",
            //    FontSize = 30,
            //    Foreground = (logList[i].completed ? Brushes.Gray : Brushes.Black),
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    Background = null,
            //    Cursor = Cursors.Hand,
            //    Margin = new Thickness { Left = 5, Top = 5, Right = 5, Bottom = 2 },
            //    TextWrapping = TextWrapping.Wrap
            //};

        
        }

        public void SetupElements(IssuesMain.IssuesLog issuesLog)
        {
            issueTextBlock.Text = issuesLog.issue;
            issueTextBlock.Foreground = (issuesLog.completed ? Brushes.Gray : Brushes.Black);

            //issueGrid.Background = (issuesLog.completed ? Brushes.White : Brushes.Yellow);

            BitmapImage squareCheckImg = new BitmapImage(new Uri("./Assets/Icons/square-check.png", UriKind.Relative));
            BitmapImage squareImg = new BitmapImage(new Uri("./Assets/Icons/square.png", UriKind.Relative));

            quickCompleteIssueButton.Source = (issuesLog.completed ? squareCheckImg : squareImg);
            quickCompleteIssueButton.MouseEnter += (sender, e) => { quickCompleteIssueButton.Source = (issuesLog.completed ? squareImg : squareCheckImg); };
            quickCompleteIssueButton.MouseLeave += (sender, e) => { quickCompleteIssueButton.Source = (issuesLog.completed ? squareCheckImg : squareImg); };

            issueTextBlock.MouseLeftButtonUp += (sender, e) => ExpandRetractLog(sender, e, issuesLog, issueGrid);

            if (!issuesLog.completed)
            {
                ExpandRetractLog(null, null, issuesLog, issueGrid);
            }

            quickCompleteIssueButton.MouseLeftButtonUp += (sender, e) => CompleteIssueLog(sender, e, issuesLog);
            quickDeleteIssueButton.MouseLeftButtonUp += (sender, e) => DeleteIssueLog(sender, e, issuesLog);
            quickEditIssueButton.MouseLeftButtonUp += (sender, e) => EditIssueLog(sender, e, issuesLog);


        }


        private void DeleteIssueLog(object sender, RoutedEventArgs e, IssuesLog issuesLog)
        {

            string messageBoxText = $"Do you really want to delete '{issuesLog.issue}'?";
            string caption = "Issues Manager";
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Exclamation;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes)
            {
                String query =
                    $"""
                    DELETE FROM issuesTable 
                    WHERE id = {issuesLog.id}; 
                    """;

                String connectionString = "Data Source=issues.db";

                // Using SqliteConnection to establish a connection to the database ...
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    // ... Open the connection, send query and close connection
                    connection.Open();
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.ExecuteReader();
                        connection.Close();
                    }
                }

                IssuesMain.main.UpdateReport();

            }
        }

        private void CompleteIssueLog(object sender, RoutedEventArgs e, IssuesLog issuesLog)
        {

            String query =
                $"""
                UPDATE issuesTable 
                SET completed = {(issuesLog.completed ? 0 : 1)}
                WHERE id = {issuesLog.id}; 
                """;

            String connectionString = "Data Source=issues.db";

            // Using SqliteConnection to establish a connection to the database ...
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                // ... Open the connection, send query and close connection
                connection.Open();
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.ExecuteReader();
                    connection.Close();
                }
            }

            IssuesMain.main.UpdateReport();

            
        }

        private void EditIssueLog(object sender, RoutedEventArgs e, IssuesLog issuesLog)
        {
            IssueEditElement issueEditElement = new IssueEditElement(issuesLog);
            issueEditElement.Name = "issueEditElement";
            IssuesMain.main.issueGrid.Children.Add(issueEditElement);

        }

        private void ExpandRetractLog(object sender, RoutedEventArgs e, IssuesLog issuesLog, Grid issueGrid)
        {
            // Check if the individual issuePanel has more than two children.
            // (Effectively this means that there is one issueTextBox and at least one logTextBlock)
            if (issueGrid.Children.Count > 2)
            {
                Trace.WriteLine("Deleting Childrens...");
                // logTextBlock detected, Deleting every logTextBlock until only issueTextBox remains.

                for (int i = issueGrid.Children.Count - 1; i >= 0; i--)
                { 
                    var children = issueGrid.Children[i];
                    if (children is TextBlock textBox && textBox.Name == "genlogTextBlock")
                    {
                        issueGrid.Children.Remove(children);
                        Trace.WriteLine("Deleted " + children);
                    }
                }

            }
            else
            {
                // Each log is separated by >/>.
                // If such delimiter exists, separate the logs and diplay them.
                // If it does not exist, use the current log.
                String[] logFragmentStringList = [];

                if (issuesLog.log.Contains(">/> "))
                {
                    logFragmentStringList = (issuesLog.log).Split(">/> ");
                }
                else
                {
                    logFragmentStringList = [issuesLog.log];
                }

                Trace.Write("\n\n\n\n\n\n\n" + (issuesLog.log).Split(">/> ") + "\n\n\n\n\n\n\n");

                // Iterate through each Fragments
                for (var i = 0; i < logFragmentStringList.Length; i++)
                {
                    RowDefinition rowDef = new RowDefinition();
                    issueGrid.RowDefinitions.Add(rowDef);
                    string logFragmentString = logFragmentStringList[i];
                    {
                        Trace.WriteLine("Writing " + logFragmentString.Replace(" -?> ", " → "));
                        // Display the log
                        TextBlock logDecTextBlock = new TextBlock
                        {
                            Name = "genlogTextBlock",
                            Text = "↳",
                            FontSize = 20,
                            Width = 20,
                            Foreground = (issuesLog.completed ? Brushes.Gray : Brushes.Black),
                            Margin = new Thickness { Left = 10, Top = 2, Right = 5, Bottom = 2 },
                            TextWrapping = TextWrapping.Wrap
                        };
                        Grid.SetColumn(logDecTextBlock, 0);
                        Grid.SetRow(logDecTextBlock, i + 1);
                        issueGrid.Children.Add(logDecTextBlock);
                        TextBlock logTextBlock = new TextBlock
                        {
                            // Set properties
                            // Each fragment contains -?>, another delimeter meant to separate the actual log and name + date
                            // EG. This is a log -?> XX 00/00/00
                            Name = "genlogTextBlock",
                            Text = logFragmentString.Replace(" -?> ", " → "),
                            FontSize = 20,
                            Foreground = (issuesLog.completed ? Brushes.Gray : Brushes.Black),
                            Margin = new Thickness { Left = 10, Top = 2, Right = 5, Bottom = 2 },
                            TextWrapping = TextWrapping.Wrap
                        };
                        Grid.SetColumn(logTextBlock, 1);
                        Grid.SetColumnSpan(logTextBlock, 3);
                        Grid.SetRow(logTextBlock, i + 1);
                        issueGrid.Children.Add(logTextBlock);
                    }
                }
            }
        }

    }
}
