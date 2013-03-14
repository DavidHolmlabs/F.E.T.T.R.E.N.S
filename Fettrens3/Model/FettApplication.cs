using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Collections.ObjectModel;

namespace Fettrens
{
    class FettApplication
    {
        private StateMachine stateMachine;
        private TimePost currentimePost;
        private List<String> Costumers;
        public ObservableCollection<TimePost> TimePostList;// { get { return new ObservableCollection<TimePost>(oldTimePosts); } }
        public XElement XTimePostList;

        public object initStateText { get { return stateMachine.initStateText; } }
        public string currentStateText { get { return stateMachine.currentStateText; } }
        public StateMachine.States currentState { get { return stateMachine.currentState; } }
        public object startStateText { get { return stateMachine.startStateText; } }

        public FettApplication()
        {
            TimePostList = new ObservableCollection<TimePost>();
            XTimePostList = new XElement("Root");
            //oldTimePosts = new List<TimePost>();
            stateMachine = new StateMachine();
            Costumers = GetCostumers();

            currentimePost = new TimePost(new Setting());
        }

        /// <summary>
        /// Saves old timepost, creates new and writes logg
        /// </summary>
        /// <param name="currentSettings"></param>
        internal void NewTimeStamp(Setting currentSettings)
        {
            if (currentimePost == null) return;
            currentimePost.EndTimePost();
            if (currentimePost.HasData)
            {
                TimePostList.Add(currentimePost);
                XTimePostList.Add(currentimePost.getElement());
            }
            currentimePost = new TimePost(currentSettings);
            xmlManager.UpdateXmlLogg(TimePostList);
        }

        /// <summary>
        /// Updates current settings
        /// </summary>
        /// <param name="currentSettings"></param>
        internal void UpdateSettings(Setting currentSettings)
        {
            currentimePost = new TimePost(currentSettings);
        }

        internal void UpdateXMLLogg()
        {
            xmlManager.UpdateXmlLogg(TimePostList);
            xmlManager.UpdateTotals(TimePostList);
        }

        internal string CurrentTime()
        {
            return currentimePost.getTime();
        }

        internal string CurrentCost()
        {
            int pricePerHour = xmlManager.getPricePerHour(currentimePost.settings.state);
            float pricePerSecond = pricePerHour / (float)3600;
            int cost = (int)(currentimePost.getSeconds() * pricePerSecond);
            return cost.ToString();
        }

        internal bool LongStart()
        {
            return stateMachine.currentState == StateMachine.States.Init && currentimePost.getSeconds() > 10;
        }

        internal object[] GetAllStates()
        {
            List<string> a = new List<string>();
            foreach (var item in StateMachine.allStates)
            {
                a.Add(xmlManager.getStateName(item));
            }
            return a.ToArray();
        }

        internal void ChangeState(string p)
        {
            if (stateMachine != null)
            {
                stateMachine.changeState(p);
            }
        }
        
        internal void ChangeState(StateMachine.States p)
        {
            if (stateMachine != null)
            {
                stateMachine.changeState(p);
            }
        }

        internal List<String> GetCostumers()
        {
            return Setting.settingList(Setting.summableCategories.costumer);
        }

        internal object[] GetDrivers()
        {
            return Setting.settingList(Setting.summableCategories.driver).ToArray();
        }

        internal object[] GetTool1Array()
        {
            return Setting.settingList(Setting.summableCategories.tool1).ToArray();
        }

        internal object[] GetTool2Array()
        {
            return Setting.settingList(Setting.summableCategories.tool2).ToArray();
        }

        internal string NextCostumer()
        {
            if (currentimePost.settings.costumer == "")
                return Costumers[1];
            int index = Costumers.IndexOf(currentimePost.settings.costumer);
            int nextIndex = index + 1;
            if (nextIndex >= Costumers.Count)
                nextIndex = 0;
            return Costumers[nextIndex];
        }

        internal string PreviousCostumer()
        {
            if (currentimePost.settings.costumer == "")
                return "";
            int index = Costumers.IndexOf(currentimePost.settings.costumer);
            int previousIndex = index - 1;
            if (previousIndex < 0)
                return Costumers.Last();
            return Costumers[previousIndex];
        }
    }
}
