/*
 * Program:         KahootGuiClient.exe
 * Module:          MainWindow.xaml.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     A Windows WPF client that uses KahootLibrary.dll via a WCF service.
 */

using System;
using System.Linq;
using System.Windows;
using System.ServiceModel;  // WCF  types
using KahootLibrary;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace CardsGUIClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        // Private member variables
        private IGame game = null;
        private int clientIdx;
        private DispatcherTimer timer;
        private TimeSpan time;

        // C'tor
        public MainWindow()
        {
            InitializeComponent();

            // Connect to the WCF service endpoint 
            DuplexChannelFactory<IGame> channel = new DuplexChannelFactory<IGame>(this, "KahootEndPoint");
            game = channel.CreateChannel();

            // Register for the callback service
            game.RegisterForCallbacks();

            // Initialize the GUI
            sliderQuestions.Minimum = 1;
            sliderQuestions.Maximum = 20;
            sliderQuestions.Value = game.NumQuestions;
            comboCategories.ItemsSource = game.Categories;

            // by default
            btnStart.Visibility = Visibility.Hidden;
            disableRuleControls(true);
        } // end default C'tor

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING
                clientIdx = game.RegisterPlayer(txtPlayerName.Text);

                if (clientIdx == -1)
                {
                    txtPlayerName.Text = "";
                    MessageBox.Show("Name is already taken or empty. please use a different one", null, MessageBoxButton.OK);
                }
                else
                {
                    btnReady.IsEnabled = false;
                    txtPlayerName.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end btnReady_Click()

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING
                clientIdx = game.RegisterPlayer(txtPlayerName.Text);

                if (clientIdx == -1)
                {
                    txtPlayerName.Text = "";
                    MessageBox.Show("Name is already taken or empty. please use a different one", null, MessageBoxButton.OK);
                }
                else
                {
                    if (game.StartGame())
                        txtPlayerName.IsEnabled = false;
                    else
                        MessageBox.Show("Wait until all players are ready", "Cannot start game!", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end btnStart_Click()

        private void btnAnswer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING
                if (game.CheckAnswer(((e.Source as Button).Content as TextBlock).Text, clientIdx, (int)time.TotalSeconds))
                    labelResult.Text = "Correct Answer";
                else
                    labelResult.Text = "Wrong Answer";
                // Update the GUI
                disableAnswerButtons(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end btnAnswer_Click()

        private void sliderQuestions_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (game != null)
                {
                    game.NumQuestions = (int)sliderQuestions.Value;

                    // update GUI
                    txtQuestionCount.Text = $"{sliderQuestions.Value} Questions";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end sliderQuestions_ValueChanged()


        private void comboCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (game != null)
                {
                    game.Category = (sender as ComboBox).SelectedValue.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end txtCategories_SelectionChanged()

        private void txtTimePerQuestion_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (game != null && !string.IsNullOrEmpty((sender as TextBox).Text))
                {
                    game.TimePerQuestion = int.Parse((sender as TextBox).Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end txtTimePerQuestion_TextChanged()

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            game?.UnregisterFromCallbacks();
        }

        // Helper methods
        private void disableAnswerButtons(bool disable)
        {
            buttonAnswerA.IsEnabled = !disable;
            buttonAnswerB.IsEnabled = !disable;
            buttonAnswerC.IsEnabled = !disable;
            buttonAnswerD.IsEnabled = !disable;
        }

        private void disableRuleControls(bool disable)
        {
            btnStart.IsEnabled = !disable;
            sliderQuestions.IsEnabled = !disable;
            txtQuestionCount.IsEnabled = !disable;
            txtTimePerQuestion.IsEnabled = !disable;
            comboCategories.IsEnabled = !disable;
        }

        /// ICallback implementaion

        private delegate void ClientUpdateInGameDelegate(CallbackInGameInfo info);

        public void UpdateInGame(CallbackInGameInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // disable rule controls
                disableRuleControls(true);

                labelGameStatus.Text = $"Game started with category: {game.Category.Replace("-", " ")}";
                labelCurrentQuestion.Text = info.Question.Sentence;
                txtAnswerA.Text = info.Question.Options[0];
                txtAnswerB.Text = info.Question.Options[1];
                txtAnswerC.Text = info.Question.Options[2];
                txtAnswerD.Text = info.Question.Options[3];

                if (!info.EndGame)
                {
                    labelResult.Text = "Choose your answer...";
                    disableAnswerButtons(false);
                    // get players and sort by points
                    var sortedPlayers = info.Players.OrderByDescending(p => p.TotalPoints).ToArray();
                    lstPlayers.ItemsSource = sortedPlayers;
                    labelGameStatus.Text = $"Leading player: {sortedPlayers[0]}";

                    time = TimeSpan.FromSeconds(game.TimePerQuestion);
                    if (timer == null)
                    {
                        timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                        {
                            txtTimer.Text = $"{time.ToString("ss")} s";
                            if (time == TimeSpan.Zero)
                            {
                                timer.Stop();
                                if (info.GameHost)
                                    game.GetNextQuestion();
                            }
                            time = time.Add(TimeSpan.FromSeconds(-1));
                        }, Application.Current.Dispatcher);
                    }
                    timer.Start();
                }
                else
                {
                    labelResult.Text = $"The winner is: {info.Players.OrderByDescending(p => p.TotalPoints).ToArray()[0]}";
                    labelResult.Text += "\nGame over.... Check player list for scores!";
                    // reset view to pre game content
                    labelCurrentQuestion.Text = "Question?";
                    txtAnswerA.Text = "A";
                    txtAnswerB.Text = "B";
                    txtAnswerC.Text = "C";
                    txtAnswerD.Text = "D";
                    txtPlayerName.IsEnabled = true;
                    btnReady.IsEnabled = true;
                    disableAnswerButtons(true);
                    if(info.GameHost)
                        game.EndGame();
                }
            }
            else
            {
                // Not the dispatcher thread that's calling this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateInGameDelegate(UpdateInGame), info);
            }
        }

        private delegate void ClientUpdateGameRulesDelegate(CallbackGameRulesInfo info);

        public void UpdateGameRules(CallbackGameRulesInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                sliderQuestions.Value = info.NumQuestions;
                txtQuestionCount.Text = $"{info.NumQuestions} Questions";
                txtTimePerQuestion.Text = info.TimePerQuestion.ToString();
                txtTimer.Text = $"{TimeSpan.FromSeconds(info.TimePerQuestion).ToString("ss")} s";
                comboCategories.Text = info.Category?.Replace("-", " ");
                lstPlayers.ItemsSource = info.Players.OrderByDescending(p => p.TotalPoints);
                disableAnswerButtons(true);
                if (info.GameHost)
                {
                    labelGameStatus.Text = "You're the game host! Start game when everyone is ready";
                    btnStart.Visibility = Visibility.Visible;
                    btnReady.Visibility = Visibility.Hidden;
                    disableRuleControls(false);
                }
            }
            else
            {
                // Not the dispatcher thread that's calling this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateGameRulesDelegate(UpdateGameRules), info);
            }
        }
    } // end MainWindow class
}
