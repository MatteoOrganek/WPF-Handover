using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using static WPF_Handover.MainWindow;

namespace WPF_Handover
{
    public partial class IssuesMain : Page
    {
        public IssuesMain()
        {
            InitializeComponent();
            //CreateSampleDatabase();
            UpdateReport();
            main = this;
        }

        internal static IssuesMain main;
        public class IssuesLog()
        {
            public required int id { get; set; }
            public required string time { get; set; }
            public required string manager { get; set; }
            public required string issue { get; set; }
            public required string log { get; set; }
            public required int severity { get; set; }
            public required bool completed { get; set; }
        }

        public void UpdateReport()
        {
            // Clear all children from issuePanel
            issuePanel.Children.Clear();

            // Get Issues logs from issue.db
            List<IssuesLog> logList = ReadIssueDatabase();

            // For every IssuesLog in loglist
            for (var i = 0; i < logList.Count; i++)
            {

                // Get individual log
                IssuesLog issuesLog = logList[i];
                // Create new user control element to display every issue
                var control = new IssuesDisplayListElement();
                // Populate the element with the log
                control.SetupElements(issuesLog);
                // Add the user control to issuePanel
                issuePanel.Children.Add(control);

            }

        }


        // Read issues.db and create a new IssuesLog List
        public List<IssuesLog> ReadIssueDatabase()
        {
            // Create empty list
            List<IssuesLog> listLog = new List<IssuesLog>();
            // Connect to issues.db
            using (var connection = new SqliteConnection("Data Source=issues.db"))
            {
                connection.Open();
                // Send a query requesting all IssuesLogs,

                // ↓↓↓↓↓↓↓↓↓
                // ########3
                // --------3
                // ########2
                // --------2
                // --------1
                // ########1

                // sorted by having the completed ones appear on the top ...

                // ########3
                // ########1 ↑ Not completed 
                // ########2
                // --------2
                // --------3 ↓ Completed
                // --------1

                // ... and followed by sorting each group by how severe the issue is.

                // ########1
                // ########2 ↑↓ Severity up/down 
                // ########3

                // --------1
                // --------2 ↑↓ Severity up/down 
                // --------3

                // The output will result in a list of IssuesLogs divided in two groups of completed 
                // and not compleded logs, sorted by severity, showing the most severe ones on the top.


                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT id, time, manager, issue, log, severity, completed
                    FROM IssuesTable
                    ORDER BY
                        completed ASC,
                        severity DESC;
                ";


                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string time = reader.GetString(1);
                        string manager = reader.GetString(2);
                        string issue = reader.GetString(3);
                        string log = reader.GetString(4);
                        int severity = reader.GetInt32(5);
                        bool completed = reader.GetBoolean(6);

                        // Create new issuesLog and fill every entry
                        IssuesLog issuesLog = new IssuesLog()
                        {
                            id = id,
                            time = time,
                            manager = manager,
                            issue = issue,
                            log = log,
                            severity = severity,
                            completed = completed
                        };

                        listLog.Add(issuesLog);


                    }
                }
                return listLog;
            }
        }

        // Sample data to populate the report with example issues.
        // Function used for testing purposes only.
        private static void CreateSampleDatabase()
        {
            {
                // Connection string to the SQLite database file
                string connectionString = "Data Source=issues.db";

                // Using SqliteConnection to establish a connection to the database
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string query = """

                        -- Create a table named "IssuesTable" with the specified columns
                        CREATE TABLE IF NOT EXISTS IssuesTable (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,    -- Unique identifier for each issue
                            time DATETIME DEFAULT CURRENT_TIMESTAMP, -- Time when the issue was logged
                            manager TEXT NOT NULL,                   -- Name of the manager logging the issue
                            issue TEXT NOT NULL,                     -- Description of the issue
                            log TEXT NOT NULL,                       -- Log containing resolution steps and notes
                            severity INTEGER,                        -- Severity of the issue
                            completed BOOLEAN                        -- Completed or not
                        );

                        -- Insert sample data into IssuesTable
                        INSERT INTO IssuesTable (manager, issue, log, severity, completed) VALUES
                            ('John Doe', 'Refrigerator malfunction', 'Checked compressor, requires replacement -?> JD 09/14/23>/> Awaiting parts -?> JD 09/14/23', 3, 0),
                            ('John Doe', 'Shortage of utensils', 'Ordered more utensils from supplier -?> JD 09/13/23>/> Delivery scheduled -?> JD 09/13/23', 2, 1),
                            ('John Doe', 'Staff shortage', 'Reassigned shifts to cover absence -?> JD 09/12/23>/> Full coverage for dinner -?> JD 09/12/23', 2, 1),
                            ('Sarah Smith', 'Leaking sink', 'Plumber contacted, repair scheduled -?> SS 09/14/23>/> Temporary fix applied -?> SS 09/14/23', 3, 0),
                            ('Sarah Smith', 'Slow service', 'Reviewed with wait staff, training scheduled -?> SS 09/13/23>/> Customer followed up -?> SS 09/13/23', 2, 1),
                            ('Sarah Smith', 'POS system down', 'System restarted, tech support contacted -?> SS 09/12/23>/> Issue resolved -?> SS 09/12/23', 3, 1),
                            ('Mark Johnson', 'Late wine delivery', 'Supplier contacted, new delivery scheduled -?> MJ 09/14/23>/> No further issues expected -?> MJ 09/14/23', 2, 1),
                            ('Mark Johnson', 'Incorrect food order', 'Kitchen staff retrained, customer compensated -?> MJ 09/13/23>/> Review scheduled -?> MJ 09/13/23', 2, 1),
                            ('Mark Johnson', 'Broken chair', 'Chair removed from service, replacement ordered -?> MJ 09/12/23>/> Expected next week -?> MJ 09/12/23', 1, 0),
                            ('Mark Johnson', 'Pest sighting', 'Pest control called, inspection scheduled -?> MJ 09/14/23>/> Follow-up next week -?> MJ 09/14/23', 3, 0);
                        -- View the inserted data
                        SELECT * FROM IssuesTable;

                        """;
                    // SqliteCommand to execute the query
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Access data from the reader
                                string someColumnValue = reader["Manager"].ToString();
                                // Do something with the data (e.g., update the UI)
                            }
                        }
                    }
                }
            }
        }
    }
}
