using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;

namespace Fcg.Orders.Domain.ValueObject
{
    public class Price : ValueObject<Price>
    {
        public decimal Amount { get; }

        public Price(decimal amount)
        {
            AssertionConcern.AssertArgumentValueFormat(amount, DomainMessages.InvalidValue);
            this.Amount = amount;
        }

        protected override bool EqualsCore(Price other)
        {
            return Amount == other.Amount;
        }

        protected override int GetHashCodeCore()
        {
            return Amount.GetHashCode();
        }
    }
}
