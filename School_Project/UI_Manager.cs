using Accessibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using static School_Project.DatabaseManager;

namespace School_Project
{

    internal class UI_Manager
    {
        public enum Forms { Initial, Main };
        public enum Class_Windows { Main, Module, Exam, HW, HW_People, HW_Sub, Info };
        public static void LoadDisplay(bool stat)
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.Loading.Visibility = stat ? Visibility.Visible : Visibility.Hidden;
            });
        }
        public static void ShowMessage(string msg)
        {
            ThreadManager.MSGActions.Enqueue((Action)(() =>
            {
                MessageBox.Show(msg);
            }));
        }
        public static void Open_Catalog()
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.All_Courses.ItemsSource = DatabaseManager.Fetch_Possible_Courses();
                mw_inst.View_Catalog.Visibility = Visibility.Visible;
            });
        }
        public static void ChangeForm(Forms form)
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.Init.Visibility = form == Forms.Initial ? Visibility.Visible : Visibility.Hidden;
                mw_inst.Main.Visibility = form == Forms.Main ? Visibility.Visible : Visibility.Hidden;
            });
        }
        public static void Refresh_My_Class_Display(List<string[]> classes)
        {
            if (classes == null)
                return;
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.My_Courses.ItemsSource = classes;
        }
        public static void Refresh_My_Finished_Classes_Display(List<string[]> classes)
        {
            if (classes == null)
                return;
            MainWindow mw_inst = MainWindow.instance;
 
            mw_inst.Courses_Taken.ItemsSource = classes;

            List<object[]> needed_format = new List<object[]>();
            int total_hours = 0;
            foreach (string[] classes_str in classes)
            {
                float gpa_4scale = Functions.LetterToGPA(Functions.PercentToLetter(int.Parse(classes_str[3])));
                int hrs = gpa_4scale != 0f ? int.Parse(classes_str[4]) : 0;
                total_hours += hrs;
                needed_format.Add(new object[2] { gpa_4scale, hrs });
            }


            mw_inst.my_info.Text = "Valid Credit Hours: " + total_hours + "\r\n" + "GPA: " + Functions.Overall_GPA(needed_format).ToString("0.00") ;
        }
        public static void Display_Question(int i)
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                if (TestManager.questions.Count > 0)
                {
                    mw_inst.ExamQuestion.Text = TestManager.questions[i].question;
                    mw_inst.ExamQuestionBank.ItemsSource = TestManager.questions[i].options;
                    mw_inst.ExamQuestionBank.SelectedIndex = TestManager.questions[i].selected_index;
                }

            });
        }
        public static void Open_HW_Window(string[] arr)
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.HW_Answer.Text = "";
                mw_inst.HW_Question.Text = arr[2];
                mw_inst.Assignment_Window.Visibility = Visibility.Visible;
            });
        }
        public static void Open_Exam_Window()
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < TestManager.questions.Count; i++)
                {
                    mw_inst.ExamQuestions.Items.Add("Question " + (i+1));
                }
                mw_inst.ExamQuestions.SelectedIndex = 0;
                mw_inst.Exam_Window.Visibility = Visibility.Visible;
            });
        }
        public static void Load_Class_Display()
        {
            //test grade weight = 66%
            //hq grade weight = 33%
            MainWindow mw_inst = MainWindow.instance;
            if (mw_inst.My_Courses.SelectedItem == null)
                return;
            mw_inst.Dispatcher.Invoke(() =>
            {

                string[] info = ((string[])mw_inst.My_Courses.SelectedItem);
                mw_inst.Modules.ItemsSource = DatabaseManager.Get_Course_Info(int.Parse(info[0]));
                int grades_total = 0;
                int grades_max = 0;
                {
                    for (int i = 0; i < mw_inst.Modules.Items.Count; i++)
                    {

                        bool is_test = ((string[])(mw_inst.Modules.Items.GetItemAt(i)))[3] == "1";
                        bool is_hw = ((string[])(mw_inst.Modules.Items.GetItemAt(i)))[5] == "1";

                        if (is_test)
                        {
                            string grade = ((string[])(mw_inst.Modules.Items.GetItemAt(i)))[4];
                            if (string.IsNullOrWhiteSpace(grade) == false && grade != "%")
                            {
                                grades_total += int.Parse(grade.Replace("%", "")) * 2;
                                grades_max += 200;
                            }

                        }
                        else
                        if (is_hw)
                        {
                            string grade = ((string[])(mw_inst.Modules.Items.GetItemAt(i)))[6];
                            if (string.IsNullOrWhiteSpace(grade) == false && grade != "%")
                            {
                                grades_total += int.Parse(grade.Replace("%", ""));
                                grades_max += 100;
                            }

                        }

                    }
                }
                if (int.Parse(info[2]) == User.My_ID)
                {
                    mw_inst.Grade_Panel.Visibility = Visibility.Hidden;
                    mw_inst.Create_Module_Button.Visibility = Visibility.Visible;
                }
                else
                {
                    mw_inst.Grade_Panel.Visibility = Visibility.Visible;
                    mw_inst.Create_Module_Button.Visibility = Visibility.Hidden;
                    int? percent = grades_max > 0 ? (int)(((float)grades_total / grades_max) * 100) : null;
                    if (percent == null)
                    {
                        mw_inst.Class_Grade_Text.Text = "Grade: N/A";
                        mw_inst.Class_Grade_Letter.Text = "N/A";
                        return;
                    }
                    mw_inst.Class_Grade_Text.Text = "Grade: " + percent + "%";
                    mw_inst.Class_Grade_Letter.Text = Functions.PercentToLetter((int)percent);

                }
          

            });
        }
        public static void ChangeClassWindow(Class_Windows window)
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {

                mw_inst.Module_Window.Visibility = window == Class_Windows.Main ? Visibility.Visible : Visibility.Hidden;
                if (window == Class_Windows.Main)
                    Load_Class_Display();
                else if(window == Class_Windows.Info)
                {
                    Refresh_My_Finished_Classes_Display((DatabaseManager.Fetch_My_Courses() ?? new List<string[]>()).FindAll(x => x != null && x[3] != "") ?? new List<string[]>());
                    mw_inst.welcome.Text = "Welcome " + (User.im_teacher ? "Professor " : "") + User.Full_name;
                    mw_inst.Teach_Courses.Visibility = User.im_teacher ? Visibility.Visible : Visibility.Hidden;
                    mw_inst.Enroll_Courses.Visibility = !User.im_teacher ? Visibility.Visible : Visibility.Hidden;
                    mw_inst.student_section_main.Visibility = !User.im_teacher ? Visibility.Visible : Visibility.Hidden;
                }
                mw_inst.Module_Opened_Window.Visibility = window == Class_Windows.Module ? Visibility.Visible : Visibility.Hidden;
                mw_inst.Exam_Window.Visibility = window == Class_Windows.Exam ? Visibility.Visible : Visibility.Hidden;
                mw_inst.Assignment_Window.Visibility = window == Class_Windows.HW ? Visibility.Visible : Visibility.Hidden;
                mw_inst.Grading_Window.Visibility = window == Class_Windows.HW_Sub ? Visibility.Visible : Visibility.Hidden;
                mw_inst.User_HW_List.Visibility = window == Class_Windows.HW_People ? Visibility.Visible : Visibility.Hidden;
                mw_inst.Overview_Window.Visibility = window == Class_Windows.Info ? Visibility.Visible : Visibility.Hidden;

            });
        }
        public static void Display_Module(string[] module_data)
        {
            if (module_data == null)
                return;
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                ChangeClassWindow(Class_Windows.Module);


                mw_inst.Module_Title.Text = module_data[1];

                FlowDocument flowdoc = new FlowDocument();
                Paragraph par = new Paragraph();
                par.Inlines.Add(new Run(module_data[2]));
                flowdoc.Blocks.Add(par);
                mw_inst.Module_Des.Document = flowdoc;
                mw_inst.Module_Des.IsReadOnly = true;
                mw_inst.DoHW_Button.Visibility = (module_data[5] == "0" || int.Parse(((string[])mw_inst.My_Courses.SelectedItem)[2]) == User.My_ID ? Visibility.Hidden : Visibility.Visible);
                mw_inst.TakeExam_Button.Visibility = (module_data[3] == "0" || int.Parse(((string[])mw_inst.My_Courses.SelectedItem)[2]) == User.My_ID ? Visibility.Hidden : Visibility.Visible);
                mw_inst.GradeHW_Button.Visibility = int.Parse(((string[])mw_inst.My_Courses.SelectedItem)[2]) != User.My_ID || module_data[3] == "1" || module_data[3] == "0" && module_data[5] == "0" ? Visibility.Hidden : Visibility.Visible;
            });
        }
        public static void Refresh_CreateModule()
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.CreateModule_HasExam.IsChecked = TestManager.questions.Count > 0;
                mw_inst.CreateModule_HasHW.IsChecked = string.IsNullOrWhiteSpace(mw_inst.CreateHW_Question.Text) == false;
            });
        }
        public static void Wipe_Test()
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.CreateExam_Questions.Items.Clear();
                mw_inst.CreateExam_QInput.Text = "";
                mw_inst.CreateExam_DInput.Text = "";
                mw_inst.ExamQuestion.Text = "";
                mw_inst.ExamQuestions.Items.Clear();
            });
        }

        public static void Open_CreateTest()
        {
            MainWindow mw_inst = MainWindow.instance;
            if (TestManager.questions.Count == 0)
            {
                TestManager.questions.Add(new Question("", -1, new string[0]));
                mw_inst.Dispatcher.Invoke(() =>
                {
                    mw_inst.CreateExam_Questions.Items.Clear();
                    mw_inst.CreateExam_Questions.Items.Add("Question 1");
                    mw_inst.CreateExam_Questions.SelectedIndex = 0;
                });
                DateTime plusseven = (DateTime.Now.AddDays(7));
                plusseven = new DateTime(plusseven.Year, plusseven.Month, plusseven.Day, 11, 59, 59);

                mw_inst.CreateExam_DueDate.SelectedDate = new DateTime(plusseven.Year, plusseven.Month, plusseven.Day, 11,59,59);
            }
            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.Exam_Creator.Visibility = Visibility.Visible;
            });
        }
        public static void Open_CreateHW()
        {
            MainWindow mw_inst = MainWindow.instance;

            mw_inst.Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrWhiteSpace(mw_inst.CreateHW_Question.Text))
                {
                    mw_inst.CreateHW_Question.Text = "";

                    DateTime plusseven = (DateTime.Now.AddDays(7));
                    plusseven = new DateTime(plusseven.Year, plusseven.Month, plusseven.Day, 11, 59, 59);
                
                    mw_inst.CreateHW_DueDate.SelectedDate = new DateTime(plusseven.Year, plusseven.Month, plusseven.Day, 11, 59, 59);
                }
                mw_inst.Assignment_Creator.Visibility = Visibility.Visible;
            });
        }
        public static void Open_HWPeople(List<string[]> students)
        {
            MainWindow mw_inst = MainWindow.instance;

            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.HWPeople.ItemsSource = students;
                ChangeClassWindow(Class_Windows.HW_People);
            });
        }
        public static void Open_Person_Submission(string[] info)
        {
            MainWindow mw_inst = MainWindow.instance;

            mw_inst.Dispatcher.Invoke(() =>
            {
                mw_inst.Grading_Who.Text = string.Format("{0}'s Submission For {1}", info[0], ((string[])mw_inst.Modules.SelectedItem)[1]);
                mw_inst.Grading_Field.Text = info[3];
                mw_inst.Grade_inp.Text = "";
                ChangeClassWindow(Class_Windows.HW_Sub);
            });
        }
        public static void RefreshTime()
        {
            MainWindow mw_inst = MainWindow.instance;
            mw_inst.Dispatcher.Invoke(() =>
            {
                if (TestManager.live_exam == false)
                    mw_inst.CreateExam_Time.Text = TestManager.Time_Left.ToString("00") + ":00";
                else
                {
                    TimeSpan ts = TimeSpan.FromSeconds(TestManager.Time_Left);

                    string t = string.Format("{0:D2}:{1:D2}",
                                    ts.Minutes,
                                    ts.Seconds);

                    mw_inst.ExamTimer.Text = t;
                }
            });
        }
    }
}
