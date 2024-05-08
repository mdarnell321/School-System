using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Project
{
    [System.Serializable]
    public class Question
    {
        public string question = "";
        public int answer;
        public string[] options = new string[0];
        public int selected_index = -1;
        public Question(string q, int a, string[] opt) {
            question = q;
            answer = a;
            options = opt;
        }

    }
    class TestManager
    {
        
        public static List<Question> questions = new List<Question>();
        public static float Time_Left = 10;
        public static bool live_exam = false;
        public static int? test_id;
        public static void WipeData()
        {
            live_exam = false;
            Time_Left = 10;
            questions.Clear();
            
            UI_Manager.Wipe_Test();
        }
    }
}
