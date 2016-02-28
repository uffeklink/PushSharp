using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

            SentNotification("Hej Arun, kommer denne frem?", "e7f1cb61 6b33c8ac fa5124ed 85924351 346a554e 15ebb7fc ef03fb28 6d32d6eb");
        }


        private void SentNotification(string text, string deviceToken)
        {
            deviceToken = deviceToken.Replace(" ", string.Empty);

            //var config = new ApnsConfiguration("push-cert.pfx", "push-cert-pwd");
            // var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "aps_development.cer", "OapT1986!");
            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "aps_1.cer", "OapT1986!");
            // Create a new broker
            var broker = new ApnsServiceBroker(config);

            broker.OnNotificationFailed += this.NotificationFailed;
            broker.OnNotificationSucceeded += this.NotificationSucceeded;

            // Start the broker
            broker.Start();

            // Queue a notification to send
            broker.QueueNotification(new ApnsNotification
            {
                DeviceToken = deviceToken,
                Payload = JObject.Parse("{\"aps\":{\"badge\":7}}")
            });

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
    }
}
