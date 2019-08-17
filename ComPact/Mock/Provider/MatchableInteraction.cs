using ComPact.Models.V2;

namespace ComPact.Mock.Provider
{
    internal class MatchableInteraction
    {
        public Interaction Interaction { get; set; }
        public bool HasBeenMatched { get; set; }

        public MatchableInteraction(Interaction interaction)
        {
            Interaction = interaction;
        }
    }
}
