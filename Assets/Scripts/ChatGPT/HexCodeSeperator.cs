using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class hexCodeFromString : MonoBehaviour
{

    public OnResponseEvent OnResponse;
    [System.Serializable]
    public class OnResponseEvent: UnityEvent<string> {}
    [SerializeField] Material colorMat;
    public List<String> historyColor;
    public string answer;
    public bool shutDown = false;

    void Start()
    {
        colorMat.color = new Color(colorMat.color.r, colorMat.color.g, colorMat.color.b, 0);
    }
    public void splitResponse(string response)
    {//Trennt die Nachricht von ChatGPT in 2 Teile: Text und Hexcode
        if (response.Contains("#"))
        {
            string[] textAndColor = response.Split("#");
            answer = textAndColor[0];
            Debug.Log(textAndColor[0]);
            Debug.Log(textAndColor[1]);
            if (textAndColor[1] == "THE-END")
            {
                shutDown = true;
                //Application.Quit();
            }
            else
            {
                OnResponse.Invoke(textAndColor[0]);
                Match hexCode = Regex.Match(("#" + textAndColor[1]), @"#(.{6})");
                if (hexCode.Success)
                {
                    Console.WriteLine(hexCode);
                }
                else
                {
                    Console.WriteLine("Fehler");
                }
                string hexColor = hexCode.ToString();
                historyColor.Add(hexColor);//fügt Hexcode zu einer Liste, aller letzten Farben hinzu
                WriteString(hexColor);//schreibt Hexcode in Textdatei
                //Färbt das Material der Wände in die von ChatGPT empfohlene Farbe
                ColorUtility.TryParseHtmlString(hexColor, out Color newColor);
                colorMat.color = newColor;
            }
        }
        else
        {
            answer = response;
            OnResponse.Invoke(answer);
        }
    }
    public static void WriteString(String order)
    {//Funktion um in eine Textdatei zu schreiben
        string path = Application.persistentDataPath + "/bestellung.txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(order);
            writer.Close();
        StreamReader reader = new StreamReader(path);
        //Debug.Log(reader.ReadToEnd());
        reader.Close();
        }
    public static string ReadString(string path)
    {//Liest den Inhalt der Angegebenen Textdatei
        //path = Application.persistentDataPath + "/bestellung.txt";
        StreamReader reader = new StreamReader(path);
        //Debug.Log(reader.ReadToEnd());
        string inhalt = reader.ReadToEnd();
        reader.Close();
        return inhalt;
    }
}
