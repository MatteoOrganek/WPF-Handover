using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Data.Sqlite;
using SharpVectors.Dom.Svg;
using static WPF_Handover.IssuesMain;

namespace WPF_Handover
{
    public partial class IssueEditElement : UserControl
    {
        public IssueEditElement(IssuesLog issuesLog)
        {
            InitializeComponent();
            // If the issueLog is not null, the user is trying to edit an existing issue
            if (issuesLog != null)
            { 
                idTextBlock.Text = "ID: " + issuesLog.id;
                severitySlider.Value = issuesLog.severity;
                issueTextBox.Text = issuesLog.issue;
                String finalIssuesLog = issuesLog.log;
                if (issuesLog.log.Contains(">/> "))
                {
                    finalIssuesLog = finalIssuesLog.Replace(">/> ", "\n");
                }
                if (issuesLog.log.Contains("-?> "))
                {
                    finalIssuesLog = finalIssuesLog.Replace(" -?> ", " → ");
                }

                logTextBox.Text = finalIssuesLog;

                }

            }
           

            
        public void CloseSelf(object sender, RoutedEventArgs e)
        {
            for (int i = IssuesMain.main.issueGrid.Children.Count - 1; i >= 0; i--)
            {
                var children = IssuesMain.main.issueGrid.Children[i];
                if (children is IssueEditElement issueEditElement )
                {
                    IssuesMain.main.issueGrid.Children.Remove(children);
                    Trace.WriteLine("Deleted " + children);
                }
            }
        }

        // Funtion that triggers every time the slider updates
        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Translate value ((1.0 -> 10.0) to int) to text value and update severityTextBlock.
            severityTextBlock.Text = TranslateSeverity(Convert.ToInt32(e.NewValue));
        }
        // Translate 1 ... 10 int to text value.
        static string TranslateSeverity(int severity)
        {
            switch (severity)
            {
                case 0:
                    return "No Concern";
                case 1:
                    return "Minimal Issue";
                case 2:
                    return "Minor Inconvenience";
                case 3:
                    return "Low Impact";
                case 4:
                    return "Noticeable Issue";
                case 5:
                    return "Moderate Problem";
                case 6:
                    return "Significant Problem";
                case 7:
                    return "Major Problem";
                case 8:
                    return "Critical Problem";
                case 9:
                    return "Severe Problem";
                case 10:
                    return "Complete Disaster";
                default:
                    return "ERROR!";
            }
        }

        public void UpdateDatabaseEntry(object sender, RoutedEventArgs e)
        {
            // Get id from idTextBlock
            // If the id is empty, sqlite will insert the new record id and create a new element in issueTable (issues.db)
            // If the id is not empty it will fetch the current id and edit the selected id.
            string issueId = "";
            if (idTextBlock.Text != "")
            {
                issueId = idTextBlock.Text.Split("ID: ")[1];
            }
            
            // Get time
            DateTime dateTime = DateTime.UtcNow.Date;

            // Get date
            string date = dateTime.ToString("dd/MM/yy");

            // Get manager
            string manager = "Matteo Organek";

            // Get issue from issueTextBox.
            string issue = issueTextBox.Text;

            // Fetch manager initials from full name eg. John Doe -> JD
            string initials = "";
            foreach (string name in manager.Split(" "))
            {
                initials = initials + name[0];
            }

            // Stitch together the log content, initials and date.

            // log1 → XX 00/00/00
            // log2 → XX 00/00/00
            // log3 → XX 00/00/00
            // newlog (With no initials and date)

            // ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

            // log1 -?> XX 00/00/00>/> 
            // log2 -?> XX 00/00/00>/> 
            // log3 -?> XX 00/00/00>/> 
            // newlog -?> XX 00/00/00 

            string log = logTextBox.Text;
            log.Replace(" → ", " -?> ");
            log.Replace("\n", ">/> ");


            if (!(log.Split("→").Last().Length < 15))
            {
                log = log + " -?> " + initials + " " + date + ">/> ";
            }

            // Read severity from slider
            int severity = Convert.ToInt32(severitySlider.Value);

            // Create query
            string query = "";
            if (issueId == "")
            {
                query =
                $"""
                INSERT INTO IssuesTable (manager, issue, log, severity, completed) VALUES
                ("{manager}", "{issue}", "{log}", {severity}, {0}); 
                """;
            } 
            else
            {
                Trace.WriteLine(issueId);
                query =
                $"""
                UPDATE IssuesTable  SET manager = "{manager}", issue = "{issue}", log = "{log}", severity = {severity}, completed = {0}
                WHERE id = {issueId};
                """;
            }
            

            // Connection string to the SQLite database file
            string connectionString = "Data Source=issues.db";

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

            // Clear text from TextBox-es
            issueTextBox.Text = "";
            logTextBox.Text = "";

            CloseSelf(null, null);

            IssuesMain.main.UpdateReport();

            // TODO highlight new log
            // TODO review log

        }
    }
}
