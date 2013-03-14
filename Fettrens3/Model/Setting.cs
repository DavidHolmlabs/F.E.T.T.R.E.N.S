using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Fettrens
{
    public class Setting
    {
        public string driver { get; set; }
        public string tool1 { get; set; }
        public string tool2 { get; set; }
        public string costumer { get; set; }
        public StateMachine.States state { get; set; }
        public bool HasData { get; set; }

        public Setting(string _driver, string _tool1, string _tool2, string _customer, string _state )
        {
            driver = _driver;
            tool1 = _tool1;
            tool2 = _tool2;
            costumer = _customer;
            state = StateMachine.GetState(_state);
            HasData = true;
        }

        public Setting() 
        {
            driver = "";
            tool1 = "";
            tool2 = "";
            costumer = "";
            state = StateMachine.States.Init;
            HasData = false;
        }

        public enum summableCategories { driver, tool1, tool2, costumer, state };

        public static List<summableCategories> allSummableCategories()
        {
            return new List<summableCategories>{
                    summableCategories.costumer,
                    summableCategories.driver,
                    summableCategories.state,
                    summableCategories.tool1,
                    summableCategories.tool2
                };
        }

        public static XDocument getSettingDataTemplate()
        {
            XElement root = new XElement("root");
            root.Add(getTemplate(Setting.summableCategories.driver));
            root.Add(getTemplate(Setting.summableCategories.costumer));
            root.Add(getTemplate(Setting.summableCategories.tool1));
            root.Add(getTemplate(Setting.summableCategories.tool2));

            XElement states = new XElement("States");
            foreach (StateMachine.States state in StateMachine.allStates) 
            {
                XElement xe = new XElement(state.ToString());
                xe.Add(new XElement("Text", state.ToString()));
                xe.Add(new XElement("Price", 100));
                states.Add(xe);
            }
            root.Add(states);
            root.Add(new XElement("LoggPath"));
            return new XDocument(root);
        }

        private static XElement getTemplate(summableCategories summableCategories)
        {
            XElement element = new XElement(summableCategories.ToString());
            element.Add(new XElement(summableCategories.ToString() + "item", "Allan"));
            element.Add(new XElement(summableCategories.ToString() + "item", "Bertil"));
            element.Add(new XElement(summableCategories.ToString() + "item", "Cesar"));
            return element;
        }

        public static List<string> settingList(summableCategories category)
        {
            List<XElement> elementList = xmlManager.settingsfile.Descendants(category.ToString() + "item").ToList<XElement>();
            List<String> resultList = new List<string>();
            foreach (XElement xe in elementList) 
            {
                resultList.Add(xe.Value);
            }
            return resultList;
        }
    }
}