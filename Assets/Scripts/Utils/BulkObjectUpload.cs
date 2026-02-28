using System;
using System.Collections.Generic;

namespace Utils
{
    [Serializable]
    public class BulkObjectUpload
    {
        public string EnvironmentId;
        public List<Object2D> Objects;
    }
}