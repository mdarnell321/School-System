using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using MySql;
using MySql.Data.MySqlClient;
using Mysqlx;
using MySqlX.XDevAPI.Common;

public class Program
{
    public static readonly string _host = "localhost";
    public static readonly int _port = 3306;
    public static readonly string _name = "root";
    public static readonly string _pass = "";
    public static readonly string _database = "school";
    public static readonly string _charset = "utf8";

    public static void CloseSqL(MySqlConnection sqlConnection)
    {
        if (sqlConnection == null)
        {
            return;
        }
        if (sqlConnection.State != System.Data.ConnectionState.Closed)
        {
            sqlConnection.Close();
        }
    }
    public static void OpenSql(MySqlConnection sqlConnection)
    {
        if (sqlConnection == null)
        {
            return;
        }
        if (sqlConnection.State != System.Data.ConnectionState.Open)
        {
            sqlConnection.Open();
        }
    }
    public static void DB_Init()
    {
        Stream stream = WebRequest.Create(string.Format("http://{0}/schoolproj/connection.php",_host)).GetResponse().GetResponseStream();
        using (StreamReader sr = new StreamReader(stream))
        {
            sr.ReadToEnd();
        }
    }
    public static void Hunt_Down_Incompletes_Tests() // give zeros
    {
        List<string[]> students = new List<string[]>();
        MySqlConnection sqlconnection = new MySqlConnection(String.Format("SERVER={0}; port={1}; user id={2}; PASSWORD={3}; DATABASE={4};CharSet={5}", _host, _port, _name, _pass, _database, _charset));
        OpenSql(sqlconnection);
        string query_parsed_now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string selectandfrom = "SELECT DISTINCT t.ID AS TID, u.ID AS UID FROM test AS t, modules as m, class AS c, user AS u, class_enrolled_in AS cei ";
        string where = String.Format("WHERE NOT EXISTS(SELECT * FROM test_turned_in as tt WHERE u.id = tt.User_ID AND tt.Test_ID = t.ID) AND t.Module_ID = m.ID AND c.Professor_id = m.Creator_id AND cei.User_ID = u.iD AND cei.Class_ID = c.ID AND t.Due <= '{0}' AND u.ID != c.Professor_id", query_parsed_now);
        string selection = selectandfrom + where;
        MySqlCommand cmd = new MySqlCommand(selection, sqlconnection);
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine(reader["UID"].ToString() + " has failed to complete test " + reader["TID"].ToString());
                students.Add(new string[] { reader["UID"].ToString(), reader["TID"].ToString() });
            }
        }
        for(int i = 0; i < students.Count; i++)
        {
            string query = "INSERT INTO test_turned_in (Test_ID,User_ID, Stamp, Grade) values(@testid ,@userid ,@curtime,'0')";
            MySqlCommand cmd2 = new MySqlCommand();
            cmd2.CommandText = query;
            cmd2.Parameters.AddWithValue("@testid", students[i][1]);
            cmd2.Parameters.AddWithValue("@userid", students[i][0]);
            cmd2.Parameters.AddWithValue("@curtime", query_parsed_now);
            cmd2.Connection = sqlconnection;
            try
            {
                cmd2.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
      
        CloseSqL(sqlconnection);
    }
    public static void Hunt_Down_Incompletes_HW() // give zeros
    {
        List<string[]> students = new List<string[]>();
        MySqlConnection sqlconnection = new MySqlConnection(String.Format("SERVER={0}; port={1}; user id={2}; PASSWORD={3}; DATABASE={4};CharSet={5}", _host, _port, _name, _pass, _database, _charset));
        OpenSql(sqlconnection);
        string query_parsed_now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string selectandfrom = "SELECT DISTINCT hw.ID AS HID, u.ID AS UID FROM assignment AS hw, modules as m, class AS c, user AS u, class_enrolled_in AS cei ";
        string where = String.Format("WHERE NOT EXISTS(SELECT * FROM hw_turned_in as hwt WHERE u.id = hwt.User_ID AND hwt.Assignment_ID = hw.ID) AND hw.Module_ID = m.ID AND c.Professor_id = m.Creator_id AND cei.User_ID = u.iD AND cei.Class_ID = c.ID AND hw.Due <= '{0}' AND u.ID != c.Professor_id", query_parsed_now);
        string selection = selectandfrom + where;
        MySqlCommand cmd = new MySqlCommand(selection, sqlconnection);
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine(reader["UID"].ToString() + " has failed to complete assignment " + reader["HID"].ToString());
                students.Add(new string[] { reader["UID"].ToString(), reader["HID"].ToString() });
            }
        }
        for (int i = 0; i < students.Count; i++)
        {
            string query = "INSERT INTO hw_turned_in (Assignment_ID,User_ID, Stamp, Grade) values(@hwid ,@userid ,@curtime,'0')";
            MySqlCommand cmd2 = new MySqlCommand();
            cmd2.CommandText = query;
            cmd2.Parameters.AddWithValue("@hwid", students[i][1]);
            cmd2.Parameters.AddWithValue("@userid", students[i][0]);
            cmd2.Parameters.AddWithValue("@curtime", query_parsed_now);
            cmd2.Connection = sqlconnection;
            try
            {
                cmd2.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        CloseSqL(sqlconnection);
    }
    public static void End_Semester()
    {
        List<int> students = new List<int>();
        MySqlConnection sqlconnection = new MySqlConnection(String.Format("SERVER={0}; port={1}; user id={2}; PASSWORD={3}; DATABASE={4};CharSet={5}", _host, _port, _name, _pass, _database, _charset));
        OpenSql(sqlconnection);
        {
            string selectandfrom = "SELECT ID FROM user";
            string selection = selectandfrom;
            MySqlCommand cmd = new MySqlCommand(selection, sqlconnection);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    students.Add(int.Parse(reader["ID"].ToString()));
                }
            }
        }
        for (int i = 0; i < students.Count; i++)
        {
            Dictionary<int, List<int[]> /*score with weight*/  > classes = new Dictionary<int, List<int[]>>();
            {//Tests
                string selectandfrom = "SELECT DISTINCT tt.Grade, m.Class_id FROM test_turned_in AS tt, test AS t, modules AS m, class AS c";
                string where = string.Format(" WHERE tt.User_ID = {0} AND tt.Test_ID = t.ID AND t.Module_ID = m.ID AND tt.Grade IS NOT NULL AND c.Professor_id != {0} AND c.ID = m.Class_id", students[i]);
                string selection = selectandfrom + where;
                MySqlCommand cmd = new MySqlCommand(selection, sqlconnection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int index = int.Parse(reader["Class_id"].ToString());
                        int[] val = new int[2] { int.Parse(reader["Grade"].ToString()), 200 };
                        if (!classes.TryAdd(index, new List<int[]> { val }))
                        classes[index].Add(val);
                    }
                }
            }
            {//HW
                string selectandfrom = "SELECT DISTINCT hwt.Grade, m.Class_id FROM hw_turned_in AS hwt, assignment AS a, modules AS m, class AS c";
                string where = string.Format(" WHERE hwt.User_ID = {0} AND hwt.Assignment_ID = a.ID AND a.Module_ID = m.ID AND hwt.Grade IS NOT NULL AND c.Professor_id != {0}", students[i]);
                string selection = selectandfrom + where;
                MySqlCommand cmd = new MySqlCommand(selection, sqlconnection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int index = int.Parse(reader["Class_id"].ToString());
                        int[] val = new int[2] { int.Parse(reader["Grade"].ToString()), 100 };
                        if(!classes.TryAdd(index, new List<int[]> { val }))

                            classes[index].Add(val);
                    }
                }
            }
            foreach (var item in classes)
            {
             
                int grade_total = 0;
                int grade_max = 0;
                for(int ii = 0; ii < item.Value.Count; ii++)
                {
                    grade_total += item.Value[ii][0];
                    grade_max += item.Value[ii][1];
                }
                int final_grade = (int)(((float)grade_total / grade_max) * 100);

                string query = String.Format("UPDATE class_enrolled_in SET Grade=@grade WHERE Class_ID = '{0}' AND User_ID = '{1}'", item.Key, students[i]);
                MySqlCommand curcommand = new MySqlCommand();
                curcommand.CommandText = query;
                curcommand.Parameters.AddWithValue("@grade", final_grade);
                curcommand.Connection = sqlconnection;
                Console.WriteLine("posting grades for user " + students[i] + " in class " + item.Key);
                try
                {
                    curcommand.ExecuteNonQuery();
                }
                catch
                {
                    
                }
            }
        }
        //at this point all grades are posted
        {
            string query = String.Format("UPDATE class SET Professor_id =@pid WHERE Professor_id IS NOT NULL");
            MySqlCommand curcommand = new MySqlCommand();
            curcommand.CommandText = query;
            curcommand.Parameters.AddWithValue("@pid", null);
            curcommand.Connection = sqlconnection;
            try
            {
                curcommand.ExecuteNonQuery();
            }
            catch
            {

            }
        }
        {
            string query = String.Format("DELETE FROM class_enrolled_in WHERE Grade IS NULL");
            MySqlCommand curcommand = new MySqlCommand();
            curcommand.CommandText = query;
            curcommand.Connection = sqlconnection;
            try
            {
                curcommand.ExecuteNonQuery();
            }
            catch
            {

            }
        }

        //delete all submitted work for this class
        {
            string query = String.Format("DELETE FROM test_turned_in");
            MySqlCommand curcommand = new MySqlCommand();
            curcommand.CommandText = query;
            curcommand.Connection = sqlconnection;
            try
            {
                curcommand.ExecuteNonQuery();
            }
            catch
            {

            }
        }
        {
            string query = String.Format("DELETE FROM hw_turned_in");
            MySqlCommand curcommand = new MySqlCommand();
            curcommand.CommandText = query;
            curcommand.Connection = sqlconnection;
            try
            {
                curcommand.ExecuteNonQuery();
            }
            catch
            {

            }
        }
        CloseSqL(sqlconnection);
    }
    public static void Main()
    {
        DB_Init();
        Hunt_Down_Incompletes_Tests();
        Hunt_Down_Incompletes_HW();
        DateTime last = DateTime.Now;
        while (true)
        {
            DateTime now = DateTime.Now;
            if (now < last) // new day
            {
                Hunt_Down_Incompletes_Tests();
                Hunt_Down_Incompletes_HW();
                if(now.Day == 12 && now.Month == 5 || now.Day == 12 && now.Month == 12) // semester done
                {
                    End_Semester();
                }
            }
            Thread.Sleep(1000);
        }

    }

}