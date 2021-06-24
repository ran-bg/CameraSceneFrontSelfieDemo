using System;
using System.Collections;
using System.Collections.Generic;
using Language;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 端末の加速度センサーにより、座標の取得を行う
/// </summary>
public class CurrentGyroImageController : MonoBehaviour
{
    
    [SerializeField] private Image image = default;
    [SerializeField] private RectTransform containerRectTransform = default;
    [SerializeField] private Button button = default;

    /// <summary>
    /// 水平状態の確認
    /// true: 水平、垂直である
    /// </summary>
    private bool _isGyroCheck = false;

    private Vector3 _basePotision;
    
    private float _drawH = 0;
    private float _drawW = 0;


    //現在オン動画
    private double _drawX = 0;
    private double _drawY = 0;


    // Start is called before the first frame update
    void Start()
    {
        _drawW = containerRectTransform.rect.width;
        _drawH = containerRectTransform.rect.height;

        //basePotision = this.gameObject.transform.position;
        _basePotision = this.gameObject.transform.localPosition;

        // サポートするかの確認 (source整理中に入れ忘れていたことを発覚とりあえずコメントとしてここに入れておく
        /*
        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogError("No SystemInfo.supportsGyroscope!!!");
            Destroy(this);
            return;
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        // 加速度センサの値を取得
        Vector3 val = Input.acceleration;

        // デバイスの角度
        Quaternion gyro = Input.gyro.attitude;
        var gyroX = val.x * Mathf.Rad2Deg;
        var gyroZ = val.z * Mathf.Rad2Deg;

        var x = _basePotision.x + gyroX * 11;
        var y = _basePotision.y + gyroZ * 11;
        
        
        var x2 = (float) Normalize((double)x, -180, 180, -_drawW / 2f, _drawW / 2f);
        var y2 = (float) Normalize((double)y, -180, 180, -_drawH / 2f, _drawH / 2f);

        x2 = Mathf.Clamp(x2,-200,200);
        y2 = Mathf.Clamp(y2,-200,200);
        
        //Gyroの位置を直接
        // this.gameObject.transform.localPosition = new Vector3(x2, y2, 0);
        
        // 描画位置をホーミングさせる。
        var spd = getDistance(_drawX,_drawY,x2,y2) /5;
        if (spd <= 1) {
            spd = 0;
        }
        //double  radian = degree * Math.PI / 180;
        
        double radian = getRadian( _drawX, _drawY, x2, y2);
        var mx = spd * Math.Cos(radian);
        var my = spd * Math.Sin(radian);
        _drawX += mx;
        _drawY += my;
        _drawX = Mathf.Clamp((float)_drawX,-200,200);
        _drawY = Mathf.Clamp((float)_drawY,-200,200);
        //Debug.Log($"({_drawX:f1},{_drawY:f1})({x2:f1},{y2:f1})({mx:f1},{my:f1})(rot:{redianToDegree(radian)})");

        this.gameObject.transform.localPosition = new Vector3((float)_drawX, (float)_drawY, 0);

        _isGyroCheck = (-5 < gyroX && gyroX < 5 && -5 < gyroZ && gyroZ < 5);
        UpdateIsFittingMark();
    }

    private void UpdateIsFittingMark()
    {
        if (_isGyroCheck)
        {
            //isFittingImage.sprite = ok;
            image.color = ColorPalette.GetGyroOk();
            button.interactable = true;
        }
        else
        {
            //isFittingImage.sprite = ng;
            image.color = ColorPalette.GetGyroNg();
            button.interactable = false;
        }
    }
    
    double Normalize(double val, double valmin, double valmax, double min, double max)
    {
        return (((val - valmin) / (valmax - valmin)) * (max - min)) + min;
    }
    
    public bool getGyroCheck()
    {
        return _isGyroCheck;
    }
    
    /// <summary>
    /// ２転換の距離を求める
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    protected double getDistance(double x, double y, double x2, double y2) {
        var distance = Math.Sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y));

        return distance;
    }
    
    /// <summary>
    /// 2点間の角度を求める関数
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    protected double getRadian(double x, double y, double x2, double y2) {
        var radian = Math.Atan2(y2 - y,x2 - x);
        return radian;
    }
    
    private double redianToDegree(double radian){
        return radian * 180.0 / Math.PI;
    }

    private void Move( double baseX ,double baseY , double x2 , double y2)
    {

    }

    //double degree = arcLineMargineRadian * 180d / Math.PI;
}

