using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI.Chat;

public class GPTTest : MonoBehaviour
{
    const string key = "";
    void Start() {
        ChatClient client = new ChatClient(model: "gpt-4o-mini", apiKey: key);
        ChatCompletion completion = client.CompleteChat("Say 'this is a test.'");

        Debug.Log($"[ASSISTANT]: {completion.Content[0].Text}");
    }
}
