using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Gameboard
{
    public class AnimatorInfo : ScriptableObject
    {
        public int[] parameterHashes;

        public int GetParameterIndex(int hash)
        {
            return Array.IndexOf(parameterHashes, hash);
        }
    }
}
