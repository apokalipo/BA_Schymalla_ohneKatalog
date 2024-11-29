using System.Text;
using System.IO;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections;
using UnityEngine.Networking;
using Samples.Whisper;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField, Tooltip(("Your OpenAI API key. If you use a restricted key, please ensure that it has permissions for /v1/audio."))] 
    private string openAIKey = "sk-proj-MdfCtDV9W5UcPQI9ukowT3BlbkFJtU87lQcMFfeEgMNtHPZ4";
    private readonly string outputFormat = "mp3";
    private AudioSource audioSource;
    private const bool deleteCachedFile = true;
    public ChatGPTManager manager;
    public SpeechToText stt;
    
    
    [System.Serializable]
    private class TTSPayload
    {
        public string model;
        public string input;
        public string voice;
        public string response_format;
        public float speed;
    }

        private void OnEnable()
    {
        if (!audioSource) this.audioSource = GetComponent<AudioSource>();
    }

    public async Task<byte[]> RequestTextToSpeech(string text, string model, string voice, float speed)
    {
        Debug.Log("Sending new request to OpenAI TTS.");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAIKey);

        TTSPayload payload = new TTSPayload
        {
            model = "tts-1",
            input = text,
            voice = voice,
            response_format = this.outputFormat,
            speed = speed
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        var httpResponse = await httpClient.PostAsync(
            "https://api.openai.com/v1/audio/speech", 
            new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        );

        byte[] response = await httpResponse.Content.ReadAsByteArrayAsync();

        if (httpResponse.IsSuccessStatusCode) return response;
        
        Debug.Log("Error: " + httpResponse.StatusCode);
        return null;
    }

    public void SetAPIKey(string openAIKey) => this.openAIKey = openAIKey;

     public void ProcessAudioBytes(byte[] audioData)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "audio.mp3");
        File.WriteAllBytes(filePath, audioData);

        StartCoroutine(LoadAndPlayAudio(filePath));
    }
    
    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else Debug.LogError("Audio file loading error: " + www.error);
        
        if (deleteCachedFile) File.Delete(filePath);
        manager.waiting = false;
        stt.loading.SetActive(false);
    }
}