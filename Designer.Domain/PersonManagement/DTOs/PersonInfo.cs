using System;
namespace Designer.Domain.PersonManagement.DTOs
{
    /// <summary>
    /// contains everything needed for querying person lists
    /// </summary>
    public class PersonInfo
    {
        public int Id { get; internal set; }
        public string Fullname { get; internal set; }
        public string Email { get; internal set; }

        public PersonInfo(int id, string fullname, string email)
        {
            Id = id;
            Fullname = fullname;
            Email = email;
        }
    }
}
