using System;

namespace DefaultNamespace
{
    [Serializable]
    public class ApiLoginReturn
    {
        public string tokenType;
        public string accessToken;
        public string expiresIn;
        public string refreshToken;
    }
}