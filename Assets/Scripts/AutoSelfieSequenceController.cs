using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using Language;
using DigitsSystemVolume;
using Debug = UnityEngine.Debug;

namespace AutoSelfie
{

    public class AutoSelfieSequenceController : MonoBehaviour
    {
        // Start is called before the first frame update

        // sequence
        public enum Sequence
        {
            NONE,
            VOLUME_MUTECHECK,
            VOLUME_SETTING,
            BOOK_DESCRIPTION,
            GYRO_ALIGN,

            FRONT_CAPTURE_EXPLAIN,
            FRONT_CAPTURE,
            FRONT_COMPLETE,
            SIDE_CAPTURE_EXPALIN,
            SIDE_CAPTURE,
            SIDE_COMPLETE,

            ALL_COMPLETE
        }

        private Sequence _sequence;

        private Sequence _prev;

        private Action<bool> _onUpdateGyro;

        [Header("Scenes")]
        [SerializeField] private CameraSceneFrontSelfieController cameraSceneFrontSelfieController = null;

        [SerializeField] private AutoSelfieCameraMuteCheckController muteCheckController = default;
        [SerializeField] private AutoSelfieVolumeController volumeController = null;
        
        [SerializeField] private AutoSelfieCameraSettingDesctriptionController cameraDescriptionController = default;
        [SerializeField] private AutoSelfieGyroController gyroController = null;
        [SerializeField] private AutoSelfieFrontCaptureExplainController frontCaptureExplainController = null;
        [SerializeField] private AutoSelfieFrontCaptureController frontCaptureController = null;
        //[SerializeField] private AutoSelfieFrontCaptureCompleteController frontCaptureCompleteController = null;

        [Header("Other")]
        //[SerializeField] private ParticleSystem completeParticle = null;

        [SerializeField] private GameObject cameraImageConainer = default;
        // [SerializeField] private GameObject countDown = default;
        // [SerializeField] private GameObject loading = default;
        [SerializeField] private GameObject glLenderer = default;

        protected bool muted;
        private bool initialized = false;
        private AudioStreamType AudioStreamType;
        private float basePercent = 7.7f;

        private void Start()
        {
            // Speachの言語設定
            if (DetectLanguage.IsJapanese())
            {
                EasyTTSUtil.Initialize(EasyTTSUtil.Japan);
            }
            else
            {
                EasyTTSUtil.Initialize(EasyTTSUtil.UnitedStates);
            }

            DebugLog("MuteCheck");
            
            /// iPhoneではミュートチェックをせずに音量調整画面へ
            /// Androidではミュートチェック（音量が０の時だけ警告画面を表示）する
#if UNITY_IOS
                _sequence = Sequence.VOLUME_SETTING;
#elif UNITY_ANDROID
                
            if (NativeSystemVolumeManager.IsCurrentPlatformSupported())
            {
                DebugLog("MuteCheck-Success");
                // サポートしている
                AudioStreamType[] audioStreamTypes = NativeSystemVolumeManager.GetSupportedAudioStreamTypes();
                AudioOutputDevice[] audioOutputDevices = NativeSystemVolumeManager.GetAudioOutputDevices();
                InitializeUI(audioStreamTypes, audioOutputDevices);

                if (muted)
                {
                    _sequence = Sequence.VOLUME_MUTECHECK;
                }
                else
                {
                    _sequence = Sequence.VOLUME_SETTING;
                }
                
                DebugLog($"mute:{muted}");
            }
            else
            {
                DebugLog("MuteCheck-Nosupport");
                _sequence = Sequence.VOLUME_SETTING;
            }
#endif

            _prev = Sequence.NONE;
            
            muteCheckController.OnComplete  += HandleOnComplete;
            muteCheckController.OnBack += HandleBackButton;

            volumeController.OnComplete += HandleOnComplete;
            volumeController.OnBack += HandleBackButton;

            cameraDescriptionController.OnComplete += HandleOnComplete;
            cameraDescriptionController.OnBack += HandleBackButton;
                
            gyroController.OnUpdateState += HandleGyroStateUpdate;
            gyroController.OnBack += HandleBackButton;

            frontCaptureExplainController.OnComplete += HandleOnComplete;
            cameraSceneFrontSelfieController.OnFrontPoseCaptured += HandleOnComplete;
            //frontCaptureCompleteController.OnComplete += HandleOnComplete;
            
            cameraSceneFrontSelfieController.OnSidePoseCaptured += HandleOnComplete;

            //全てのActiveにする
            muteCheckController.gameObject.SetActive(true);
            volumeController.gameObject.SetActive(true);
            cameraDescriptionController.gameObject.SetActive(true);
            gyroController.gameObject.SetActive(true);
            frontCaptureExplainController.gameObject.SetActive(true);
            frontCaptureController.gameObject.SetActive(true);
            //frontCaptureCompleteController.gameObject.SetActive(true);

            
            cameraImageConainer.SetActive(true);
            // countDown.SetActive(true);
            // loading.SetActive(true);
            glLenderer.SetActive(true);
            
            //全てのViewを非表示にする
            muteCheckController.HideCanvas();
            volumeController.HideCanvas();
            cameraDescriptionController.HideCanvas();
            gyroController.HideCanvas();
            frontCaptureExplainController.HideCanvas();
            frontCaptureController.HideCanvas();
            //frontCaptureCompleteController.HideCanvas();

        }

        private void Update()
        {
            // Debug.Log(prev);
            if (_prev != _sequence)
            {
                OnChangeSequence();
                _prev = _sequence;
            }
        }
        
        public void InitializeUI(AudioStreamType[] audioStreamTypes, AudioOutputDevice[] audioOutputDevices)
        {
            int length = audioStreamTypes.Length;
            for (int i = 0; i < length; i++)
            {
                AudioStreamType audioStreamType = audioStreamTypes[i];
                DebugLog($"AudioStreamType:{audioStreamType}");
                
                
#if UNITY_IOS
                // iPhoneの場合音量が0 == Mute (スイッチで判断したいため未使用とする)
                // if (audioStreamType == AudioStreamType.SYSTEM)
                // {
                //     InitUI(audioStreamType);
                // }
                
                //　iOSの場合、ミュートスイッチで判断する == Mute
                muted = _muteSwitchDetector.isMuted;
                break;
#elif UNITY_ANDROID
                //　Androidの場合音量が0 == Mute
                if (audioStreamType == AudioStreamType.MUSIC)
                {
                    InitUI(audioStreamType);
                }
#endif
            }
        }
        
        /// <summary>
        /// AndroidのMute判定時に利用
        /// Volume = 0 として判定される。
        /// </summary>
        /// <param name="audioStreamType"></param>
        public void InitUI(AudioStreamType audioStreamType)
        {
            AudioStreamType = audioStreamType;
            NativeSystemVolumeManager.AddSystemVolumeMuteChangedListener(OnSystemVolumeMuteChanged);
            muted = NativeSystemVolumeManager.IsSystemVolumeMuted(audioStreamType);
        }
        
		public void OnSystemVolumeMuteChanged(bool muted, AudioStreamType audioStreamType)
		{
			if(AudioStreamType == audioStreamType)
			{
				this.muted = muted;
				//UpdateMuteSprite();

                // ミュート状態で切り替えたら次のシーンに進む
                if( !this.muted && _sequence == Sequence.VOLUME_MUTECHECK){
                    _sequence = Sequence.VOLUME_SETTING;
                }
			}
		}

        // ReSharper disable Unity.PerformanceAnalysis
        private void OnChangeSequence()
        {
            //前画面の処理
            switch (_prev)
            {
                case Sequence.VOLUME_MUTECHECK: EndMute(); break;
                case Sequence.VOLUME_SETTING:          EndVolume(); break;
                case Sequence.BOOK_DESCRIPTION:        EndBookDescriptionAlign(); break;
                case Sequence.GYRO_ALIGN:              EndGyroAlign(); break;
                  
                case Sequence.FRONT_CAPTURE_EXPLAIN: EndFrontExplain(); break;
                case Sequence.FRONT_CAPTURE:         EndFrontCapture(); break;
                case Sequence.FRONT_COMPLETE:        EndFrontCaptureComplete(); break;
                
                case Sequence.SIDE_CAPTURE_EXPALIN: EndSideExplain(); break;
                case Sequence.SIDE_CAPTURE:         EndSideCapture(); break;
                case Sequence.SIDE_COMPLETE:        EndSideCaptureComplete(); break;
            }
            
            //変更後の画面の初期化処理
            switch (_sequence)
            {
                case Sequence.VOLUME_MUTECHECK: StartMute(); break;
                case Sequence.VOLUME_SETTING:   StartVolume(); break;
                case Sequence.BOOK_DESCRIPTION: StartBookDescriptionAlign(); break;
                case Sequence.GYRO_ALIGN:       StartGyroAlign(); break;

                case Sequence.FRONT_CAPTURE_EXPLAIN: StartFrontExplain(); break;
                case Sequence.FRONT_CAPTURE:         StartFrontCapture(); break;
                case Sequence.FRONT_COMPLETE:
                    StartFrontCaptureComplete();
                   // completeParticle.Play();
                    break;

                case Sequence.SIDE_CAPTURE_EXPALIN: StartSideExplain(); break;
                case Sequence.SIDE_CAPTURE:         StartSideCapture(); break;
                case Sequence.SIDE_COMPLETE:
                    StartSideCaptureComplete();
                   // completeParticle.Play();
                    break;
                
                case Sequence.ALL_COMPLETE:
                    OnCompleteCapture();
                    break;
            }
        }

        private void OnCompleteCapture()
        {
           
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void HandleGyroStateUpdate(bool state, bool isHideOnly = false)
        {
            if (state)
            {
                OnGyroComplete();
                gyroController.HideCanvas();
            }
            else
            {
                if(!isHideOnly)
                    cameraSceneFrontSelfieController.UpdateRequiredPosenet(false);
                gyroController.ShowCanvas();
            }
        }

        /// <summary>
        /// 戻る系の処理
        /// </summary>
        private void HandleBackButton()
        {
            switch (_sequence)
            {
                case Sequence.VOLUME_MUTECHECK :
                    HandleBackButtonAtFirstPage();
                    break;
                case Sequence.VOLUME_SETTING :
#if UNITY_IOS
                HandleBackButtonAtFirstPage();
#elif UNITY_ANDROID
                    
                    if (muted)
                    {
                        _sequence = Sequence.VOLUME_MUTECHECK;
                    }
                    else
                    {
                        HandleBackButtonAtFirstPage();
                    }
#endif                    
                    break;
                
                case Sequence.BOOK_DESCRIPTION:
                    _sequence = Sequence.VOLUME_SETTING;
                    break;
                
                case Sequence.GYRO_ALIGN:
                    _sequence = Sequence.BOOK_DESCRIPTION;
                    break;
                
                case Sequence.FRONT_CAPTURE_EXPLAIN:
                    // BackButton is none
                    break;
                
                case Sequence.FRONT_CAPTURE:
                    // BackButton is none
                    break;
                
                case Sequence.FRONT_COMPLETE:
                    // BackButton is none
                    break;
                
                case Sequence.SIDE_CAPTURE_EXPALIN:
                    // BackButton is none
                    break;
                
                case Sequence.SIDE_CAPTURE:
                    // BackButton is none
                    break;
                
                case Sequence.SIDE_COMPLETE:
                    // BackButton is none
                    break;
                
            }
        }


        private void HandleBackButtonAtFirstPage()
        {

        }
        
        /// <summary>
        /// 進める系の処理
        /// </summary>
        private void HandleOnComplete()
        {
            switch (_sequence)
            {
                case Sequence.VOLUME_MUTECHECK : _sequence = Sequence.VOLUME_SETTING; break;
                case Sequence.VOLUME_SETTING :          _sequence = Sequence.BOOK_DESCRIPTION; break;
                case Sequence.BOOK_DESCRIPTION:         _sequence = Sequence.GYRO_ALIGN; break;
                case Sequence.GYRO_ALIGN:               _sequence = Sequence.FRONT_CAPTURE_EXPLAIN; break;
                case Sequence.FRONT_CAPTURE_EXPLAIN:    _sequence = Sequence.FRONT_CAPTURE; break;
                case Sequence.FRONT_CAPTURE:            _sequence = Sequence.FRONT_COMPLETE; break;
                case Sequence.FRONT_COMPLETE:           _sequence = Sequence.SIDE_CAPTURE_EXPALIN; break;
                case Sequence.SIDE_CAPTURE_EXPALIN:     _sequence = Sequence.SIDE_CAPTURE; break;
                case Sequence.SIDE_CAPTURE:             _sequence = Sequence.SIDE_COMPLETE; break;
                case Sequence.SIDE_COMPLETE:            _sequence = Sequence.ALL_COMPLETE; break;
            }
        }

        private void StartMute()
        {
            muteCheckController.ShowCanvas();
            //muteCheckController.StartFrontTutorial1();
        }
        
        private void EndMute()
        {
            muteCheckController.HideCanvas();
        }
        
        private void StartVolume()
        {
            volumeController.ShowCanvas();
            volumeController.StartFrontTutorial1();
        }

        private void EndVolume()
        {
            volumeController.HideCanvas();
        }


        private void StartBookDescriptionAlign()
        {
            cameraDescriptionController.ShowCanvas();
            cameraDescriptionController.StartSpeach();
            //cameraDescriptionController.Speech();
        }

        private void EndBookDescriptionAlign()
        {
            cameraDescriptionController.HideCanvas();
        }

        private void StartGyroAlign()
        {
            gyroController.ShowCanvas();
            gyroController.Speech();
        }

        private void EndGyroAlign()
        {
            gyroController.HideCanvas();
        }


        private void StartFrontExplain()
        {
            frontCaptureExplainController.ShowCanvas();
            frontCaptureExplainController.StartFlow();
        }

        private void EndFrontExplain()
        {
            frontCaptureExplainController.HideCanvas();
        }

        private void StartFrontCapture()
        {
            cameraSceneFrontSelfieController.UpdateRequiredPosenet(true);
            cameraSceneFrontSelfieController.currentPoseType = "front";
            frontCaptureController.ShowCanvas();
        }

        private void EndFrontCapture()
        {
            frontCaptureController.HideCanvas();
            cameraSceneFrontSelfieController.UpdateRequiredPosenet(false);
        }

        private void StartFrontCaptureComplete()
        {
            // frontCaptureCompleteController.ShowCanvas();
            // frontCaptureCompleteController.StartFrow();
        }

        private void EndFrontCaptureComplete()
        {
            // frontCaptureCompleteController.HideCanvas();
        }

        private void StartSideExplain()
        {
        }

        private void EndSideExplain()
        {

        }

        private void StartSideCapture()
        {
            cameraSceneFrontSelfieController.UpdateRequiredPosenet(true);
            cameraSceneFrontSelfieController.currentPoseType = "side";
        }

        private void EndSideCapture()
        {
            cameraSceneFrontSelfieController.UpdateRequiredPosenet(false);
        }

        private void StartSideCaptureComplete()
        {
        }

        private void EndSideCaptureComplete()
        {

        }

        private void OnGyroComplete()
        {
            //Debug.Log("Sequece:::OnGyroComplete");
            // required next step
            if (_sequence == Sequence.GYRO_ALIGN)
            {
                _sequence = Sequence.FRONT_CAPTURE_EXPLAIN;
            }

            if (_sequence == Sequence.FRONT_CAPTURE || _sequence == Sequence.SIDE_CAPTURE)
            {
                cameraSceneFrontSelfieController.UpdateRequiredPosenet(true);
            }
        }
        
        [Conditional("ENV_DEVELOP")]
        private void DebugLog(string message)
        {
            Debug.Log($"{message}");
        }
    }
}