using System;
using System.Xml;
using Microsoft.Data.Sqlite;

class Entry
{
    public static void Main()
    {

        var sqlWrapper = new SQLWrapper("./database.db");

        /*
        foreach (var row in sqlWrapper.SQLQuery("SELECT * FROM PersonalSupervisor"))
        {
            Console.WriteLine(row["name"]);
        }
        */


        // STUDENT
        /*
        sqlWrapper.SQLExecute("DROP TABLE IF EXISTS Student ");
        sqlWrapper.SQLExecute("DROP TABLE IF EXISTS PersonalSupervisor");
        sqlWrapper.SQLExecute("DROP TABLE IF EXISTS Meeting");
        sqlWrapper.SQLExecute("DROP TABLE IF EXISTS Report");
        sqlWrapper.SQLExecute("DROP TABLE IF EXISTS SeniorTutor ");
        */

        // STUDENT
        sqlWrapper.SQLExecute(
            "CREATE TABLE Student (student_id INTEGER PRIMARY KEY, supervisor_id INTEGER, name STRING)");

        // SUPERVISOR
        sqlWrapper.SQLExecute(
            "CREATE TABLE PersonalSupervisor (supervisor_id INTEGER PRIMARY KEY, name STRING)");

        // MEETING
        sqlWrapper.SQLExecute(
            "CREATE TABLE Meeting (meeting_id INTEGER PRIMARY KEY, student_id INTEGER NOT NULL, location STRING, date_of_meeting string, " +
            "FOREIGN KEY (student_id) REFERENCES Student(student_id) ON DELETE CASCADE )");

        // REPORT
        sqlWrapper.SQLExecute(
            "CREATE TABLE Report (report_id INTEGER PRIMARY KEY, student_id INTEGER NOT NULL, rating INTEGER, " +
            "FOREIGN KEY (student_id) REFERENCES Student(student_id) ON DELETE CASCADE )");

        // TUTOR
        sqlWrapper.SQLExecute(
            "CREATE TABLE SeniorTutor (tutor_id INTEGER PRIMARY KEY, name STRING)");

        /*
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO PersonalSupervisor (name) VALUES ('jerry')");
        
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Student (supervisor_id, name) VALUES (1, 'bob')");
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Student (supervisor_id, name) VALUES (1, 'matt')");
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Student (supervisor_id, name) VALUES (1, 'clark')");
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Student (supervisor_id, name) VALUES (1, 'kent')");
        
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Meeting (student_id, location, date_of_meeting) VALUES (3, 'room 3', '2023-01-01')");
        
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO PersonalSupervisor (name) VALUES ('andy')");
        
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Student (supervisor_id, name) VALUES (2, 'sock')");
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO Student (supervisor_id, name) VALUES (2, 'rock')");
        
        sqlWrapper.SQLExecuteTransaction(
            "INSERT INTO SeniorTutor (name) VALUES ('mr tutor 2000')");
        */

        sqlWrapper.Close();

        // Console.WriteLine(GetSupervisor(allowCancel:true));

        while (true)
        {
            Console.WriteLine("Select an Option:");
            string[] options = { "New User", "Existing User", "Quit" };
            switch (OptionMenu(options))
            {
                case 0:
                    CreateNewUser();
                    break;
                case 1:
                    UserMenu();
                    break;
                case 2:
                    return;
            }
        }
    }

    static void CreateNewUser()
    {
        var sqlWrapper = new SQLWrapper("./database.db");
        Console.WriteLine("Select User Type:");
        string[] users = { "Student", "Personal Supervisor", "Senior Tutor", "Back" };
        int user = OptionMenu(users);
        string name = "";
        if (user != 3) // as all user types have a name, always ask for a name unless selected "back"
        {
            Console.WriteLine("Enter Name:");
            name = Console.ReadLine();
        }
        switch (user)
        {
            case 0: // student
                int supervisor_id = GetSupervisor(true);
                if (supervisor_id == -1)
                {
                    Console.WriteLine("No Supervisor Selected");
                }
                sqlWrapper.SQLExecuteTransaction(
                    $"INSERT INTO Student (supervisor_id, name) VALUES({(supervisor_id != -1 ? supervisor_id : "NULL")}, '{name}')");
                sqlWrapper.Close();
                break;
            case 1: // supervisor
                sqlWrapper.SQLExecuteTransaction(
                    $"INSERT INTO PersonalSupervisor (name) VALUES ('{name}')");
                break;
            case 2: // tutor
                sqlWrapper.SQLExecuteTransaction(
                    $"INSERT INTO SeniorTutor (name) VALUES ('{name}')");
                break;
            case 3:
                return;
        }

        sqlWrapper.Close();
    }

    static void UserMenu()
    {
        Console.WriteLine("Select User Type:");
        string[] users = { "Student", "Personal Supervisor", "Senior Tutor", "Back" };
        int user = OptionMenu(users);
        switch (user)
        {
            case 0:
                StudentMenu();
                break;
            case 1:
                SupervisorMenu();
                break;
            case 2:
                TutorMenu();
                break;
            case 3:
                return;
        }
    }

    static void StudentMenu()
    {
        int id = GetStudent(true);
        if (id == -1)
        {
            return;
        }

        var sql = new SQLWrapper("./database.db");

        int supervisor_id = -1;
        string name = "";

        foreach (var row in sql.SQLQuery(
                     $"SELECT supervisor_id, name FROM STUDENT " +
                     $"WHERE student_id={id}"))
        {
            if (row["supervisor_id"].GetType() == typeof(DBNull)) // if there is no selected
            {
                Console.WriteLine("No selected supervisor, please select:");
                supervisor_id = GetSupervisor(true);
                if (supervisor_id == -1)
                {
                    Console.WriteLine("No Supervisor Selected");
                    return;
                }
                sql.SQLExecuteTransaction($"UPDATE Student SET supervisor_id == {supervisor_id} WHERE student_id == {id}");
            }
            else
            {
                supervisor_id = int.Parse(row["supervisor_id"].ToString());
            }
            name = row["name"].ToString();
        }

        while (true)
        {
            Console.WriteLine("Select Option:");
            string[] options = { "Make Report", "Creating Meeting", "View Meetings", "Back" };
            int option = OptionMenu(options);
            switch (option)
            {
                case 0:
                    Console.WriteLine("Enter on a scale of one to ten how you have been feeling over the last month:");
                    string rating = Console.ReadLine();
                    sql.SQLExecuteTransaction($"INSERT INTO Report (student_id, rating) VALUES ({id}, {int.Parse(rating)})");
                    break;
                case 1:
                    Console.WriteLine("Enter the date of your meeting:");
                    string date = Console.ReadLine();
                    Console.WriteLine("Enter the location of your meeting:");
                    string location = Console.ReadLine();
                    sql.SQLExecuteTransaction($"INSERT INTO Meeting (student_id, location, date_of_meeting) VALUES ({id}, '{location}', '{date}')");
                    break;
                case 2:
                    Console.WriteLine("Showing Meetings:");
                    int x = 1;
                    foreach (var row in sql.SQLQuery(
                                 $"SELECT PersonalSupervisor.name, location, date_of_meeting " +
                                 $"FROM Meeting JOIN Student ON Meeting.student_id = Student.student_id " +
                                 $"JOIN PersonalSupervisor ON Student.supervisor_id = PersonalSupervisor.supervisor_id " +
                                 $"WHERE Meeting.student_id == {id}"))
                    {
                        Console.WriteLine($"Meeting {x}:\n\tsupervisor name: {row["name"]}\n\tlocation: {row["location"]}\n\tdate: {row["date_of_meeting"]}");
                    }
                    Console.WriteLine("Press Any Key to Continue");
                    Console.ReadKey();
                    break;
                case 3:
                    return;
            }
        }

    }

    static void SupervisorMenu()
    {
        int id = GetSupervisor(true);
        if (id == -1)
        {
            return;
        }

        var sql = new SQLWrapper("./database.db");

        string name = "";

        var student_ids = new List<int>();
        var student_names = new List<string>();

        foreach (var row in sql.SQLQuery(
                     $"SELECT student_id, name FROM Student " +
                     $"WHERE supervisor_id={id}"))
        {
            student_ids.Add(int.Parse(row["student_id"].ToString()));
            student_names.Add(row["name"].ToString());
        }

        student_ids.Add(-1);
        student_names.Add("Back");

        while (true)
        {
            Console.WriteLine("Select an Option:");
            string[] options = { "View Students", "View Meetings", "Back" };
            int option = OptionMenu(options);
            switch (option)
            {
                case 0:
                    Console.WriteLine("Select a Student to Create a Meeting With Them");
                    int selected_student = OptionMenu(student_names.ToArray());
                    int student_id = student_ids[selected_student];
                    if (student_id == -1)
                    {
                        continue;
                    }
                    Console.WriteLine("Enter the date of your meeting:");
                    string date = Console.ReadLine();
                    Console.WriteLine("Enter the location of your meeting:");
                    string location = Console.ReadLine();
                    sql.SQLExecuteTransaction($"INSERT INTO Meeting (student_id, location, date_of_meeting) VALUES ({student_id}, '{location}', '{date}')");
                    break;
                case 1:
                    Console.WriteLine("Showing Meetings:");
                    int x = 1;
                    foreach (var row in sql.SQLQuery(
                                 $"SELECT Student.name, location, date_of_meeting " +
                                 $"FROM Meeting JOIN Student ON Meeting.student_id = Student.student_id " +
                                 $"JOIN PersonalSupervisor ON Student.supervisor_id = PersonalSupervisor.supervisor_id " +
                                 $"WHERE PersonalSupervisor.supervisor_id == {id}"))
                    {
                        Console.WriteLine($"Meeting {x}:\n\tstudent name: {row["name"]}\n\tlocation: {row["location"]}\n\tdate: {row["date_of_meeting"]}");
                    }
                    Console.WriteLine("Press Any Key to Continue");
                    Console.ReadKey();
                    break;
                case 2:
                    return;
            }
        }
    }

    static void TutorMenu()
    {
        int id = GetTutor(true);
        if (id == -1)
        {
            return;
        }

        var sql = new SQLWrapper("./database.db");

        string name = "";

        foreach (var row in sql.SQLQuery(
                     $"SELECT name FROM SeniorTutor " +
                     $"WHERE tutor_id={id}"))
        {
            name = row["name"].ToString();
        }

        var student_ids = new List<int>();
        var student_names = new List<string>();

        foreach (var row in sql.SQLQuery(
                     $"SELECT student_id, name FROM Student"))
        {
            student_ids.Add(int.Parse(row["student_id"].ToString()));
            student_names.Add(row["name"].ToString());
        }

        student_ids.Add(-1);
        student_names.Add("Back");

        while (true)
        {
            Console.WriteLine("Select an Option:");
            string[] options = { "View All Students", "View All Meetings", "Back" };
            int option = OptionMenu(options);
            switch (option)
            {
                case 0:
                    Console.WriteLine("Select a Student to Create a Meeting For Them");
                    int selected_student = OptionMenu(student_names.ToArray());
                    int student_id = student_ids[selected_student];
                    if (student_id == -1)
                    {
                        continue;
                    }
                    Console.WriteLine("Enter the date of the meeting:");
                    string date = Console.ReadLine();
                    Console.WriteLine("Enter the location of the meeting:");
                    string location = Console.ReadLine();
                    sql.SQLExecuteTransaction($"INSERT INTO Meeting (student_id, location, date_of_meeting) VALUES ({student_id}, '{location}', '{date}')");
                    break;
                case 1:
                    Console.WriteLine("Showing Meetings:");
                    int x = 1;
                    foreach (var row in sql.SQLQuery(
                                 $"SELECT Student.name, PersonalSupervisor.name as name2, location, date_of_meeting " +
                                 $"FROM Meeting JOIN Student ON Meeting.student_id = Student.student_id " +
                                 $"JOIN PersonalSupervisor ON Student.supervisor_id = PersonalSupervisor.supervisor_id"))
                    {
                        Console.WriteLine($"Meeting {x}:\n\tstudent name: {row["name"]}\n\tsupervisor name: {row["name2"]}\n\tlocation: {row["location"]}\n\tdate: {row["date_of_meeting"]}");
                    }
                    Console.WriteLine("Press Any Key to Continue");
                    Console.ReadKey();
                    break;
                case 2:
                    return;
            }
        }

    }


    static int GetStudent(bool allowCancel = false)
    {
        var student_ids = new List<int>();
        var students = new List<string>();

        var sql = new SQLWrapper("./database.db");
        foreach (var row in sql.SQLQuery("SELECT student_id, name FROM STUDENT"))
        {
            student_ids.Add(int.Parse(row["student_id"].ToString()));
            students.Add(row["name"].ToString());
        }

        if (allowCancel)
        {
            student_ids.Add(-1);
            students.Add("Cancel");
        }


        Console.WriteLine("Select Student:");
        int student = OptionMenu(students.ToArray());

        return student_ids[student];
    }

    static int GetSupervisor(bool allowCancel = false)
    {
        var ids = new List<int>();
        var names = new List<string>();

        var sql = new SQLWrapper("./database.db");
        foreach (var row in sql.SQLQuery("SELECT supervisor_id, name FROM PersonalSupervisor"))
        {
            ids.Add(int.Parse(row["supervisor_id"].ToString()));
            names.Add(row["name"].ToString());
        }

        if (allowCancel)
        {
            ids.Add(-1);
            names.Add("Cancel");
        }


        Console.WriteLine("Select Personal Supervisor:");
        int name = OptionMenu(names.ToArray());

        return ids[name];
    }

    static int GetTutor(bool allowCancel = false)
    {
        var ids = new List<int>();
        var names = new List<string>();

        var sql = new SQLWrapper("./database.db");
        foreach (var row in sql.SQLQuery("SELECT tutor_id, name FROM SeniorTutor"))
        {
            ids.Add(int.Parse(row["tutor_id"].ToString()));
            names.Add(row["name"].ToString());
        }

        if (allowCancel)
        {
            ids.Add(-1);
            names.Add("Cancel");
        }


        Console.WriteLine("Select Senior Tutor:");
        int name = OptionMenu(names.ToArray());

        return ids[name];
    }

    public static int OptionMenu(string[] options) // a generic menu for any options
    {
        int l_length = 0; // the length of the longest option
        foreach (var option in options)
        {
            if (option.Length > l_length)
            {
                l_length = option.Length;
            }
        }

        (int x, int y) = Console.GetCursorPosition(); // the position of the cursor (where it would be typing) is stored so the console can be reset

        int selected_option = 0;
        while (true) // constantly moves the pointer with arrow keys until an option is selected
        {
            Console.SetCursorPosition(x, y);
            for (int i = 0; i < options.Length; i++)
            {
                if (selected_option == i)
                {
                    Console.WriteLine($"{options[i].PadRight(l_length)} <--");
                }
                else
                {
                    Console.WriteLine($"{options[i].PadRight(l_length)}    ");
                }
            }

            switch (Console.ReadKey(false).Key)
            {
                case ConsoleKey.UpArrow:
                    {
                        if (selected_option == 0)
                        {
                            selected_option = options.Length - 1; // wraps up
                        }
                        else
                        {
                            selected_option -= 1;
                        }
                        break;
                    }
                case ConsoleKey.DownArrow:
                    {
                        selected_option += 1;
                        selected_option %= options.Length; // wraps down
                        break;
                    }
                case ConsoleKey.Enter:
                    {
                        Console.Clear();
                        return selected_option;
                    }
            }
        }
    }
}

class SQLWrapper
{
    private SqliteConnectionStringBuilder connectionStringBuilder;
    private SqliteConnection connection;
    private bool closed;
    public SQLWrapper(string db_name)
    {
        connectionStringBuilder = new SqliteConnectionStringBuilder();
        connectionStringBuilder.DataSource = db_name;

        connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        closed = false;
    }

    ~SQLWrapper()
    {
        Close();
    }

    public void Close()
    {
        if (!closed)
        {
            connection.Close();
            closed = true;
        }
    }

    public void SQLExecute(string commandText)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = commandText;
        cmd.ExecuteNonQuery();
    }

    public void SQLExecuteTransaction(string commandText)
    {
        using (var transaction = connection.BeginTransaction())
        {
            SQLExecute(commandText);

            transaction.Commit();
        }
    }

    public IEnumerable<SqliteDataReader?> SQLQuery(string commandText)
    {

        var cmd = connection.CreateCommand();
        cmd.CommandText = commandText;
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                yield return reader;

            }
        }
    }
}