using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class BackTutorialController : MonoBehaviour
{
    public Image timerImage;
    public delegate void onComplete();

    private bool isCancel = false;

    public void StartAnimation(onComplete callback = null, int time = 4)
    {
        if (isCancel)
        {
            return;
        }

        this.gameObject.SetActive(true);
        isCancel = false;

        GameObject go = gameObject;

        CanvasGroup img = go.GetComponent<CanvasGroup>();
        img.DOFade(1, 0)
        .OnComplete(() =>
        {
            if (isCancel)
            {
                return;
            }

            img.DOFade(1, time)
            .OnComplete(() =>
            {
                if (isCancel)
                {
                    return;
                }

                img.DOFade(0, 1).OnComplete(() =>
                {
                    if (isCancel)
                    {
                        return;
                    }

                    callback?.Invoke();
                    go.SetActive(false);
                });
            });
        });
    }


    public void StartAnimationTimer(onComplete callback = null)
    {
        this.gameObject.SetActive(true);
        isCancel = false;

        GameObject go = gameObject;

        timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_3");
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        cg.DOFade(1, 0)
        .OnComplete(() =>
        {
            if (isCancel)
            {
                return;
            }
            cg.DOFade(1, 1)
            .OnComplete(() =>
            {
                if (isCancel)
                {
                    return;
                }
                timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_2");
                cg.DOFade(1, 1)
                .OnComplete(() =>
                {
                    if (isCancel)
                    {
                        return;
                    }
                    timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_1");
                    cg.DOFade(1, 1)
                    .OnComplete(() =>
                    {
                        cg.DOFade(0, 1).OnComplete(() =>
                        {
                            callback?.Invoke();
                            go.SetActive(false);
                        });
                    });
                });
            });
        });
    }

    public void StartAnimationTimer10(onComplete callback = null)
    {
        this.gameObject.SetActive(true);
        isCancel = false;

        GameObject go = gameObject;

        EasyTTSUtil.StopSpeech();
        EasyTTSUtil.SpeechAdd("10");
        timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_10");
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        cg.DOFade(1, 1)
        .OnComplete(() =>
        {
            if(isCancel)
            {
                return;
            }

            EasyTTSUtil.SpeechAdd("9");
            timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_9");
            cg.DOFade(1, 1)
            .OnComplete(() =>
            {
                if (isCancel)
                {
                    return;
                }

                EasyTTSUtil.SpeechAdd("8");
                timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_8");
                cg.DOFade(1, 1)
                .OnComplete(() =>
                {
                    if (isCancel)
                    {
                        return;
                    }
                    EasyTTSUtil.SpeechAdd("7");
                    timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_7");
                    cg.DOFade(1, 1)
                    .OnComplete(() =>
                    {
                        if (isCancel)
                        {
                            return;
                        }

                        EasyTTSUtil.SpeechAdd("6");
                        timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_6");
                        cg.DOFade(1, 1)
                        .OnComplete(() =>
                        {
                            if (isCancel)
                            {
                                return;
                            }

                            EasyTTSUtil.SpeechAdd("5");
                            timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_5");
                            cg.DOFade(1, 1)
                            .OnComplete(() =>
                            {
                                if (isCancel)
                                {
                                    return;
                                }

                                EasyTTSUtil.SpeechAdd("4");
                                timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_4");
                                cg.DOFade(1, 1)
                                .OnComplete(() =>
                                {
                                    if (isCancel)
                                    {
                                        return;
                                    }

                                    EasyTTSUtil.SpeechAdd("3");
                                    timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_3");
                                    cg.DOFade(1, 1)
                                    .OnComplete(() =>
                                    {
                                        if (isCancel)
                                        {
                                            return;
                                        }

                                        EasyTTSUtil.SpeechAdd("2");
                                        timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_2");
                                        cg.DOFade(1, 1)
                                        .OnComplete(() =>
                                        {
                                            if (isCancel)
                                            {
                                                return;
                                            }

                                            EasyTTSUtil.SpeechAdd("1");
                                            timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_1");
                                            cg.DOFade(1, 1)
                                            .OnComplete(() =>
                                            {
                                                if (isCancel)
                                                {
                                                    return;
                                                }

                                                cg.DOFade(0, 1).OnComplete(() =>
                                                {
                                                    callback?.Invoke();
                                                    go.SetActive(false);
                                                });
                                            });

                                        });

                                    });

                                });
                            });

                        });

                    });
                });
            });
        });
    }

    public void StartAnimationTimer5(onComplete callback = null)
    {
        this.gameObject.SetActive(true);

        GameObject go = gameObject;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();

        // EasyTTSUtil.StopSpeech();
        EasyTTSUtil.SpeechAdd("5");
        timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_5");
        cg.DOFade(1, 1)
        .OnComplete(() =>
        {
            if (isCancel)
            {
                return;
            }
            EasyTTSUtil.SpeechAdd("4");
            timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_4");
            cg.DOFade(1, 1)
            .OnComplete(() =>
            {
                if (isCancel)
                {
                    return;
                }
                EasyTTSUtil.SpeechAdd("3");
                timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_3");
                cg.DOFade(1, 1)
                .OnComplete(() =>
                {
                    if (isCancel)
                    {
                        return;
                    }
                    EasyTTSUtil.SpeechAdd("2");
                    timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_2");
                    cg.DOFade(1, 1)
                    .OnComplete(() =>
                    {
                        if (isCancel)
                        {
                            return;
                        }
                        EasyTTSUtil.SpeechAdd("1");
                        timerImage.sprite = Resources.Load<Sprite>("Camera/camera_num_1");
                        cg.DOFade(1, 1)
                        .OnComplete(() =>
                        {
                            cg.DOFade(0, 1).OnComplete(() =>
                            {
                                callback?.Invoke();
                                go.SetActive(false);
                            });
                        });

                    });

                });

            });
        });
    }


    public void IsCancel(bool flag)
    {
        isCancel = flag;
    }
}

/*
cg.DOFade(0, 1).OnComplete(() =>
                        {
    callback?.Invoke();
    go.SetActive(false);
});
*/