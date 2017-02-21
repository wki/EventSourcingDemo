using Wki.EventSourcing.Protocol;

namespace Designer.Domain.PersonManagement.Messages
{
    public class AddressUpdated : Event<int>
    {
        public AddressUpdated(int id) : base(id)
        {
        }
    }
}
