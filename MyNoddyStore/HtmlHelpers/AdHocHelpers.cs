using System;
using System.Text;
//using System.Web.SessionState;
//using Microsoft.AspNet.Session;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
//using Newtonsoft.Json;
using MyNoddyStore.Abstract;
using MyNoddyStore.Models;
using MyNoddyStore.AdHocHelpers;


namespace MyNoddyStore.AdHocHelpers
{
    public static class AdHocHelpers   // a public static class allows extension method!
    {
        //Helper method required for paging.
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> sequence)
        {
            return sequence.Where(e => e != null);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
            where T : struct
        {
            return sequence.Where(e => e != null).Select(e => e.Value);
        }

        public static IEnumerable<T> EmptyArrayIfNull<T>(this IEnumerable<T> sequence) //todo clean up
        {
            if (sequence == null) {
                //string[] emptyArray = new string[] { };            // handle a product with no categories by returning an empty array object.
                //T[] emptyArray = new T[] { };                        // handle a product with no categories by returning an empty array object.

                //return emptyArray; //.Cast<T>();                         //we could cast to type T but this is superfluous.
                return new T[] { };
            }
            else
            {
                return sequence;
            }
        }

        //public static void SetObjectAsJson(this ISession session, string key, object value) //todo use this values and change this class name or delete these methods
        //{
        //    session.SetString(key, JsonConvert.SerializeObject(value));
        //}

        //public static T GetObjectFromJson<T>(this ISession session, string key)
        //{
        //    var value = session.GetString(key);

        //    return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        //}

        public static T GetDataFromSession<T>(this HttpSessionStateBase session, string key)  //todo use this values and change this class name or delete these methods
        {
            try { return (T)session[key]; }
            catch { return default(T); }
        }
        /// <summary> 
        /// Set value. 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="session"></param> 
        /// <param name="key"></param> 
        /// <param name="value"></param> 
        public static void SetDataToSession<T>(this HttpSessionStateBase session, string key, object value)
        {
            session[key] = value;
        }

        //if countdown already started, return the remaining time. Otherwise return a default start value.
        public static int GetRemainingTimeOrSetDefault(this HttpSessionStateBase session)
        {
            int remainingMilliseconds; // countdown time variable

            DateTime countdownTime = session.GetDataFromSession<DateTime>("countdownTimeCsKey");

            if (countdownTime == DateTime.MinValue)
            {
                //set a new countdown time of 41 seconds (1 second extra to account for lag)
                session.SetDataToSession<string>("countdownTimeCsKey", DateTime.Now.AddMilliseconds(61000));
                remainingMilliseconds = 61000;
            }
            else
            {
                TimeSpan tsRemaining = countdownTime - DateTime.Now;
                remainingMilliseconds = (int)tsRemaining.TotalMilliseconds;  //convert to integer for passing to view.
            }
            return remainingMilliseconds;
        }

        //if countdown already started, return this value. Otherwise return int.Minvalue.
        public static int GetRemainingTime(this HttpSessionStateBase session)
        {
            int remainingMilliseconds; // countdown time variable
            DateTime countdownTime = session.GetDataFromSession<DateTime>("countdownTimeCsKey");

            if (countdownTime == DateTime.MinValue)
            {
                remainingMilliseconds = int.MinValue;
            }
            else
            {
                TimeSpan tsRemaining = countdownTime - DateTime.Now;
                remainingMilliseconds = (int)tsRemaining.TotalMilliseconds;  //convert to integer for passing to view.
            }
            return remainingMilliseconds;
        }

    }
}