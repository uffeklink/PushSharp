using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using PushSharp.Core;
using PushSharp.Apple;

namespace WebTestClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string pushText)
        {
            try
            {
                if (!string.IsNullOrEmpty(pushText))
                {
                    SentNotification(pushText, "e7f1cb61 6b33c8ac fa5124ed 85924351 346a554e 15ebb7fc ef03fb28 6d32d6eb");
                    ViewBag.PushTestResult = "OK";
                }
            }
            catch (Exception ex)
            {
                ViewBag.PushTestResult = ex.ToString();
            }
            

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        private void SentNotification(string text, string deviceToken)
        {
            deviceToken = deviceToken.Replace(" ", string.Empty);

            //var config = new ApnsConfiguration("push-cert.pfx", "push-cert-pwd");
            // var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "aps_development.cer", "OapT1986!");
            var certFile = Server.MapPath("~/App_Data/aps_1.cer");
            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, certFile, "OapT1986!");
            config.SkipSsl = true;
            // Create a new broker
            var broker = new ApnsServiceBroker(config);

            broker.OnNotificationFailed += this.NotificationFailed;
            broker.OnNotificationSucceeded += this.NotificationSucceeded;

            var appleMessageJson = new AppleMessageJson(text, "default", 1);
            var json = appleMessageJson.ToJson();

            // Start the broker
            broker.Start();

            // Queue a notification to send
            broker.QueueNotification(new ApnsNotification
            {
                DeviceToken = deviceToken,
                Payload = json
            });

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            broker.Stop();
        }

        protected void NotificationSucceeded(ApnsNotification a)
        {
            // MessageBox.Show("Notification Sent!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void NotificationFailed(ApnsNotification apnNotification, Exception aggregateEx)
        {

            // See what kind of exception it was to further diagnose
            if (aggregateEx is ApnsNotificationException)
            {
                var apnsEx = aggregateEx as ApnsNotificationException;

                // Deal with the failed notification
                var n = apnsEx.Notification;

               // MessageBox.Show($"Notification Failed: ID={n.Identifier}, Code={apnsEx.ErrorStatusCode}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else if (aggregateEx is ApnsConnectionException)
            {
                // Something failed while connecting (maybe bad cert?)
                // MessageBox.Show("Notification Failed(Bad APNS Connection)!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // MessageBox.Show(aggregateEx.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class AppleMessageJson
        {
            public AppleMessageJson(string message, string sound, int badge)
            {
                this.aps = new Aps(message, sound, badge);
            }

            public Aps aps { get; private set; }

            public JObject ToJson()
            {
                return JObject.FromObject(this);
            }

            public class Aps
            {
                public Aps(string message, string sound, int badge)
                {
                    this.alert = message;
                    this.sound = sound;
                    this.badge = badge;
                }

                public string alert { get; private set; }
                public int badge { get; private set; }
                public string sound { get; private set; }
            }
        }
    }
}