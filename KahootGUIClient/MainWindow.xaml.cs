/*
 * Program:         KahootGuiClient.exe
 * Module:          MainWindow.xaml.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     A Windows WPF client that uses KahootLibrary.dll via a WCF service.
 */

using System;
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
            sliderQuestions.IsEnabled = false;
            txtQuestionCount.IsEnabled = false;
            txtTimePerQuestion.IsEnabled = false;
            txtTimer.IsEnabled = false;
            comboCategories.IsEnabled = false;
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
                    game.StartGame();
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
                if (game.CheckAnswer((e.Source as Button).Content.ToString(), clientIdx, (int)time.TotalSeconds))
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
                if (game != null)
                {
                    game.TimePerQuestion = int.Parse((sender as TextBox).Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end txtTimePerQuestion_TextChanged()

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game?.UnregisterFromCallbacks();
        }

        // Helper method
        private void disableAnswerButtons(bool disable)
        {
            buttonAnswerA.IsEnabled = !disable;
            buttonAnswerB.IsEnabled = !disable;
            buttonAnswerC.IsEnabled = !disable;
            buttonAnswerD.IsEnabled = !disable;
        }

        /// ICallback implementaion

        private delegate void ClientUpdateInGameDelegate(CallbackInGameInfo info);

        public void UpdateInGame(CallbackInGameInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // disable rule controls
                btnStart.IsEnabled = false;
                txtPlayerName.IsEnabled = false;
                sliderQuestions.IsEnabled = false;
                txtQuestionCount.IsEnabled = false;
                txtTimePerQuestion.IsEnabled = false;
                txtTimer.IsEnabled = false;
                comboCategories.IsEnabled = false;

                labelGameStatus.Text = $"Game started with category: {game.Category.Replace("-", " ")}";
                labelCurrentQuestion.Text = info.Question.Sentence;
                buttonAnswerA.Content = info.Question.Options[0];
                buttonAnswerB.Content = info.Question.Options[1];
                buttonAnswerC.Content = info.Question.Options[2];
                buttonAnswerD.Content = info.Question.Options[3];

                if (!info.EndGame)
                {
                    labelResult.Text = "Choose your answer...";
                    disableAnswerButtons(false);
                    // get players and sort by points
                    lstPlayers.ItemsSource = info.Players;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lstPlayers.ItemsSource);
                    view.SortDescriptions.Add(new SortDescription("TotalPoints", ListSortDirection.Descending));
                    view.Refresh();

                    time = TimeSpan.FromSeconds(game.TimePerQuestion);
                    if (timer == null)
                    {
                        timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                        {
                            txtTimer.Text = $"{time.ToString("ss")} s";
                            if (time == TimeSpan.Zero)
                            {
                                timer.Stop();
                                game.GetNextQuestion();
                            }
                            time = time.Add(TimeSpan.FromSeconds(-1));
                        }, Application.Current.Dispatcher);
                    }
                    timer.Start();
                }
                else
                {
                    labelGameStatus.Text = "Game over.... Check player list for scores!";
                    labelResult.Text = "Game over.... Check player list for scores!";
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
                txtTimer.Text = $"{info.TimePerQuestion} s";
                comboCategories.Text = info.Category?.Replace("-", " ");
                lstPlayers.ItemsSource = info.Players;
                disableAnswerButtons(true);
                if (info.GameHost)
                {
                    labelGameStatus.Text = "You're the game host! Start game when eveyrone joins";
                    btnStart.Visibility = Visibility.Visible;
                    btnReady.Visibility = Visibility.Hidden;
                    btnStart.IsEnabled = true;
                    sliderQuestions.IsEnabled = true;
                    txtQuestionCount.IsEnabled = true;
                    txtTimePerQuestion.IsEnabled = true;
                    txtTimer.IsEnabled = true;
                    comboCategories.IsEnabled = true;
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
