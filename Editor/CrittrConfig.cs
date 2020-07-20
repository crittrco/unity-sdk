using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crittr.Editor
{
    // fileName must match the name of the class to be found in the Resources.Load method.
    [CreateAssetMenu(fileName = "CrittrConfig", menuName = "Crittr Config", order=51)]
    [Serializable]
    public class CrittrConfig : ScriptableObject
    {
        [SerializeField]
        [Header("Connection URI with API Key")]
        public string ConnectionURI;

        [SerializeField]
        [Header("Toggle debug messages")]
        public bool Verbose = true;

        [SerializeField]
        [Header("Default input command to trigger bug report")]
        public string defaultKey = "f8";

        private static CrittrConfig _instance;
        public static CrittrConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    CrittrConfig settings = Resources.Load<CrittrConfig>(nameof(CrittrConfig));
                    if (settings == null) {
                        settings = CreateInstance<CrittrConfig>();
                    }

                    _instance = Instantiate(settings);
                }

                return _instance;
            }
        }

    }

}
