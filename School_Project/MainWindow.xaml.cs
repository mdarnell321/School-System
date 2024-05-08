using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Xml.Linq;
using static School_Project.DatabaseManager;
using static School_Project.UI_Manager;

namespace School_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;

        public MainWindow()
        {
            InitializeComponent();
            ThreadManager.Initialize_Threads();
            this.Closed += (s, a) => { System.Diagnostics.Process.GetCurrentProcess().Kill(); };
            instance = this;
            UI_Manager.ChangeForm(UI_Manager.Forms.Initial);
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            if(DatabaseManager.Login(Login_Email.Text, Login_Pass.Password) == true)
            {
                UI_Manager.Refresh_My_Class_Display((DatabaseManager.Fetch_My_Courses() ?? new List<string[]>()).FindAll(x => x != null && x[3] == "") ?? new List<string[]>());
                UI_Manager.Refresh_My_Finished_Classes_Display((DatabaseManager.Fetch_My_Courses() ?? new List<string[]>()).FindAll(x => x != null && x[3] != "") ?? new List<string[]>());
            }
        }

        private void Reg_Button_Click(object sender, RoutedEventArgs e)
        {
            DatabaseManager.Register(Reg_First.Text, Reg_Last.Text, Reg_Email.Text, Reg_Pass.Password, Reg_Pass_Repeat.Password, (bool)Reg_Imteacher.IsChecked);
        }

        private void All_Courses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(All_Courses.SelectedIndex != -1)
                Catalog_Description.Text = "Professor: " + ((string[])(All_Courses.SelectedItem))[1] + "\n\n" + ((string[])(All_Courses.SelectedItem))[2];
        }
        private void CloseAllWindows(object sender, RoutedEventArgs e)
        {
            View_Catalog.Visibility = Visibility.Hidden;
            Create_Module.Visibility = Visibility.Hidden;

        }
        private void CloseAllSecondaryWindows(object sender, RoutedEventArgs e)
        {
           
            if (Exam_Creator.Visibility == Visibility.Visible || Assignment_Creator.Visibility == Visibility.Visible)
            {
                UI_Manager.Refresh_CreateModule();
            }
            Exam_Creator.Visibility = Visibility.Hidden;
            Assignment_Creator.Visibility = Visibility.Hidden;
            Assignment_Window.Visibility = Visibility.Hidden;
        }
        private void Enroll_In_Click(object sender, RoutedEventArgs e)
        {
            if (All_Courses.SelectedItem == null)
                return;
            string Course_Branch = ((string[])(All_Courses.SelectedItem))[3];
            string Course_Number = ((string[])(All_Courses.SelectedItem))[4];
            if(DatabaseManager.Enroll_In_Course(Course_Branch, Course_Number) == true)
            {
                UI_Manager.Open_Catalog();
                UI_Manager.Refresh_My_Class_Display((DatabaseManager.Fetch_My_Courses() ?? new List<string[]>()).FindAll(x => x != null && x[3] == "") ?? new List<string[]>());
            }
        }

        private void My_Courses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (My_Courses.SelectedItem != null)
            {
                UI_Manager.Load_Class_Display();
                User. is_teaching_class = int.Parse(((string[])My_Courses.SelectedItem)[2]) == User.My_ID;
                if (Modules.ItemsSource != null)
                    UI_Manager.ChangeClassWindow(Class_Windows.Main);
            }

        }

        private void Modules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UI_Manager.Display_Module(((string[])Modules.SelectedItem));
        }

        private void CreateModule_Click(object sender, RoutedEventArgs e)
        {

            TextRange textRange = new TextRange(
                CreateModule_Des.Document.ContentStart,
                CreateModule_Des.Document.ContentEnd
            );
            string test_data = ""; // for questions only
            if(textRange.Text.Contains("|") || textRange.Text.Contains("\\"))
            {
                UI_Manager.ShowMessage("| and \\ are forbidden characters.");
                return;
            }
            if (CreateModule_Name.Text.Contains("|") || CreateModule_Name.Text.Contains("\\"))
            {
                UI_Manager.ShowMessage("| and \\ are forbidden characters.");
                return;
            }
            {//Test Creation
             // | - root seperator
             // ~ - per question
             // ` - per option   
                for (int i = 0; i < TestManager.questions.Count; i++)
                {

                    Question q = TestManager.questions[i];
                    if (q.question.Contains("|") || q.question.Contains("\\") || q.question.Contains("~") || q.question.Contains("`"))
                    {
                        UI_Manager.ShowMessage(string.Format("For question {0}, | \\ ~ ` are forbidden characters.", i+1));
                        return;
                    }
                    
                    test_data += (q.question + "`");
                    int? answer_index = null;
                    for (int ii = 0; ii < q.options.Length; ii++)
                    {
                        if (q.options[ii].Contains("|") || q.options[ii].Contains("\\") || q.options[ii].Contains("~") || q.options[ii].Contains("`"))
                        {
                            UI_Manager.ShowMessage(string.Format("For question {0}, option {1} | \\ ~ ` are forbidden characters.", i + 1, ii+1));
                            return;
                        }
                        if (q.options[ii][0] == '*')
                        {
                            if(answer_index == null)
                                answer_index = ii;
                            else
                            {
                                UI_Manager.ShowMessage("Question " + (i+1) + " has more than one answer. Please fix this.");
                                return;
                            }
                        }   
                        test_data += q.options[ii];
                    }
                    if (answer_index == null)
                    {
                        UI_Manager.ShowMessage("Question " + (i + 1) + " has no answer. Please fix this.");
                        return;
                    }
                    test_data += "`" + answer_index + "~";
                    
                }
             


                if(TestManager.questions.Count > 0)
                {
                    if (CreateExam_DueDate.SelectedDate == null)
                    {
                        UI_Manager.ShowMessage("Due date is undefined. Please fix this.");
                        return;
                    }
                    DateTime used_test_date = new DateTime(CreateExam_DueDate.SelectedDate.Value.Year, CreateExam_DueDate.SelectedDate.Value.Month, CreateExam_DueDate.SelectedDate.Value.Day, 23, 59, 59);
                    DateTime effective_start = (DateTime.Now > new DateTime(DateTime.Now.Year - (DateTime.Now.Month != 12 ? 1 : 0), 12, 12) && DateTime.Now < new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 5, 12)) ?
                        /*spring*/ new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 1, 12) : /*winter*/ new DateTime(DateTime.Now.Year, 8, 12);

                    DateTime effective_end = (DateTime.Now > new DateTime(DateTime.Now.Year - (DateTime.Now.Month != 12 ? 1 : 0), 12, 12) && DateTime.Now < new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 5, 12)) ?
                        /*spring*/ new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 5, 12) : /*winter*/ new DateTime(DateTime.Now.Year, 12, 12);

                    if (used_test_date < effective_start || used_test_date > effective_end)
                    {
                        UI_Manager.ShowMessage(string.Format("Due date is in an invalid range. Range: {0} to {1}", effective_start.AddDays(1).ToString("yyyy-MM-dd"), effective_end.AddDays(-1).ToString("yyyy-MM-dd")));
                        return;
                    }
                    if (used_test_date < DateTime.Now)
                    {
                        UI_Manager.ShowMessage("You can not set a due date for the past. Reminder: Whether spring or fall semester, if you decide to start this class back up, due dates will be recalculated based off offset of respected semester.");
                        return;
                    }

                    
                    test_data += "|" + TestManager.Time_Left + "|" + used_test_date.ToString("yyyy-MM-dd HH:mm:ss");
                   
                }

            }
          

            string hw_data = "";
            {
                if(string.IsNullOrWhiteSpace(CreateHW_Question.Text ) == false)
                {
                    if (CreateHW_DueDate.SelectedDate == null)
                    {
                        UI_Manager.ShowMessage("Due date is undefined. Please fix this.");
                        return;
                    }

                    DateTime used_hw_date = new DateTime(CreateHW_DueDate.SelectedDate.Value.Year, CreateHW_DueDate.SelectedDate.Value.Month, CreateHW_DueDate.SelectedDate.Value.Day, 23, 59, 59);
                    DateTime effective_start = (DateTime.Now > new DateTime(DateTime.Now.Year - (DateTime.Now.Month != 12 ? 1 : 0), 12, 12) && DateTime.Now < new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 5, 12)) ?
                        /*spring*/ new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 1, 12) : /*winter*/ new DateTime(DateTime.Now.Year, 8, 12);

                    DateTime effective_end = (DateTime.Now > new DateTime(DateTime.Now.Year - (DateTime.Now.Month != 12 ? 1 : 0), 12, 12) && DateTime.Now < new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 5, 12)) ?
                        /*spring*/ new DateTime(DateTime.Now.Year + (DateTime.Now.Month == 12 ? 1 : 0), 5, 12) : /*winter*/ new DateTime(DateTime.Now.Year, 12, 12);

                    if (used_hw_date < effective_start || used_hw_date > effective_end)
                    {
                        UI_Manager.ShowMessage(string.Format("Due date is in an invalid range. Range: {0} to {1}", effective_start.AddDays(1).ToString("yyyy-MM-dd"), effective_end.AddDays(-1).ToString("yyyy-MM-dd")));
                        return;
                    }
                    if (used_hw_date < DateTime.Now)
                    {
                        UI_Manager.ShowMessage("You can not set a due date for the past. Reminder: Whether spring or fall semester, if you decide to start this class back up, due dates will be recalculated based off offset of respected semester.");
                        return;
                    }

                    if (CreateHW_Question.Text.Contains("|") || CreateHW_Question.Text.Contains("\\"))
                    {
                        UI_Manager.ShowMessage("| and \\ are forbidden characters.");
                        return;
                    }
                    
                    hw_data = CreateHW_Question.Text + "|" + used_hw_date.ToString("yyyy-MM-dd HH:mm:ss");
                   
                     
                }
            }
            if(DatabaseManager.Create_Module(CreateModule_Name.Text, textRange.Text.Replace("\n", "\\n"), int.Parse(((string[])My_Courses.SelectedItem)[0]), test_data, hw_data) == true)
            {
                UI_Manager.Load_Class_Display();
                TestManager.WipeData();
                UI_Manager.Refresh_CreateModule();
                CloseAllSecondaryWindows(null, null);
                CloseAllWindows(null, null);
            }
          
        }

        private void Create_Module_Button_Click(object sender, RoutedEventArgs e)
        {
            Create_Module.Visibility = Visibility.Visible;
            TestManager.WipeData();
            UI_Manager.Refresh_CreateModule();
            CreateExam_DueDate.DisplayDate = new DateTime();
        }

        private void GoBackModule_Button_Click(object sender, RoutedEventArgs e)
        {
            UI_Manager.ChangeClassWindow(Class_Windows.Main);
        }

        private void AddQuestion_Button_Click(object sender, RoutedEventArgs e)
        {
            CreateExam_Questions.Items.Add("Question " + (CreateExam_Questions.Items.Count + 1));
            TestManager.questions.Add(new Question("",  -1, new string[0]));
        }

        private void CreateExam_Questions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Exam_Creator.Visibility != Visibility.Visible || TestManager.questions.Count == 0)
                return;
            CreateExam_QInput.Text = TestManager.questions[CreateExam_Questions.SelectedIndex].question;
            string temp = "";
            for(int i = 0; i < TestManager.questions[CreateExam_Questions.SelectedIndex].options.Length; i++)
            {
                temp += TestManager.questions[CreateExam_Questions.SelectedIndex].options[i] + "\n";
            }
            CreateExam_DInput.Text = temp;
        }

        private void CreateExam_DInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestManager.questions.Count == 0)
                return;
            Question selected = TestManager.questions[CreateExam_Questions.SelectedIndex];
            selected.options = CreateExam_DInput.Text.Split("\n").ToList().FindAll(x=> String.IsNullOrWhiteSpace(x) == false).ToArray();
          
        }

        private void CreateExam_QInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestManager.questions.Count == 0)
                return;
            Question selected = TestManager.questions[CreateExam_Questions.SelectedIndex];
            selected.question = CreateExam_QInput.Text;
        }

        private void CreateExam_DecreaseTime_Click(object sender, RoutedEventArgs e)
        {
            TestManager.Time_Left = TestManager.Time_Left > 0 ? TestManager.Time_Left - 1 : 0;
            UI_Manager.RefreshTime();
        }

        private void CreateExam_AddTime_Click(object sender, RoutedEventArgs e)
        {
            TestManager.Time_Left++;
            UI_Manager.RefreshTime();
        }

        private void AddHWToModule_Click(object sender, RoutedEventArgs e)
        {
            if(CreateModule_HasExam.IsChecked == true)
            {
                UI_Manager.ShowMessage("This module already contains a test. You cant have both HW & Test.");
                return;
            }
            UI_Manager.Open_CreateHW();
            UI_Manager.Refresh_CreateModule();
        }

        private void AddExamToModule_Click(object sender, RoutedEventArgs e)
        {
            if (CreateModule_HasHW.IsChecked == true)
            {
                UI_Manager.ShowMessage("This module already contains an assignment. You cant have both HW & Test.");
                return;
            }
            UI_Manager.Open_CreateTest();
            TestManager.live_exam = false;
            UI_Manager.Refresh_CreateModule();
        }

        private void DiscardExam_Button_Click(object sender, RoutedEventArgs e)
        {
            TestManager.WipeData();
            UI_Manager.Refresh_CreateModule();
            CloseAllSecondaryWindows(null,null);
        }
        private void DeleteModule_Click(object sender, RoutedEventArgs e)
        {
            int module_id = int.Parse(((string[])((Button)sender).DataContext)[0]);
            if(DatabaseManager.Delete_Module(module_id) == true)
                UI_Manager. Load_Class_Display();
        }

        private void TakeExam_Button_Click(object sender, RoutedEventArgs e)
        {
            TestManager.questions.Clear();
            Exam_Format? exam = DatabaseManager.Take_Exam(int.Parse(((string[])Modules.SelectedItem)[0]));
            if(exam != null)
            {
                TestManager.questions = exam.Value.questions;
                TestManager.Time_Left = exam.Value.time*60 > (int)(exam.Value.due.Subtract(DateTime.Now).TotalSeconds) ? (int)(exam.Value.due.Subtract(DateTime.Now).TotalSeconds) : (exam.Value.time*60);        
                TestManager.live_exam = true;
                TestManager.test_id = exam.Value.id;
                UI_Manager.Open_Exam_Window();
            }
        }

        private void ExamQuestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UI_Manager.Display_Question(ExamQuestions.SelectedIndex);
        }
        public static string GetPopulatedTestData()
        {
            string populated = "";
            for (int i = 0; i < TestManager.questions.Count; i++)
            {
                Question q = TestManager.questions[i];
                populated += q.selected_index + "|";
            }
            return populated;
        }
        private void ExamQuestionBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ExamQuestionBank.SelectedIndex != -1)
            {
                TestManager.questions[ExamQuestions.SelectedIndex].selected_index = ExamQuestionBank.SelectedIndex;
    
                DatabaseManager.Turn_In_Exam((int)TestManager.test_id, GetPopulatedTestData(), false);
            }
           
        }

        private void SubmitTest_Button_Click(object sender, RoutedEventArgs e)
        {
            string test_Data_arr = "";
            for (int i = 0; i < TestManager.questions.Count; i++)
            {
                if(TestManager.questions[i].selected_index == -1)
                {
                    UI_Manager.ShowMessage("Please answer all questions before turning in exam.");
                    return;
                }    
                test_Data_arr += TestManager.questions[i].selected_index + "|";
            }
                 
            
            if(TestManager.test_id != null)
            {
                if (DatabaseManager.Turn_In_Exam((int)TestManager.test_id,test_Data_arr, true))
                {
                    TestManager.WipeData();
                    UI_Manager.ChangeClassWindow(Class_Windows.Main);
                    UI_Manager.Load_Class_Display();
                }  
            } 
        }

        private void DiscardHW_Button_Click(object sender, RoutedEventArgs e)
        {
            CreateHW_Question.Text = "";
            UI_Manager.Refresh_CreateModule();
            CloseAllSecondaryWindows(null, null);
        }

        private void DOHW_Button_Click(object sender, RoutedEventArgs e)
        {
            string[] hw = DatabaseManager.Do_HW(int.Parse(((string[])Modules.SelectedItem)[0]));
            if (hw != null)
            {
                UI_Manager.Open_HW_Window(hw);
            }
        }

        private void SubmitHW_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HW_Answer.Text) == true)
            {
                UI_Manager.ShowMessage("This is not a valid answer.");
                HW_Answer.Text = "";
                return;
            }
            if (DatabaseManager.Turn_In_HW(int.Parse(((string[])Modules.SelectedItem)[0]), HW_Answer.Text))
            {
                UI_Manager.ChangeClassWindow(Class_Windows.Main);
                UI_Manager.Load_Class_Display();
            }
        }

        private void Submit_grade_Click(object sender, RoutedEventArgs e)
        {
            string grade_raw = Grade_inp.Text.Trim();
            int grade_input = 0;
            if (string.IsNullOrWhiteSpace(grade_raw) || grade_raw.All(char.IsDigit) == false || (grade_input = int.Parse(grade_raw)) > 100 || (int.Parse(grade_raw)) < 0)
            {
                UI_Manager.ShowMessage("Invalid grade input.");
                return;
            }

            if (DatabaseManager.Post_Grade(int.Parse(((string[])HWPeople.SelectedItem)[1]), int.Parse(((string[])Modules.SelectedItem)[0]), grade_input) == true)
            {
                GradeHW_Button_Click(null, null);
            }

        }

        private void HWPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(HWPeople.SelectedItem != null)
            UI_Manager.Open_Person_Submission((string[])HWPeople.SelectedItem);
        }

        private void GradeHW_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Modules.SelectedItem == null)
                return;
            List<string[]>? list = DatabaseManager.Get_People_HW(int.Parse(((string[])Modules.SelectedItem)[0]));
            if (list == null)
                return;
            UI_Manager.Open_HWPeople(list);
        }

        private void Enroll_Courses_Click(object sender, RoutedEventArgs e)
        {
            UI_Manager.Open_Catalog();
        }

        private void Teach_Courses_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Courses_Taken_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            My_Courses.SelectedItem = null;
            UI_Manager.ChangeClassWindow(Class_Windows.Info);
        }
    }
}
