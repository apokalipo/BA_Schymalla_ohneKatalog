using System;
using System.Collections;
using System.Collections.Generic;
using Samples.Whisper;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

//https://medium.com/@louisvanhove/microphone-input-has-never-been-easier-in-unity-no-library-no-plugins-366062e7c74a
public class VolumeDetect : MonoBehaviour
{
    public OnResponseEvent OnSpeak;
        [System.Serializable]
        public class OnResponseEvent: UnityEvent {}
    public AudioClip _clipRecord = null;
    private string _device;
    private int _sampleWindow = 128;
    //private bool _isInitialized;
    public static float MicLoudness;
    private float time;

    public SpeechToText recorder;
    public ChatGPTManager manager;

    public hexCodeFromString end;
    public bool isRecording = false;

    public AudioSource assistent;
    public TextToSpeech tts;
    public AudioSource startRec;
    public AudioSource stopRec;

    public float recordTime;

    public void InitializeMic()
    {
        AudioClip.Destroy(_clipRecord);
        _clipRecord = Microphone.Start(null, true, 20,44100);
    }

    private float LevelMax()
    {
        if (_clipRecord.loadState == AudioDataLoadState.Loaded)
        {
            float levelMax = 0 ;
            float[] waveData = new float[_sampleWindow];
            int micPosition = Microphone.GetPosition(null) - (_sampleWindow+1);
            if(micPosition < 0) return 0;
            _clipRecord.GetData(waveData,micPosition);
            for(int i=0; i < _sampleWindow; i++)
            {
                float wavePeak = waveData[i] * waveData[i];
                if (levelMax < wavePeak)
                {
                    levelMax = wavePeak;
                }
            }
            return levelMax;
        }
        else
        {
            Debug.LogError("AudioClip ist noch nicht vollständig geladen.");
            return LevelMax();
        }
    }

    void OnEnable()
    {
        InitializeMic();
        //_isInitialized = true;
    }

    void Update()
    {
        if(isRecording)
        {
            recordTime += Time.deltaTime;
        }
        //time += Time.deltaTime;
        //MicLoudness = LevelMax()*1000;

        if(!assistent.isPlaying && !manager.waiting)
        {
            time += Time.deltaTime;
            MicLoudness = LevelMax()*1000;
            if(end.shutDown)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if(MicLoudness >=2.0)
            {
                Debug.Log("LevelMax: " + MicLoudness);
                time = 0;
                if(!isRecording)
                {
                    Microphone.End(null);
                    InitializeMic();
                    startRec.Play();
                    isRecording=true;
                }
            }
            else if (time >= 2 && (MicLoudness <2.0))
            {
                if(isRecording)
                {
                    stopRec.Play();
                    Debug.Log("Dauer: " + recordTime);
                    //endOfRec = Microphone.GetPosition(null);
                    time = 0;
                    recordTime = 0;
                    isRecording = false;
                    Microphone.End(null);
                    recorder.EndRecording();
                }
            }
        }
        //this.gameObject.transform.localScale = new Vector3(MicLoudness, MicLoudness, MicLoudness) * 10000 + Vector3.one;

    }
}
