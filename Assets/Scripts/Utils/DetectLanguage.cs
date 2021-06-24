using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Language
{
    public class DetectLanguage : MonoBehaviour
    {
        static public bool IsJapanese()
        {
            return Application.systemLanguage == SystemLanguage.Japanese;
        }

        static public bool IsEnglish()
        {
            return Application.systemLanguage == SystemLanguage.English;
        }
    }

}