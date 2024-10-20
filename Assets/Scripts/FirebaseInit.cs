using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Analytics;
using Firebase.Crashlytics;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseInit instance;

    private Firebase.FirebaseApp app;
    // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                instance = this;
                Debug.Log("Firebase Init");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void MoneyEvent(double money)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.ParameterPrice, new Parameter("moneyPerRound", money));
    }
}
