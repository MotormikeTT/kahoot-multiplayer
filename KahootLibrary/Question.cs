/*
 * Program:         KahootLibrary.dll
 * Module:          Player.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     The Question class represents a Kahoot Question.
 */

using System;
using System.Runtime.Serialization; // WCF data contract types

namespace KahootLibrary
{
    // The class definition is now invisible from outside the KahootLibrary assembly
    [DataContract]
    public class Question
    {
        /*-------------------------- Constructors --------------------------*/

        internal Question(string s, string a, string[] o)
        {
            Sentence = s;
            Answer = a;
            Options = o;
        }

        /*------------------ Public properties and methods -----------------*/

        [DataMember]
        public string Sentence { get; private set; }
        [DataMember]
        public string Answer { get; private set; }
        [DataMember]
        public string[] Options { get; private set; }
    } // end Question class
}
