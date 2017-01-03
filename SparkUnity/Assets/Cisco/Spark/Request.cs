﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark
{
    public class Request : MonoBehaviour
    {

        // Singleton Request Instance.
        public static Request Instance;

        // Publically Set Variables.
        public const string BaseUrl = "https://api.ciscospark.com/v1";
        public string AuthenticationToken = "";

        void Awake()
        {
            // Assign singleton.
            Instance = this;
        }

        /// <summary>
        /// Generate a Web Request to Spark.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="requestType">Request type.</param>
        /// <param name="data">Data to upload.</param>
        public UnityWebRequest Generate(string resource, string requestType, byte[] data = null)
        {
            // Setup Headers.
            var www = new UnityWebRequest(BaseUrl + "/" + resource);
            www.SetRequestHeader("Authorization", "Bearer " + AuthenticationToken);
            www.SetRequestHeader("Content-type", "application/json; charset=utf-8");
            www.method = requestType;
            www.downloadHandler = new DownloadHandlerBuffer();

            // Is there data to upload?
            if (data != null)
            {
                www.uploadHandler = new UploadHandlerRaw(data);
            }

            return www;
        }

        /// <summary>
        /// Retrives an existing record from the Spark service by ID.
        /// </summary>
        /// <returns>The record's data.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="type">SparkType being retrieved.</param>
        /// <param name="error">Error.</param>
        /// <param name="result">Result.</param>
        public IEnumerator GetRecord(string id, SparkType type, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            var url = string.Format("{0}/{1}", SparkResources.Instance.UrlEndpoints[type], id);
            using (var www = Generate(url, UnityWebRequest.kHttpVerbGET))
            {
                yield return www.Send();

                if (www.isError)
                {
                    Debug.LogError("Couldn't connect to Spark: " + www.error);
                }
                else
                {
                    var returnedData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                    if (returnedData.ContainsKey("message"))
                    {
                        // Spark side error.
                        error(new SparkMessage(returnedData));
                    }
                    else
                    {
                        // Returned data.
                        result(returnedData);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the record.
        /// </summary>
        /// <returns>The record.</returns>
        /// <param name="data">Data.</param>
        /// <param name="type">Type.</param>
        /// <param name="error">Error.</param>
        /// <param name="result">Result.</param>
        public IEnumerator CreateRecord(Dictionary<string, object> data, SparkType type, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            // Create request.
            var recordDetails = System.Text.Encoding.UTF8.GetBytes(Json.Serialize(data));
            using (var www = Generate(SparkResources.Instance.UrlEndpoints[type], UnityWebRequest.kHttpVerbPOST, recordDetails))
            {
                yield return www.Send();

                if (www.isError)
                {
                    // Network error.
                    Debug.LogError("Couldn't connect to Spark: " + www.error);
                }
                else
                {
                    var returnedData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                    if (returnedData.ContainsKey("message"))
                    {
                        // Spark side error.
                        error(new SparkMessage(returnedData));
                    }
                    else
                    {
                        // Returned data.
                        result(returnedData);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the record.
        /// </summary>
        /// <returns>The record.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="data">Data.</param>
        /// <param name="type">Type.</param>
        /// <param name="error">Error.</param>
        /// <param name="result">Result.</param>
        public IEnumerator UpdateRecord(string id, Dictionary<string, object> data, SparkType type, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            // Update Record.
            var recordDetails = System.Text.Encoding.UTF8.GetBytes(Json.Serialize(data));
            using (var www = Generate(SparkResources.Instance.UrlEndpoints[type] + "/" + id, UnityWebRequest.kHttpVerbPUT, recordDetails))
            {
                yield return www.Send();

                if (www.isError)
                {
                    // Network error.
                    Debug.LogError("Couldn't connect to Spark: " + www.error);
                }
                else
                {
                    var returnedData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                    if (returnedData.ContainsKey("message"))
                    {
                        // Spark side error.
                        error(new SparkMessage(returnedData));
                    }
                    else
                    {
                        // Returned data.
                        result(returnedData);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the record from Spark.
        /// </summary>
        /// <returns>The record.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="type">Type.</param>
        /// <param name="error">Error.</param>
        /// <param name="success">Success.</param>
        public IEnumerator DeleteRecord(string id, SparkType type, Action<SparkMessage> error, Action<bool> success)
        {
            // Create Request.
            using (var www = Generate(SparkResources.Instance.UrlEndpoints[type] + "/" + id, UnityWebRequest.kHttpVerbDELETE))
            {
                // Make request.
                yield return www.Send();

                // Check result.
                if (www.isError)
                {
                    // Network Error.
                    Debug.LogError("Couldn't connect to Spark: " + www.error);
                }
                else
                {
                    // Deletion gives 204 on success;
                    if (www.responseCode == 204)
                    {
                        success(true);
                    }
                    else
                    {
                        var data = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                        error(new SparkMessage(data));
                    }
                }
            }
        }

        public IEnumerator ListRecords(Dictionary<string, string> constraints, SparkType type, Action<SparkMessage> error, Action<List<object>> result)
        {
            string queryString = System.Text.Encoding.UTF8.GetString(UnityWebRequest.SerializeSimpleForm(constraints));
            string url = string.Format("{0}?{1}", SparkResources.Instance.UrlEndpoints[type], queryString);
            using (var www = Generate(url, UnityWebRequest.kHttpVerbGET))
            {
                yield return www.Send();

                if (www.isError)
                {
                    Debug.LogError("Couldn't connect to Spark: " + www.error);
                }
                else
                {
                    var returnedData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                    if (returnedData.ContainsKey("message"))
                    {
                        // Spark side error.
                        error(new SparkMessage(returnedData));
                    }
                    else
                    {
                        // Returned data.
                        var items = returnedData["items"] as List<object>;
                        result(items);
                    }
                }
            }
        }
    }
}