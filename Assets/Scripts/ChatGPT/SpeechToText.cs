using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Samples.Whisper
{
    public class SpeechToText : MonoBehaviour
    {
        public OnResponseEvent OnTranscript;
        [System.Serializable]
        public class OnResponseEvent: UnityEvent<string> {}
        //[SerializeField] public Button recordButton;
        [SerializeField] public TextMeshProUGUI message;
        //[SerializeField] public Dropdown dropdown;        
        private readonly string fileName = "output.wav";
        //private readonly int duration = 5;
        
        private AudioClip record;
        public bool isRecording = false;
        private float time;
        private OpenAIApi openai = new OpenAIApi("sk-proj-MdfCtDV9W5UcPQI9ukowT3BlbkFJtU87lQcMFfeEgMNtHPZ4",null);
        public VolumeDetect volume;
        [SerializeField] public GameObject loading;

        /*
        private void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            //recordButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
            StartRecording();
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }
        */
        
        public void StartRecording()
        {
            Debug.Log("Nehme Auf");
            isRecording = true;
            //recordButton.enabled = false;

            //var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            //#if !UNITY_WEBGL
            //clip = Microphone.Start(dropdown.options[index].text, false, 3599, 44100);
            //clip = Microphone.Start(null, false, 10, 44100);
            //clip = volume._clipRecord;
            //Debug.Log(clip);
            //#endif
        }

        public async void EndRecording()
        {
            record = volume._clipRecord;
            float[] samples = new float[record.samples];
            /*
            record.GetData(samples,0);
            float[] whenSpoken = new float[volume.endOfRec];
            int i;
            for(i=0; i<volume.endOfRec; i++)
            {
                whenSpoken[i] = samples[i];
            }
            record.SetData(whenSpoken,0);
            Debug.Log(i);*/
            loading.SetActive(true);
            message.text = "Transcripting...";
            byte[] data = SaveWav.Save(fileName, record);
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                //File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "de"
            };
            var res = await openai.CreateAudioTranscription(req);
            Debug.Log(res);
            message.text = res.Text;
            //recordButton.enabled = true;
            Debug.Log("Ende");
            Debug.Log(message.text);
            OnTranscript.Invoke(message.text);
            volume.InitializeMic();
        }

        /*private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                
                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }
        */
    }
}
