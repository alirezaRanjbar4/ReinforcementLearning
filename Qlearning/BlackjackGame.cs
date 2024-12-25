using BlackJack;

namespace Qlearning
{
    public class BlackjackGame
    {
        public List<Card> PlayerHand = new List<Card>();
        public List<Card> DealerHand = new List<Card>();
        private Random random = new Random();

        public static int GetValue(List<Card> cards)
        {
            int value = cards.Sum(card => (int)card.Rank);
            int aceCount = cards.Count(card => card.Rank == Rank.Ace);

            //پیاده سازی دو وجهی بودن کارت آس
            //آس هم میتواند 1 حساب شود هم 11
            while (value > 21 && aceCount > 0)
            {
                value -= 10;
                aceCount--;
            }

            return value;
        }

        public void DealInitialCards()
        {
            //در ابتدا بازی دو کارت به بازیکن و یک کارت به دیلر داده میشود
            PlayerHand.Add(DealCard());
            PlayerHand.Add(DealCard());
            DealerHand.Add(DealCard());
        }

        public void DealerPlay()
        {
            // دیلر تا زمانی که ارزش دست کمتر از ۱۷ باشد، کارت می‌گیرد
            while (GetValue(DealerHand) < 17)
            {
                DealerHand.Add(DealCard());
            }
        }

        public Card DealCard()
        {
            //دادن یک کارت رندوم
            Suit suit = (Suit)random.Next(4);
            Rank rank = (Rank)random.Next(2, 12);
            return new Card(suit, rank);
        }

        public string GetGameState()
        {
            //ایجاد استیت بازی بر اساس دست دیلر و بازیکن
            int playerValue = GetValue(PlayerHand);
            int dealerValue = GetValue(DealerHand);

            return $"Player:{playerValue.ToString("D2")}-Dealer:{dealerValue.ToString("D2")}";
        }

        public double GetReward()
        {
            int playerValue = GetValue(PlayerHand);
            int dealerValue = GetValue(DealerHand);

            // بررسی شرایط مختلف برای تعیین پاداش
            if (playerValue > 21) return -2.5;
            if (playerValue == 21) return 2.5;

            // اگر بازیکن برنده شود
            if (playerValue > dealerValue)
            {
                if (playerValue == 20) return 2;
                if (playerValue == 19) return 1.8;
                if (playerValue == 18) return 1.6;
                if (playerValue == 17) return 1.4;
                return 1;
            }
            // اگر بازیکن ببازد
            else if (playerValue < dealerValue)
            {
                if (playerValue == 20) return -0.2;
                if (playerValue == 19) return -0.4;
                if (playerValue == 18) return -0.6;
                if (playerValue == 17) return -0.8;
                return -1;
            }

            return 0.1;
        }

        public int GetWinner()
        {
            int playerValue = GetValue(PlayerHand);
            int dealerValue = GetValue(DealerHand);

            if (playerValue > 21) return -1;
            if (dealerValue > 21) return 1;
            if (playerValue > dealerValue) return 1;
            if (playerValue < dealerValue) return -1;
            return 0;
        }

        public void DisplayGameStatus(QLearningAction action)
        {
            //بازیکن
            int playerValue = GetValue(PlayerHand);
            Console.WriteLine($"Player's Hand Value: {playerValue}");
            for (int i = 0; i < PlayerHand.Count(); i++)
            {
                Console.WriteLine($"Player's Card {i + 1}: {PlayerHand[i].Suit}-{PlayerHand[i].Rank}");
            }
            Console.WriteLine();


            //دیلر
            int dealerValue = GetValue(DealerHand);
            Console.WriteLine($"Dealer's Hand Value: {dealerValue}");
            for (int i = 0; i < DealerHand.Count(); i++)
            {
                Console.WriteLine($"Dealer's Card {i + 1}: {DealerHand[i].Suit}-{DealerHand[i].Rank}");
            }
            Console.WriteLine();

            Console.WriteLine($"Last Action Chosen:{action}");

            //نتیجه
            if (action.Name == "Surrender")
            {
                Console.WriteLine("Player Surrendered.");
            }
            if (playerValue > 21)
            {
                Console.WriteLine("Player busts! Dealer wins.");
            }
            else if (dealerValue > 21)
            {
                Console.WriteLine("Dealer busts! Player wins.");
            }
            else if (playerValue > dealerValue)
            {
                Console.WriteLine("Player wins.");
            }
            else if (playerValue < dealerValue)
            {
                Console.WriteLine("Dealer wins.");
            }
            else
            {
                Console.WriteLine("It's a tie!");
            }

            Console.WriteLine("/////////////////////////////////////////////////////////////////////////////");
        }

    }
}
