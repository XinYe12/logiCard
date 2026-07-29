namespace LogiCard.Sim
{
    public enum StanceType
    {
        Sprint = 0,
        Walk = 1,
        Crawl = 2,
    }

    public static class StanceMath
    {
        /// <summary>
        /// Time Resource multipliers carried over from the paper prototype (TABLETOP_RULES / D5):
        /// Sprint x1, Tactical Walk x2, Stealth Crawl x4.
        /// </summary>
        public static float MultiplierFor(StanceType stance)
        {
            switch (stance)
            {
                case StanceType.Sprint: return 1f;
                case StanceType.Walk: return 2f;
                case StanceType.Crawl: return 4f;
                default: return 1f;
            }
        }

        public static string Label(StanceType stance)
        {
            switch (stance)
            {
                case StanceType.Sprint: return "Sprint";
                case StanceType.Walk: return "Walk";
                case StanceType.Crawl: return "Crawl";
                default: return stance.ToString();
            }
        }
    }
}
