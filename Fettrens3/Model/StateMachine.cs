using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fettrens
{
    public class StateMachine
    {
        private static Dictionary<States, String> dictionary = new Dictionary<States, String>();
        public String currentStateText { get { return getStateText(currentState); } }
        public States currentState { get; private set; }
        public string startStateText { get { return dictionary[States.Normal]; } }
        public string initStateText { get { return dictionary[States.Init]; } }

        public enum States { 
            Init,
            Normal,
            Prepare,
            UnplannedRepair,
            UnpaidStop,
            PaiedStop,
            ManualLabor,
            Service,
            Transport,
            InternalError
        }

        public static IEnumerable<States> allStates { 
            get { return new List<States>() { 
                States.Init,
                States.Normal,
                States.Prepare,
                States.UnplannedRepair,
                States.UnpaidStop,
                States.PaiedStop,
                States.Service,
                States.ManualLabor,
                States.Transport
            }; } 
        }

        public StateMachine() 
        {
            populateDictionary();
            currentState = States.Init;
        }

        private void populateDictionary()
        {
            foreach (States st in allStates)
            {
                dictionary.Add(st, xmlManager.getStateName(st));
            }
            dictionary.Add(States.InternalError, "Fel i koden");
        }

        private string getStateText(States st)
        {
            return dictionary[st];
       }

        private States getState(String st)
        {
            foreach (KeyValuePair<States, String> x in dictionary)
            {
                if (x.Value == st) return x.Key;
            }
            throw new ArgumentException("Wrong State text");
            //return States.InternalError;
        }

        internal void changeState(string newStateText)
        {
            States newState = getState(newStateText);
            currentState = newState;
        }

        internal void changeState(States newState)
        {
            currentState = newState;
        }

        internal static States GetState(string stateString)
        {
            foreach (var x in dictionary)
            {
                if (x.Value == stateString)
                {
                    return x.Key;
                }
            }
            return States.InternalError;
        }
    }
}
