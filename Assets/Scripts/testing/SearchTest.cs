using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.Services;

public class SearchTest : MonoBehaviour
{
    void Start() {
        string apiKey = "AIzaSyAjyrKVhLnM9i1RtjzBRyzlDkk7mnvZVz4";
        string cx = "d1f101f71fc714108";
        string query = "Dog";

        var svc = new CustomSearchAPIService(new BaseClientService.Initializer { ApiKey = apiKey });
        var listRequest = svc.Cse.List();
        listRequest.Q=query;

        listRequest.Cx = cx;
        var search = listRequest.Execute();

        foreach (var result in search.Items)
        {
            Debug.Log("Title: {0} " + result.Title);
            Debug.Log("Link: {0} " + result.Link);
        }
    }
}
