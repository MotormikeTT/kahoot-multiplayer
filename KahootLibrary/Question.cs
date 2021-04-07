/*
 * Program:         CardsLibrary.dll
 * Module:          Card.cs
 * Date:            January 19, 2021
 * Author:          T. Haworth
 * Description:     The Card class represents a standard playing card. (complete)
 * 
 * Modifications:   Feb 2, 2021
 *                  Added the ICard interface which the card class implements.
 *                  Now hiding the Card class from external clients.
 *                  
 *                  Mar 16, 2021
 *                  Turned the Card class into a WCF data contract
 *                  and removed the ICard interface which we aren't using.
 *                  Note that the Card class needed to be made "public"
 *                  so that the client is now able to see it.
 *                  
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.Runtime.Serialization.dll.
 */

using System;
using System.Runtime.Serialization; // WCF data contract types

namespace KahootLibrary
{
    /*--------------------- Public type definitions --------------------*/



    // The class definition is now invisible from outside the CardsLibrary assembly
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


    } // end Card class
}
