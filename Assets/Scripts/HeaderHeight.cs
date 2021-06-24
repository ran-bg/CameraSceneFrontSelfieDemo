using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ヘッダー処理を行う場所
/// 現状は割り当てやすくするためだけのもの
/// </summary>
public class HeaderHeight : MonoBehaviour
{
    [SerializeField] private bool isInSafeArea = false;
    
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_IOS


#if !ENV_PRODUCTION
             Debug.Log("HeaderHeight"); 
#endif
        //ヘッダーの位置を調整する
        Vector3 pos = transform.localPosition;
        
        //pos.y -= StatusBarController.getStatusBarHeight();
        //SafeAreaの中にいる場合ここで調整
        if (isInSafeArea){
#if !ENV_PRODUCTION
                 Debug.Log("HeaderHeight-1"); 
#endif
            transform.localPosition = pos;
            return;
        }
        
#if !ENV_PRODUCTION
             Debug.Log("HeaderHeight-2"); 
#endif
        
        // SafeArea分下にずらす
        pos.y -= Screen.safeArea.y;
        
        transform.localPosition = pos;
#endif
    }
}
