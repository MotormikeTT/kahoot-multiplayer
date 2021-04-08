/*
 * Program:         CardsLibrary.dll
 * Module:          Shoe.cs
 * Author:          T. Haworth
 * Date:            March 9, 2021
 * Description:     Defines a WCF service contract called IShoe as well as 
 *                  a Shoe class that implements the service. This is just 
 *                  a slightly modified version of the earlier CardsLibrary 
 *                  example.
 *                  
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.ServiceModel.dll.
 *                  
 * Modificatons:    
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;  // WCF types

namespace KahootLibrary
{
    //[ServiceContract]
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGameRules(CallbackGameRulesInfo info);
        [OperationContract(IsOneWay = true)]
        void UpdateInGame(CallbackInGameInfo info);
    }

    // Converted IShoe to a WCF service contract
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGame
    {
        [OperationContract(IsOneWay = true)]
        void StartGame();
        [OperationContract]
        Question GetQuestion();
        [OperationContract]
        bool RegisterPlayer(string playerName);
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
    // of the Shoe class
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Game : IGame
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Question> questions = null;    // collection of cards
        private List<Player> players = null;
        private int questionIdx;                // index of the next card to be dealt
        private int numQuestions;               // number of decks in the shoe
        private int timePerQuestion;
        private string category;
        private List<string> categories;
        private bool gameHost;

        private HashSet<ICallback> callbacks = new HashSet<ICallback>();

        /*-------------------------- Constructors --------------------------*/

        public Game()
        {
            Console.WriteLine($"Creating Game object");

            questions = new List<Question>();
            players = new List<Player>();
            numQuestions = 5;
            timePerQuestion = 15;
            //category = "brain-teasers";
            // populate with catgories from files
            string[] categoriesFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@".\categories\"));
            categoriesFiles = categoriesFiles.Select(file => file.Substring(file.LastIndexOf('\\') + 1).Split('.')[0].Replace("-", " ")).ToArray();
            categories = new List<string>(categoriesFiles);
        }

        /*------------------ Public properties and methods -----------------*/

        // Randomizes the sequence of the Cards in the cards collection
        public void StartGame()
        {
            // Randomize the cards collection
            Random rng = new Random();
            questions = questions.OrderBy(question => rng.Next()).ToList().GetRange(0, numQuestions);

            // Reset the cards index
            questionIdx = 0;

            updateGameRules(true);
        }

        // Returns a copy of the next Card in the cards collection
        public Question GetQuestion()
        {
            if (questionIdx >= questions.Count)
                throw new ArgumentException("No more questions.");

            Question question = questions[questionIdx++];

            updateGameRules(false);

            return question;
        }
        
        /// <summary>
        /// register newely added player
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="startGame"></param>
        /// <returns>registered</returns>
        public bool RegisterPlayer(string playerName)
        {
            Player player = new Player(playerName);
            if (!players.Contains(player))
            {
                players.Add(player);
                updateGameRules(false);
                return true;
            }

            return false;
        }

        // Lets the client read or modify the number of decks in the shoe
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
                    updateGameRules(false);
                }
            }
        }

        // Lets the client read the number of cards remaining in the shoe
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

                    populateQuestions();
                    updateGameRules(false);
                }
            }
        }

        public List<string> Categories
        {
            get
            {
                return categories;
            }
        }

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
                    updateGameRules(false);
                }
            }
        }

        public void RegisterForCallbacks()
        {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Add client's callback object to the collection
            if (!callbacks.Contains(cb))
                callbacks.Add(cb);

            gameHost = callbacks.Count == 1;
            updateGameRules(false);
        }

        public void UnregisterFromCallbacks()
        {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Remove client's callback object from the collection
            if (callbacks.Contains(cb))
                callbacks.Remove(cb);
        }


        /*------------------------- Helper methods -------------------------*/

        // Populates the cards attribute with Card objects
        private void populateQuestions()
        {
            string[] questions = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@".\categories\{category}.txt")).Split(new string[] { "#Q " }, StringSplitOptions.None);
            List<Question> listOfQuestions = new List<Question>();
            foreach (string question in questions)
            {
                string[] questionSplit = question.Trim().Split('\n');
                if (questionSplit.Length == 6)
                {
                    Question newQuestion = new Question(questionSplit[0], questionSplit[1].Remove(0, 2), questionSplit.Skip(2).ToArray());
                    listOfQuestions.Add(newQuestion);
                }
            }
        }

        // Uses the client callback objects to send current Shoe information 
        // to clients. If the change in teh Shoe state was triggered by a method call 
        // from a specific client, then that particular client will be excluded from
        // the update since it will already be updated directly by the call.
        private void updateGameRules(bool startGame)
        {
            // Prepare the CallbackInfo parameter
            CallbackGameRulesInfo info = new CallbackGameRulesInfo(players, category, numQuestions, timePerQuestion, startGame, gameHost);

            // Update all clients
            foreach (ICallback cb in callbacks)
                cb.UpdateGameRules(info);
        }


    } // end Shoe class
}
