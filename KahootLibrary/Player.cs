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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization; // WCF data contract types

namespace KahootLibrary
{
    /*--------------------- Public type definitions --------------------*/



    // The class definition is now invisible from outside the CardsLibrary assembly
    [DataContract]
    public class Player
    {
        private List<int> points = new List<int>();

        /*-------------------------- Constructors --------------------------*/

        internal Player(string n)
        {
            Name = n;
        }

        /*------------------ Public properties and methods -----------------*/

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int TotalPoints { get; set; }

        public void CalculatePoints(int time)
        {
            points?.Add(50 * time);
            TotalPoints = points.Aggregate((total, point) => total += point);
        }

        public override string ToString()
        {
            return $"{Name} {TotalPoints} points";
        }

        public bool Equals(Player p)
        {
            return p.Name == Name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Player))
                return false;

            return (obj as Player).Name == Name;
        }

    } // end Card class
}
