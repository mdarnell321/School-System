using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static School_Project.UI_Manager;

namespace School_Project
{
  
    internal class ThreadManager
    {
        public static Queue<Action> MSGActions = new Queue<Action>();
        public static void Initialize_Threads()
        {
            new Thread(new ThreadStart(Update)).Start();
            new Thread(new ThreadStart(Update_MSG_Thread)).Start();
        }
        public static void Update_MSG_Thread()
        {
            while (true)
            {
                List<Action> temp = new List<Action>();
                if (MSGActions.Count > 0)
                {
                    lock (MSGActions)
                    {
                        while (MSGActions.Count > 0)
                            temp.Add(MSGActions.Dequeue());
                    }
                }

                for (int i = 0; i < temp.Count; ++i)
                    (temp[i])();
            }
        }
        public static void Update()
        {
            while (true)
            {
                if(TestManager.Time_Left > 0 && TestManager.live_exam == true)
                {
                    TestManager.Time_Left--;
                    UI_Manager.RefreshTime();
                }
                else if(TestManager.Time_Left <= 0 && TestManager.live_exam == true)
                {
                    DatabaseManager.Turn_In_Exam((int)TestManager.test_id, MainWindow.GetPopulatedTestData(), true);

                    TestManager.WipeData();
                    UI_Manager.ChangeClassWindow(Class_Windows.Main);
                    MainWindow.instance.Dispatcher.Invoke(() =>
                    {
                        UI_Manager.Load_Class_Display();
                    });
                }
                Thread.Sleep(1000);
            }
        }
    }
}
