﻿/*
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
    // The class definition is now invisible from outside the KahootLibrary assembly
    [DataContract]
    public class Player
    {
        // Private Properties
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

        // Calculates the points to be awarded and the total points achieved thus far
        public void CalculatePoints(int time)
        {
            points?.Add(50 * time);
            TotalPoints = points.Aggregate((total, point) => total += point);
        }

        // Overridden to string method to show the player name and total points
        public override string ToString()
        {
            return $"{Name} with {TotalPoints} point total";
        }
        
        // Overridden equals method to compare the player objects by name
        public override bool Equals(object obj)
        {
            if (!(obj is Player))
                return false;

            return (obj as Player).Name == Name;
        }
    } // end Player class
}
