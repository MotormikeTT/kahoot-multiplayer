/*
 * Program:         KahootLibrary.dll
 * Module:          Game.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     Defines a WCF service contract called IGame as well as 
 *                  a Game class that implements the service.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;  // WCF types

namespace KahootLibrary
{
    // ICallback interface
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGameRules(CallbackGameRulesInfo info);
        [OperationContract(IsOneWay = true)]
        void UpdateInGame(CallbackInGameInfo info);
    }

    // Converted IGame to a WCF service contract
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGame
    {
        [OperationContract]
        bool StartGame();
        [OperationContract(IsOneWay = true)]
        void EndGame();
        [OperationContract]
        void GetNextQuestion();
        [OperationContract]
        bool CheckAnswer(string answer, int currentPlayerIdx, int time);
        [OperationContract]
        int RegisterPlayer(string playerName);
        int NumQuestions { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        string Category { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        List<string> Categories { [OperationContract] get; }
        int TimePerQuestion { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        [OperationContract(IsOneWay = true)]
        void RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterFromCallbacks();

    }

    // The class that implements the service
    // ServiceBehavior is used here to select the desired instancing behaviour
    // of the Game class
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Game : IGame
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Question> questions = null;    // collection of questions
        private List<Player> players = null;
        private int questionIdx;                // index of the next question to be dealt
        private int numQuestions;               // number of questions in the game
        private int timePerQuestion;
        private string category;
        private List<string> categories;

        private HashSet<ICallback> callbacks = new HashSet<ICallback>();

        /*-------------------------- Constructors --------------------------*/

        public Game()
        {
            Console.WriteLine($"Creating Game object");

            questions = new List<Question>();
            players = new List<Player>();
            numQuestions = 5;
            timePerQuestion = 15;
            // populate with catgories from files
            string[] categoriesFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@".\categories\"));
            categoriesFiles = categoriesFiles.Select(file => file.Substring(file.LastIndexOf('\\') + 1).Split('.')[0].Replace("-", " ")).ToArray();
            categories = new List<string>(categoriesFiles);
            category = Categories.FirstOrDefault();
            populateQuestions();
        }

        /*------------------ Public properties and methods -----------------*/

        // Randomizes the sequence of the questions in the questions collection and sets the number of questions to be used
        public bool StartGame()
        {
            // only start when all players are ready
            if (players.Count != callbacks.Count)
                return false;

            // Randomize the questions collection
            populateQuestions();
            Random rng = new Random();
            questions = questions.OrderBy(question => rng.Next(0, questions.Count).ToString()).ToList().GetRange(0, numQuestions);

            // Reset the question index
            questionIdx = 0;

            updateInGameInfo(false);
            return true;
        }

        // Ends the game by clearing the player list and updating the game rules
        public void EndGame()
        {
            updateGameRules();
            players.Clear();
        }

        // Gets the next question in the question list until there is no more
        public void GetNextQuestion()
        {
            bool endGame = false;
            if (questionIdx >= questions.Count - 1)
                endGame = true;
            else
                questionIdx++;

            updateInGameInfo(endGame);
        }

        // Checks to see if the user's chosen answer is correct and clculates their points if it is
        public bool CheckAnswer(string answer, int playerIndex, int time)
        {
            if (questions[questionIdx].Answer.Equals(answer))
            {
                players[playerIndex].CalculatePoints(time);
                return true;
            }
            else
                return false;
        }

        // Register newly added player
        public int RegisterPlayer(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                return -1;
            Player player = new Player(playerName);
            if (!players.Contains(player))
            {
                players.Add(player);
                updateGameRules();
                return players.Count - 1;
            }

            return players.IndexOf(player);
        }

        // Lets the client read or modify the number of questions in the game
        public int NumQuestions
        {
            get
            {
                return numQuestions;
            }
            set
            {
                if (numQuestions != value)
                {
                    numQuestions = value;
                    updateGameRules();
                }
            }
        }

        // Lets the client read the selected category
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                string fileName = value.Replace(" ", "-");
                if (category != fileName)
                {
                    category = fileName;

                    updateGameRules();
                }
            }
        }

        // Lets the client read the categories available to choose
        public List<string> Categories
        {
            get
            {
                return categories;
            }
        }

        // Lets the client read or modify the time allotted per question
        public int TimePerQuestion
        {
            get
            {
                return timePerQuestion;
            }
            set
            {
                if (timePerQuestion != value)
                {
                    timePerQuestion = value;
                    updateGameRules();
                }
            }
        }

        // Registers a client for callbacks
        public void RegisterForCallbacks()
        {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Add client's callback object to the collection
            if (!callbacks.Contains(cb))
                callbacks.Add(cb);

            updateGameRules();
        }

        // Removes a client from being called back
        public void UnregisterFromCallbacks()
        {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Remove client's callback object from the collection
            if (callbacks.Contains(cb))
                callbacks.Remove(cb);
        }


        /*------------------------- Helper methods -------------------------*/

        // Populates the questions attribute with Question objects
        private void populateQuestions()
        {
            string[] questionsArray = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@".\categories\{category}.txt")).Split(new string[] { "#Q " }, StringSplitOptions.None);
            foreach (string question in questionsArray)
            {
                string[] questionSplit = question.Replace("\r\n\r\n", "\r\n").Trim().Split('\n');
                if (questionSplit.Length == 6)
                {
                    Question newQuestion = new Question(questionSplit[0], questionSplit[1].Remove(0, 2), questionSplit.Skip(2).Select(x => x.Remove(0,2)).ToArray());
                    questions.Add(newQuestion);
                }
            }
        }

        // Uses the client callback objects to send current Game Rules information to clients.
        private void updateGameRules()
        {
            // Update all clients
            foreach (ICallback cb in callbacks)
            {
                // only the game host recieves the game host flag as true
                bool gameHost = cb == callbacks.ElementAt(0);
                // Prepare the CallbackInfo parameter
                CallbackGameRulesInfo info = new CallbackGameRulesInfo(players, category, numQuestions, timePerQuestion, gameHost);
                cb.UpdateGameRules(info);
            }
        }

        // Uses the client callback objects to send current In Game information to clients.
        private void updateInGameInfo(bool endGame)
        {
            // Update all clients
            foreach (ICallback cb in callbacks)
            {
                // only the game host recieves the game host flag as true
                bool gameHost = cb == callbacks.ElementAt(0);
                // Prepare the CallbackInfo parameter
                CallbackInGameInfo info = new CallbackInGameInfo(players, questions[questionIdx], endGame, gameHost);
                cb.UpdateInGame(info);
            }
        }
    } // end Game class
}
