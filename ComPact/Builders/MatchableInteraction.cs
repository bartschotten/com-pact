using ComPact.Models;

namespace ComPact.Builders
{
    public class MatchableInteraction
    {
        public InteractionV2 Interaction { get; set; }
        public bool HasBeenMatched { get; set; }

        public MatchableInteraction(InteractionV2 interaction)
        {
            Interaction = interaction;
        }
    }
}
