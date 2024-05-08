using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace School_Project
{
    internal class DatabaseManager
    {
        public static string host = "http://localhost";

        public static void Register(string first, string last, string email, string password, string passwordconfirm, bool teacher)
        {

            if (String.IsNullOrWhiteSpace(first) || String.IsNullOrWhiteSpace(last))
            {
                UI_Manager.ShowMessage("Please enter your name: first and last.");
                return;
            }
            if (String.IsNullOrWhiteSpace(email) || (email).Replace("@", "").Replace(".", "").All(Char.IsLetterOrDigit) == false)
            {
                UI_Manager.ShowMessage("Please enter a valid email.");
                return;
            }
            if (new EmailAddressAttribute().IsValid(email) == false)
            {
                UI_Manager.ShowMessage("Invalid email.");
                return;
            }
            if (String.IsNullOrWhiteSpace(password))
            {
                UI_Manager.ShowMessage("Please enter a password.");
                return;
            }
            if (String.IsNullOrWhiteSpace(passwordconfirm))
            {
                UI_Manager.ShowMessage("Please confirm your password.");
                return;
            }
            if (password != passwordconfirm)
            {
                UI_Manager.ShowMessage("Password do not match.");
                return;
            }
            if ((first+last).All(Char.IsLetter) == false)
            {
                UI_Manager.ShowMessage("Name may only contain letters.");
                return;
            }

            //passed basic client side tests

            first = first.Trim();
            last = last.Trim();

            UI_Manager.LoadDisplay(true);
            string post = Functions.PHPGet(host + "/schoolproj/Register.php", "fn", first, "ln", last, "email", email, "pass", password, "teach", teacher ? "1" : "0");

            if (post.Trim() != "Success")
            {
                UI_Manager.ShowMessage(post);
                UI_Manager.LoadDisplay(false);
                return;
            }
            else
            {
                UI_Manager.ShowMessage("Account successfully created.");
            }
            UI_Manager.LoadDisplay(false);
        }

        public static List<string[]>? Fetch_Possible_Courses() 
        {
            string post = Functions.PHPGet(host + "/schoolproj/GetCourses_All.php", "teach", User.im_teacher == true ? "1" :"0");
            if (post[0] != '/')
                return null;

            List<string[]> temp = new List<string[]>();
            string[] elements = post.Substring(1,post.Length -1).Split('\\');
            for(int i = 0; i < elements.Length - 1; i++)
            {
                string[] attributes = elements[i].Split('|');
                temp.Add(attributes);
            }

            return temp;
        }
        public static List<string[]>? Fetch_My_Courses()
        {
            string post = Functions.PHPGet(host + "/schoolproj/GetCourses_Mine.php", "uid", User.My_ID.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString());
            if (post[0] != '/')
                return null;

            List<string[]> temp = new List<string[]>();
            string[] elements = post.Substring(1, post.Length - 1).Split('\\');
            for (int i = 0; i < elements.Length - 1; i++)
            {
                string[] attributes = elements[i].Split('|');
              
                temp.Add(attributes);
            }

            return temp;
        }

        public static bool Enroll_In_Course(string branch, string number)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Enroll_Course.php", "uid", User.My_ID.ToString(), "cbr", branch, "cn", number, "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return false;
            }
            UI_Manager.ShowMessage("Succesfully enrolled in course.");
            return true;

        }

        public struct Exam_Format
        {
            public int id;
            public DateTime due;
            public float time;

            public List<Question> questions;
        
        }


        public static Exam_Format? Take_Exam(int module)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Take_Exam.php", "mid", module.ToString(), "uid", User.My_ID.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return null;
            }
            Exam_Format exam = new Exam_Format();
            string[] elements = post.Substring(1,post.Length - 1).Split('|');
            string id = elements[0];
            string due = elements[1];
            string limit = elements[2];

            exam.time = int.Parse(limit);
            exam.due = DateTime.Parse(due);
            exam.questions = new List<Question>();
            exam.id = int.Parse(id);

            string[] questions = elements[3].Split('`').ToList().FindAll(x => string.IsNullOrEmpty(x) == false).OrderBy(x => x.Split('~').ToList().FindAll(xx => string.IsNullOrEmpty(xx) == false).ToArray()[0]).ToArray();
            List<string> question_list = new List<string>();
            for (int i = 0; i < questions.Length; i++)
            {
              
                string[] question_data = questions[i].Split('~');

                string _id = question_data[0];
                string _ask = question_data[1];
                int answer_index = int.Parse(question_data[3]);
                List<string> options_list = new List<string>();
                using (System.IO.StringReader reader = new System.IO.StringReader(question_data[2]))
                {
                    string? s = null ;
                    while ((s = reader.ReadLine()) != null)
                        options_list.Add(s[0] == '*' ? s.Substring(1, s.Length -1) : s);
                }
                string[] options = options_list.ToArray();

                exam.questions.Add(new Question(_ask, answer_index, options));
            }
            Turn_In_Exam(exam.id, new StringBuilder().Insert(0, "0|", questions.Count()).ToString(), false);
            return exam;

        }
        public static string[] Do_HW(int module)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Do_HW.php", "mid", module.ToString(), "uid", User.My_ID.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return null;
            }
            string[] elements = post.Substring(1, post.Length - 1).Split('|');
            return elements;
        }

        public static bool Create_Module(string module_name, string module_des, int class_id, string test_q_data, string hw_data)
        {
            module_name= module_name.Replace("'", "//tiaa//");
            module_des = module_des.Replace("'", "//tiaa//");
            test_q_data= test_q_data.Replace("'", "//tiaa//");
            hw_data= hw_data.Replace("'", "//tiaa//");

            string post = Functions.PHPGet(host + "/schoolproj/Create_Module.php", "teach", User.My_ID.ToString(), "mn", module_name, "md", module_des, "cid", class_id.ToString(), "testdata", test_q_data, "hwdata", hw_data, "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return false;
            }
            UI_Manager.ShowMessage("Succesfully created module.");
            return true;

        }
        public static bool Delete_Module(int module_id)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Delete_Module.php", "mid", module_id.ToString(), "uid", User.My_ID.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return false;
            }
            
            return true;

        }
        public static bool Turn_In_Exam(int test_id, string test_data, bool final)
        {

            string post = Functions.PHPGet(host + "/schoolproj/TurnIn_Exam.php", "uid", User.My_ID.ToString(), "tarr", test_data, "tid", test_id.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return false;
            }
            if(final == true)
                UI_Manager.ShowMessage("Test has been turned in.");
            return true;

        }
        public static bool Turn_In_HW(int module_id, string answer)
        {
            string post = Functions.PHPGet(host + "/schoolproj/TurnIn_HW.php", "uid", User.My_ID.ToString(), "ans", answer, "mid", module_id.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return false;
            }
            UI_Manager.ShowMessage("Assignment has been turned in.");
            return true;

        }
        public static bool Post_Grade(int graded, int module_id, int grade)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Push_HW_Grade.php", "tid", User.My_ID.ToString(), "gid", graded.ToString(), "mid", module_id.ToString(), "grade", grade.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString()).Trim();
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return false;
            }
            UI_Manager.ShowMessage("Grade has been posted.");
            return true;

        }
        public static List<string[]>? Get_Course_Info(int class_id)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Course_Info.php", "cid", class_id.ToString(), "uid", User.My_ID.ToString(), "pass", MainWindow.instance.Login_Pass.Password.ToString());
            if (post[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                return null;
            }
            post = post.Replace("//tiaa//", "'");

            List<string[]> temp = new List<string[]>();
            string[] elements = post.Substring(1,post.Length-1).Split('\\');
            for (int i = 0; i < elements.Length - 1; i++)
            {
                string[] attributes = elements[i].Split('|');
                temp.Add(attributes);
            }
            return temp;
        }
        public static List<string[]>? Get_People_HW(int module_id)
        {
            string post = Functions.PHPGet(host + "/schoolproj/Get_HW_People.php", "mid", module_id.ToString());
            if (post[0] != '/')
                return null;

            List<string[]> temp = new List<string[]>();
            string[] students = post.Substring(1,post.Length-1).Split('|');
            for (int i = 0; i < students.Length - 1; i++)
            {
                string[] student_info = students[i].Split('~');
                temp.Add(student_info);
            }
     
            return temp;
        }

        public static bool Login(string email, string password)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                UI_Manager.ShowMessage("Please enter an email.");
                return false;
            }
            if (new EmailAddressAttribute().IsValid(email) == false)
            {
                UI_Manager.ShowMessage("Invalid email.");
                return false;
            }
            if (String.IsNullOrWhiteSpace(password))
            {
                UI_Manager.ShowMessage("Please enter a password.");
                return false;
            }

            UI_Manager.LoadDisplay(true);
            string post = Functions.PHPGet(host + "/schoolproj/Login.php", "email", email, "pass", password);
        
            string trimmed = post.Trim();
            if (trimmed[0] != '/')
            {
                UI_Manager.ShowMessage(post);
                UI_Manager.LoadDisplay(false);
                return false;
            }
            else
            {
                //initiate login sequence
             
                string[] elements = post.Substring(1,post.Length -1).Split('|');
                User.My_ID = int.Parse(elements[0]);
                User.Full_name = elements[1] +  " " + elements[2];
                User.im_teacher = int.Parse(elements[3]) == 1? true : false;
                UI_Manager.ChangeForm(UI_Manager.Forms.Main);
                UI_Manager.ChangeClassWindow(UI_Manager.Class_Windows.Info);
            }
            return true;
        }
    }
}
