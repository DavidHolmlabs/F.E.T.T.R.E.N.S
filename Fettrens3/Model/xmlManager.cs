using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Windows;

namespace Fettrens
{
    internal static class xmlManager
    {
        private static bool LockOpen = true;
        public static string todayFilenameAndPath { get; private set; }
        private static string settingsFilenameAndPath;
        private static string totalsFilenameAndPath;
        internal static XDocument settingsfile { get; private set; }

        static xmlManager()
        {
            settingsFilenameAndPath = mainPath + "\\settings.xml";
            initPath();
            settingsfile = XDocument.Load(settingsFilenameAndPath);

            todayFilenameAndPath = getLoggPath() + "\\" + getUniqFileName("dayLogg", getLoggPath()) + ".xml";
            totalsFilenameAndPath = getLoggPath() + "\\totalLogg.xml";
        }

        private static void initPath()
        {
            try
            {
                if (!System.IO.Directory.Exists(xmlManager.mainPath))
                {
                    System.IO.Directory.CreateDirectory(xmlManager.mainPath);
                    MessageBox.Show("Skapat katalog: " + xmlManager.mainPath, "Fettrens", MessageBoxButton.OK);
                }
                if (!System.IO.File.Exists(settingsFilenameAndPath))
                {
                    XDocument xd = Setting.getSettingDataTemplate();
                    xd.Save(settingsFilenameAndPath);
                    MessageBox.Show("Skapat fil: " + settingsFilenameAndPath, "Fettrens", MessageBoxButton.OK);
                }
            }
            catch
            {
                MessageBox.Show("Mög i koden. Kunde inte skapa katalog/fil", "Fettrens", MessageBoxButton.OK);
            }
        }

        internal static string mainPath
        {
            get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FettrensLogg");
            }
        }

        private static string getLoggPath()
        {
            if (settingsfile == null)
                return mainPath;
            if (settingsfile.Element("root").Element("LoggPath") != null)
                return settingsfile.Element("root").Element("LoggPath").Value;
            return mainPath;
        }

        private static string getUniqFileName(string tag, string path)
        {
            int i = 0;
            string localFilename = tag + DateTime.Now.ToShortDateString();
            string versionFilename = localFilename;
            while (System.IO.File.Exists(path + "\\" + versionFilename + ".xml"))
            {
                versionFilename = localFilename + "v" + i.ToString();
                i++;
            }
            return versionFilename;
        }

        internal static void UpdateXmlLogg(IEnumerable<TimePost> timePosts)
        {
            if (LockOpen)
            {
                LockOpen = false;
                XDocument xd = new XDocument();
                XElement root = new XElement("root");
                xd.Add(root);
                root.Add(new XAttribute("Date", DateTime.Now.ToShortDateString()));
                root.Add(new XAttribute("Time", DateTime.Now.ToShortTimeString()));

                foreach (TimePost tp in timePosts)
                {
                    if (tp.HasData)
                    {
                        root.Add(tp.getElement());
                    }
                }
                try
                {
                    xd.Save(todayFilenameAndPath);
                }
                catch
                {

                }
                LockOpen = true;
            }
        }

        internal static void UpdateTotals(IList<TimePost> oldTimePosts)
        {
            if (!System.IO.File.Exists(totalsFilenameAndPath))
            {
                XDocument newXD = new XDocument();
                XElement newRoot = new XElement("root");
                newXD.Add(newRoot);
                newRoot.Add(new XElement("updated"));
                newXD.Save(totalsFilenameAndPath);
            }
            XDocument xd = XDocument.Load(totalsFilenameAndPath);
            XElement root = xd.Descendants("root").First();
            foreach (TimePost tp in oldTimePosts)
            {
                root.Add(tp.getElement());
            }
            XElement updated = xd.Descendants("updated").First();
            updated.Value = DateTime.Now.ToShortDateString();
            xd.Save(totalsFilenameAndPath);
        }

        internal static string getStateName(StateMachine.States state)
        {
            XDocument xd = XDocument.Load(settingsFilenameAndPath);
            XElement xe = xd.Element("root").Element("States");

            if (xe.Element(state.ToString()) == null)
            {
                XElement newElement = new XElement(state.ToString());
                newElement.Add(new XElement("Text", state.ToString()));
                newElement.Add(new XElement("Price", 100));
                xe.Add(newElement);
                xd.Save(settingsFilenameAndPath);
            }
            String name = xe.Element(state.ToString()).Element("Text").Value;
            return name;
        }

        internal static int getPricePerHour(StateMachine.States state)
        {
            XDocument xd = XDocument.Load(settingsFilenameAndPath);
            XElement xe = xd.Element("root").Element("States");
            String price = xe.Element(state.ToString()).Element("Price").Value;
            return int.Parse(price);
        }
    }
}
