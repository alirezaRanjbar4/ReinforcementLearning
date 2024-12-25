using BlackJack;

namespace Qlearning
{
    public class BlackjackGamePlay
    {
        public static void StartGame()
        {
            #region parameters
            BlackjackGame game = new BlackjackGame();
            QLearningAgent agent = new QLearningAgent();

            int totalEpisodes = 1000000;
            int countResultEpisodes = 900000;
            int minLearningRateEpisode = 0;
            bool LR = true;
            int minEpsilonEpisode = 0;
            bool E = true;

            int hitCount = 0;
            int standCount = 0;
            int doubleDownCount = 0;
            int surrenderCount = 0;
            int splitCount = 0;

            int playerWinsCount = 0;
            int playerBustCount = 0;
            int playerDubleDownCount = 0;
            int playerSurrenderCount = 0;

            int dealerBustCount = 0;
            int dealerWinsCount = 0;
            int dealerDubleDownCount = 0;

            int drawCount = 0;
            int drawDubleDownCount = 0;

            string currentState;
            string previousState;

            bool gameEnded;
            double reward;
            var availableActions = new List<QLearningAction>()
        {
            new QLearningAction(){Index=0,Name="Hit",Value=0},
            new QLearningAction(){Index=1,Name="Stand",Value=0},
            new QLearningAction(){Index=2,Name="DoubleDown",Value=0},
            new QLearningAction(){Index=3,Name="Surrender",Value=0},
            new QLearningAction(){Index=4,Name="Split",Value=0}
        };
            QLearningAction action = availableActions.First();

            bool canSplit = false;
            bool hasSplit = false;
            bool SplitInProgress = false;
            string? splitState = null;
            double? firstSplitReward = null;
            List<Card> splitHand = new List<Card>();
            List<Card> dealerSplitHand = new List<Card>();
            List<Card> dealerSplitHand2 = new List<Card>();
            #endregion

            for (int episode = 0; episode < totalEpisodes; episode++)
            {
                if (LR && agent.learningRate == 0.01)
                {
                    minLearningRateEpisode = episode;
                    LR = false;
                }

                if (E && agent.epsilon == 0.001)
                {
                    minEpsilonEpisode = episode;
                    E = false;
                }


                // شروع بازی جدید
                game.DealInitialCards();
                gameEnded = false;
                reward = 0;
                canSplit = false;

                while (!gameEnded)
                {
                    canSplit = game.PlayerHand.Count() == 2 && game.PlayerHand[1].Rank == game.PlayerHand[0].Rank && !hasSplit && firstSplitReward == null;
                    var allowedActions = canSplit ? availableActions : availableActions.Where(x => x.Name != "Split").ToList();
                    currentState = game.GetGameState();

                    // انتخاب عمل بر اساس سیاست عامل یادگیری
                    action = agent.GetBestAction(currentState, allowedActions);
                    previousState = currentState;

                    switch (action.Name)
                    {
                        case "Hit":
                            hitCount++;
                            game.PlayerHand.Add(game.DealCard());
                            if (BlackjackGame.GetValue(game.PlayerHand) > 21)
                            {
                                gameEnded = true;
                                if (SplitInProgress)
                                    game.DealerHand = dealerSplitHand2;
                                else
                                    game.DealerPlay();
                                reward = game.GetWinner();
                                currentState = game.GetGameState();
                                agent.UpdateQValue(previousState, "Hit", reward, currentState, availableActions);
                            }
                            else
                            {
                                //نتیجه بازی بدست نیامده ولی چون با کارت گرفتن بازیکن نسوخته پس یک مقدار کوچکی به عنوان ریوارد میدهیم
                                reward = 0.1;
                                currentState = game.GetGameState();
                                agent.UpdateQValue(previousState, "Hit", reward, currentState, availableActions);
                            }
                            break;


                        case "Stand":
                            standCount++;
                            if (SplitInProgress)
                                game.DealerHand = dealerSplitHand2;
                            else
                                game.DealerPlay();
                            gameEnded = true;
                            reward = game.GetWinner();
                            currentState = game.GetGameState();
                            agent.UpdateQValue(previousState, "Stand", reward, currentState, availableActions);
                            break;


                        case "DoubleDown":
                            doubleDownCount++;
                            game.PlayerHand.Add(game.DealCard());
                            if (SplitInProgress)
                                game.DealerHand = dealerSplitHand2;
                            else
                                game.DealerPlay();
                            gameEnded = true;
                            reward = game.GetWinner() * 2;
                            currentState = game.GetGameState();
                            agent.UpdateQValue(previousState, "DoubleDown", reward, currentState, availableActions);
                            break;


                        case "Surrender":
                            surrenderCount++;
                            if (SplitInProgress)
                                game.DealerHand = dealerSplitHand2;
                            else
                                game.DealerPlay();
                            gameEnded = true;
                            reward = -0.6;
                            currentState = game.GetGameState();
                            agent.UpdateQValue(previousState, "Surrender", reward, currentState, availableActions);
                            break;


                        case "Split":
                            splitCount++;
                            splitHand = new List<Card>();
                            splitState = game.GetGameState();
                            splitHand.Add(game.PlayerHand[1]);
                            var newSplitCard = game.DealCard();
                            splitHand.Add(newSplitCard);

                            dealerSplitHand.Add(game.DealerHand[0]);

                            game.PlayerHand.RemoveAt(1);
                            var newPlayerCard = game.DealCard();
                            game.PlayerHand.Add(newPlayerCard);
                            hasSplit = true;
                            canSplit = false;
                            break;
                    }


                    //اگر بازی تمام شده باشه ولی اسپلیت داشته باشیم
                    if (gameEnded && hasSplit && firstSplitReward == null)
                    {
                        game.PlayerHand = splitHand;
                        firstSplitReward = reward;
                        dealerSplitHand2 = game.DealerHand;
                        game.DealerHand = dealerSplitHand;
                        hasSplit = false;
                        canSplit = false;
                        gameEnded = false;
                        SplitInProgress = true;
                    }
                }

                if (firstSplitReward != null)
                {
                    var splitReward = firstSplitReward + reward;
                    agent.UpdateQValue(splitState, "Split", splitReward.Value, splitState, availableActions);
                    firstSplitReward = 0;
                }

                if (episode > countResultEpisodes)
                {
                    int playerValue = BlackjackGame.GetValue(game.PlayerHand);
                    int dealerValue = BlackjackGame.GetValue(game.DealerHand);

                    if (action.Name == "Surrender")
                    {
                        dealerWinsCount++;
                        playerSurrenderCount++;
                    }
                    else if (playerValue > 21)
                    {
                        playerBustCount++;
                        dealerWinsCount++;
                        if (action.Name == "DoubleDown")
                            dealerDubleDownCount++;
                    }
                    else if (dealerValue > 21)
                    {
                        dealerBustCount++;
                        playerWinsCount++;
                        if (action.Name == "DoubleDown")
                            playerDubleDownCount++;
                    }
                    else if (playerValue > dealerValue)
                    {
                        playerWinsCount++;
                        if (action.Name == "DoubleDown")
                            playerDubleDownCount++;
                    }
                    else if (playerValue < dealerValue)
                    {
                        dealerWinsCount++;
                        if (action.Name == "DoubleDown")
                            dealerDubleDownCount++;
                    }
                    else
                    {
                        drawCount++;
                        if (action.Name == "DoubleDown")
                            drawDubleDownCount++;
                    }

                    // نمایش وضعیت بازی در هر دور
                    //Console.WriteLine($"Episode {episode + 1}");
                    //game.DisplayGameStatus(action);
                }

                game.PlayerHand.Clear();
                game.DealerHand.Clear();
                splitHand.Clear();
                dealerSplitHand.Clear();
                dealerSplitHand2.Clear();

                canSplit = false;
                hasSplit = false;
                SplitInProgress = false;
                splitState = null;
                firstSplitReward = null;
            }


            foreach (var item in agent.qTable)
            {
                var ordered = item.Value.OrderByDescending(x => x.Value).ToList();
                Console.WriteLine($"{item.Key}=> '{ordered.First().Name}:{ordered.First().Value.ToString("0.000")}'   '{ordered.Skip(1).First().Name}':{ordered.Skip(1).First().Value.ToString("0.000")}   '{ordered.Skip(2).First().Name}':{ordered.Skip(2).First().Value.ToString("0.000")}   '{ordered.Skip(3).First().Name}':{ordered.Skip(3).First().Value.ToString("0.000")}   '{ordered.Skip(4).First().Name}':{ordered.Skip(4).First().Value.ToString("0.000")}");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Total episode {totalEpisodes.ToString("N0")}");
            Console.WriteLine($"Count result start at episode {countResultEpisodes.ToString("N0")}");
            Console.WriteLine($"Learning rate minimum at episode {minLearningRateEpisode.ToString("N0")}");
            Console.WriteLine($"Epsilon minimum at episode {minEpsilonEpisode.ToString("N0")}");
            Console.WriteLine($"Total action counts: Hit={hitCount.ToString("N0")}  Stand={standCount.ToString("N0")}  DoubleDown={doubleDownCount.ToString("N0")}  Surrender={surrenderCount.ToString("N0")}  Split={splitCount.ToString("N0")}");
            Console.WriteLine();
            Console.WriteLine($"Player wins {playerWinsCount.ToString("N0")} times");
            Console.WriteLine($"Player bust {playerBustCount.ToString("N0")} times");
            Console.WriteLine($"Player duble down {playerDubleDownCount.ToString("N0")} times");
            Console.WriteLine($"Player surrender {playerSurrenderCount.ToString("N0")} times");
            Console.WriteLine();
            Console.WriteLine($"Dealer wins {dealerWinsCount.ToString("N0")} times");
            Console.WriteLine($"Dealer bust {dealerBustCount.ToString("N0")} times");
            Console.WriteLine($"Dealer duble down {dealerDubleDownCount.ToString("N0")} times");
            Console.WriteLine();
            Console.WriteLine($"Draw {drawCount.ToString("N0")} times");
            Console.WriteLine($"Draw duble down {drawDubleDownCount.ToString("N0")} times");
            Console.WriteLine();
            Console.WriteLine();

        }
    }
}
