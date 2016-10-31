using System;
namespace Designer.Domain.PersonManagement.Messages
{
    public class RegisterPerson
    {
        public string Fullname { get; private set; }
        public string Email { get; private set; }

        public RegisterPerson(string fullname, string email)
        {
            if (String.IsNullOrWhiteSpace(fullname))
                throw new ArgumentNullException(nameof(fullname));
            if (String.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            
            Fullname = fullname;
            Email = email;
        }
    }
}
