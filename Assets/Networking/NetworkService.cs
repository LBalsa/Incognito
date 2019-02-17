using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NetworkService
{
    private const string xmlApi =
        "http://api.openweathermap.org/data/2.5/weather?q=London&APPID=80b8b1ffd0dedb0bfbb4cb06ac64b0e1&mode=xml";

    private bool IsResponseValid(WWW www)
    {
        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Connection failed.");
            return false;
        }
        else if (string.IsNullOrEmpty(www.text))
        {
            Debug.Log("Data corrupted.");
            return false;
        }
        else
        {
            return true;
        }
    }

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        WWW www = new WWW(url);
        yield return www;

        if (!IsResponseValid(www))
            yield break;

        callback(www.text);
    }

    public IEnumerator GetWeatherXML(Action<string> callback)
    {
        return CallAPI(xmlApi, callback);
    }
}