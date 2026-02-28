using System;
using UnityEngine.Serialization;

namespace Utils
{
    [Serializable]
    public class Object2D
    {
        public Guid? Id;
        public int posX;
        public int posY;
        public int tileId;
        public string Layer;
        public Guid? EnvironmentId;
    }
}