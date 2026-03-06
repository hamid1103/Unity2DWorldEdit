using System;
using System.Collections.Generic;

namespace Utils
{
    [Serializable]
    public class RegisterErrorResponse
    {
        public string type;
        public string title;
        public int status;
        public RegisterErrors errors;
    }
}