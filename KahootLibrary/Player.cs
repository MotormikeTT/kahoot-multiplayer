/*
 * Program:         KahootLibrary.dll
 * Module:          Player.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     The Player class represents a Kahoot Player.
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
