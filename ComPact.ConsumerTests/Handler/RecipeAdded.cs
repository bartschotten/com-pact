using ComPact.Tests.Shared;
using System;

namespace ComPact.ConsumerTests.Handler
{
    public class RecipeAdded
    {
        public Guid EventId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
