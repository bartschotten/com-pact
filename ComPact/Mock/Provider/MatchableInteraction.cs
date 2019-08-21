using ComPact.Models.V3;

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

        public Response Match(Models.V3.Request request)
        {
            var response = Interaction.Match(request);
            if (response != null)
            {
                HasBeenMatched = true;
            }
            return response;
        }
    }
}
