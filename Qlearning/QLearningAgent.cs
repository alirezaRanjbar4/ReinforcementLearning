namespace BlackJack
{
    public class QLearningAction
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
    }

    public class QLearningAgent
    {
        public Dictionary<string, List<QLearningAction>> qTable = new Dictionary<string, List<QLearningAction>>();
        private Random random = new Random();
        public double learningRate = 0.1;
        public double discountFactor = 0.9;
        public double epsilon = 0.1;

        public void UpdateQValue(string state, string actionName, double reward, string newState, List<QLearningAction> availableActions)
        {
            //آپدیت کردن جدول q-value
            if (!qTable.ContainsKey(state))
            {
                qTable.Add(state, new List<QLearningAction>());
                foreach (var item in availableActions)
                {
                    qTable[state].Add(new QLearningAction()
                    {
                        Index = item.Index,
                        Name = item.Name,
                        Value = 0
                    });
                }
                qTable = qTable.OrderBy(x => x.Key).ToDictionary();
            }

            double maxQNewState = qTable.ContainsKey(newState) ? qTable[newState].Max(x => x.Value) : 0;
            QLearningAction qAction = qTable[state].First(x => x.Name == actionName);
            double qValue = qAction.Value;

            double delta = reward + discountFactor * maxQNewState - qValue;
            qTable[state].First(x => x.Name == actionName).Value += learningRate * delta;

            // کاهش تدریجی مقدار learningRate
            learningRate = Math.Max(0.01, learningRate * 0.999998);
        }

        public QLearningAction GetBestAction(string state, List<QLearningAction> availableActions)
        {
            // کاهش تدریجی مقدار epsilon
            epsilon = Math.Max(0.001, epsilon * 0.999997);

            if (!qTable.ContainsKey(state) || random.NextDouble() < epsilon)
            {
                // انتخاب تصادفی برای کاوش بیشتر در ابتدای آموزش
                var index = random.Next(availableActions.Count());
                return availableActions[index];
            }

            var validActions = qTable[state].Where(x => availableActions.Select(x => x.Name).Contains(x.Name)).OrderBy(x => x.Value);
            return validActions.Last();
        }

    }
}