using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    /// <summary>
    /// Query Message for Person Aggregate to return a PersonInfo message
    /// </summary>
    public class GetPersonInfo : DispatchableCommand<int>
    {
        public GetPersonInfo(int id) : base(id) {}
    }
}
