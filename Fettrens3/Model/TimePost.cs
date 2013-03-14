using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Fettrens
{
    public class TimePost
    {
        private DateTime StartTime;
        public String StartTimeText 
        { 
            get 
            { 
                return StartTime.ToShortTimeString(); 
            } 
            set 
            {
                DateTime newDateTime;
                if(DateTime.TryParse(value, out newDateTime))
                    StartTime = newDateTime; 
            } 
        }
        public Setting settings {get; set; }
        private DateTime? StopTime = null;
        public bool HasData { 
            get {return settings.HasData; }
        }

        public string Driver { get { return settings.driver; } set {settings.driver = value;} }
        public string Tool1 { get { return settings.tool1; } set { settings.tool1 = value; } }
        public string Tool2 { get { return settings.tool2; } set { settings.tool2 = value; } }
        public string Costumer { get { return settings.costumer; } set { settings.costumer = value; } }
        public string State 
        { 
            get 
            { 
                return xmlManager.getStateName(settings.state); 
            } 
            set 
            { 
                settings.state = StateMachine.GetState(value); 
            } 
        }
        public string Comment { get; set; }

        internal TimePost(Setting s)
        {
            settings = s;
            StartTime = DateTime.Now;
        }

        public void EndTimePost()
        {
            StopTime = DateTime.Now;
        }

        private TimeSpan getTimeSpan()
        {
            TimeSpan x;
            if (StopTime == null)
            {
                x = DateTime.Now - StartTime;
            }
            else
            {
                TimeSpan? xx = StopTime - StartTime;
                x = (TimeSpan)xx; 
            }
            return x;
        }

        public string getTime()
        {
            TimeSpan? x = getTimeSpan();
            if (x != null)
            {
                TimeSpan y = (TimeSpan)x;
                return y.Minutes + " m, " + y.Seconds + " s";
            }
            return "0";
        }

        public int getSeconds()
        {
            TimeSpan? x = getTimeSpan();
            if (x != null)
            {
                TimeSpan y = (TimeSpan)x;
                return (int)y.TotalSeconds;
            }
            return 0;
        }

        internal int getMinutes()
        {
            TimeSpan? x = getTimeSpan();
            if (x != null)
            {
                TimeSpan y = (TimeSpan)x;
                return y.Minutes + 60 * y.Hours;
            }
            return 0;
        }

        internal string getValue(Setting.summableCategories cat)
        {
            switch (cat)
            {
                case Setting.summableCategories.costumer:
                    return settings.costumer;
                case Setting.summableCategories.driver:
                    return settings.driver;
                case Setting.summableCategories.tool1:
                    return settings.tool1;
                case Setting.summableCategories.tool2:
                    return settings.tool2;
                case Setting.summableCategories.state:
                    return settings.state.ToString();
                default:
                    throw new NotSupportedException();
            }
        }

        internal XElement getElement()
        {
            XElement loggpost = new XElement("loggpost");
            loggpost.Add(new XElement("Driver", settings.driver));
            loggpost.Add(new XElement("Costumer", settings.costumer));
            loggpost.Add(new XElement("Tool1", settings.tool1));
            loggpost.Add(new XElement("Tool2", settings.tool2));
            loggpost.Add(new XElement("State", xmlManager.getStateName( settings.state )));
            loggpost.Add(new XElement("StartTime", StartTime.ToShortTimeString()));
            loggpost.Add(new XElement("StartDate", StartTime.ToShortDateString()));
            loggpost.Add(new XElement("Comment", Comment));
            if (StopTime != null)
            {
                loggpost.Add(new XElement("StopTime", ((DateTime)StopTime).ToShortTimeString()));
            }
            int seconds = (int)getTimeSpan().TotalSeconds;
            loggpost.Add(new XElement("Seconds", seconds.ToString()));

            return loggpost;
        }

        public override string ToString()
        {
            return State + ", " + Driver + ", " + Costumer;
        }
    }
}
