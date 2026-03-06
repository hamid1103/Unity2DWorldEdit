using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Utils
{
    [Serializable]
    public class RegisterErrors
    {
        [CanBeNull] public List<string> PasswordTooShort;
        [CanBeNull] public List<string> PasswordRequiresUpper;
        [CanBeNull] public List<string> DuplicateEmail;
        [CanBeNull] public List<string> DuplicateUserName;
    }
}