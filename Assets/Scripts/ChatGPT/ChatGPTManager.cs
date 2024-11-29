using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;
public class ChatGPTManager : MonoBehaviour
{
    [SerializeField] TextToSpeech tts;
    public OnResponseEvent OnResponse;
    [System.Serializable]
    
    public class OnResponseEvent: UnityEvent<string> {}
    private OpenAIApi openAI = new OpenAIApi("sk-proj-MdfCtDV9W5UcPQI9ukowT3BlbkFJtU87lQcMFfeEgMNtHPZ4",null);
    private	List<ChatMessage> messages = new List<ChatMessage>();
    public hexCodeFromString saidPart;
    public bool waiting = false;

    public AudioSource teststart;

    
    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";

        waiting = true;
        var response = await openAI.CreateChatCompletion(request);

        if(response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            OnResponse.Invoke(chatResponse.Content);
            byte[] speak = await tts.RequestTextToSpeech(saidPart.answer,"tts-1","onyx",1f);
            tts.ProcessAudioBytes(speak);
        }
    }
    void Awake()
    {
        teststart.Play();
        string katalog = hexCodeFromString.ReadString(Application.streamingAssetsPath + "/Katalog.txt");
        ChatMessage instructions = new ChatMessage();
        string anforderungen ="Du bist ein Berater für Wandfarben in einem Malerunternehmen. Auf dich werden Kunden zukommen und dich nach empfehlungen für Wandfarben fragen. Antworte ihnen freundlich und hilf ihnen auf ihrer suche nach einer Farbe für ihre Wand. Du kannst auch kurze fragen stellen um herauszufinden wonach deine Kunden suchen. Wenn du eine Farbe angibst, dann gib den Hexcode dieser Farbe am Ende der Nachricht an. Gib dabei IMMER nur eine Farbe gleichzeitig an. Nach dem Hexcode der Farbe darf nichts anderes stehen. Der Hexcode soll außerdem seperat da stehen, ohne dass du einen ganzen Satz dazu schreibst, beispielweise sollst du nicht sagen der Hexcode lautet: #FFFFFF , sondern einfach nur #FFFFFF. Wenn du denkst, dass der Kunde eine Farbe gefunden hat die ihm gefällt und das gespräch beendet ist schreibe stattdessen eine verabschiedung und #THE-END am Ende deiner Nachricht.";
        if (katalog!="")
        {
            anforderungen += "Beachte dass du nur Farben aus einem Katalog empfehlen kannst, welcher alle Farben enthält die dein Unternehmen verkauft. Benutze nur Farben die in diesem Katalog enthalten sind. Suche aus dem Katalog die Farbe die am besten auf die Wünsche des Kunden passt. Aber denke auch hier dran den Hexcode der Farbe nur einzeln am Ende deiner Nachricht zu nennen ohne dass er sich in einem Satz befindet. Nenne dem Kunden trotzdem den dazugörigen Namen der Farbe (NICHT DEN HEXCODE!), aber vermeide den Namen zweimal in einer Nachricht zu erwähnen. Sollte dem Kunden keine einzige Farbe aus dem Katalog gefallen, sag ihm, dass dies die einzigen Farben sind, die ihr vorrätig habt. Der Katalog lautet: "+ katalog;
        }
        Debug.Log(anforderungen);
        instructions.Content = anforderungen;
        instructions.Role = "system";
        messages.Add(instructions);
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";
    }
}
