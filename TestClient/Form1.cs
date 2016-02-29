using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using PushSharp.Core;
using PushSharp.Apple;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void butSentNotification_Click(object sender, EventArgs e)
        {
            LocalPing("www.jp.dk");
            // SentNotification(this.textBox1.Text, "20e2d9c5 2e0f209c de3fec6d 43db5cde 7ec09ec4 7a7a1ab7 8a62377e 18a9f04b");
            SentNotification(this.textBox1.Text, "e7f1cb61 6b33c8ac fa5124ed 85924351 346a554e 15ebb7fc ef03fb28 6d32d6eb");
        }


        private void SentNotification(string message, string deviceToken)
        {
            deviceToken = deviceToken.Replace(" ", string.Empty);

            //var config = new ApnsConfiguration("push-cert.pfx", "push-cert-pwd");
            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, "aps_1.cer", "OapT1986!");
            config.SkipSsl = true;

            // Create a new broker
            var broker = new ApnsServiceBroker(config);

            broker.OnNotificationFailed += this.NotificationFailed;
            broker.OnNotificationSucceeded += this.NotificationSucceeded;

            // Start the broker
            broker.Start();

            var appleMessageJson = new AppleMessageJson(message, "default", 1);
            var json = appleMessageJson.ToJson();

            var apnsNotification =  new ApnsNotification
            {
                DeviceToken = deviceToken,
                Payload = json
            };

            // Queue a notification to send
            broker.QueueNotification(apnsNotification);

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            broker.Stop();
        }

        protected void NotificationSucceeded(ApnsNotification a)
        {
            MessageBox.Show("Notification Sent!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void NotificationFailed(ApnsNotification apnNotification, Exception aggregateEx)
        {

            // See what kind of exception it was to further diagnose
            if (aggregateEx is ApnsNotificationException)
            {
                var apnsEx = aggregateEx as ApnsNotificationException;

                // Deal with the failed notification
                var n = apnsEx.Notification;

                MessageBox.Show($"Notification Failed: ID={n.Identifier}, Code={apnsEx.ErrorStatusCode}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else if (aggregateEx is ApnsConnectionException)
            {
                // Something failed while connecting (maybe bad cert?)
                MessageBox.Show("Notification Failed(Bad APNS Connection)!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(aggregateEx.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LocalPing(string ipadr)
        {
            // Ping's the local machine.
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            for (int i = 0; i < 1; i++)
            {
                PingReply reply = pingSender.Send(ipadr, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
//                MessageBox.Show("ping ok");
                    Console.WriteLine("Address: {0}", reply.Address.ToString());
                    Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                    Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                    Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                    Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                }
            }
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

