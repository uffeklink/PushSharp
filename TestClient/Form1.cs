using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
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
            // SentNotification(this.textBox1.Text, "f35090b623a61fac5754117ee72515cf1562ebd9055000d2d67c4c2c0c82d560"); // søren
            // SentNotification(this.textBox1.Text, "e7cc8cae3d9db2bb0fcf4e7a57606842b245fa237933c4e94d6b39a596669ebd"); //TK
            SentNotification(this.textBox1.Text, new string[] { "aa4f73f5a3117bd4321c826a5cf5223d10f6d93386c63627861d48a19f3fd844"} ); // Arun
        }


        private void xx( string message, string devideToken)
        {
            var certs = new X509Certificate2Collection();
            var xcert = new X509Certificate2();
            byte[] notificationData = new byte[] { };

            var appleMessageJson = new AppleMessageJson(message, "default", 1);
            var json = appleMessageJson.ToJson();

            var apnsNotification = new ApnsNotification(devideToken, json);
            xcert.Import("Certificates-Push-dev.p12", "OapT1986!", X509KeyStorageFlags.UserKeySet);
            
            certs.Add(xcert);

            string apsHost = "gateway.sandbox.push.apple.com";
            var tcpClient = new TcpClient(apsHost, 2195);
            using (var sslStream = new SslStream(tcpClient.GetStream()))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                sslStream.AuthenticateAsClient(apsHost, certs, System.Security.Authentication.SslProtocols.Tls, false);

                if (!sslStream.IsMutuallyAuthenticated)
                    throw new ApnsConnectionException("SSL Stream Failed to Authenticate", null);

                if (!sslStream.CanWrite)
                    throw new ApnsConnectionException("SSL Stream is not Writable", null);

                notificationData = apnsNotification.ToBytes();
                sslStream.Write(notificationData, 0, notificationData.Length);
                sslStream.Flush();
            }

            return;
        }
        private void SentNotification(string message, string[] deviceTokens)
        {
            // Configuration
            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "CertificatesOdendoPush.cer", "OapT1986!");

            // Create a new broker
            var broker = new ApnsServiceBroker(config);

            // Wire up events
            broker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = ex as ApnsNotificationException;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Console.WriteLine($"Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    }
                    else {
                        // Inner exception might hold more useful information like an ApnsConnectionException           
                        Console.WriteLine($"Notification Failed for some (Unknown Reason) : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            broker.OnNotificationSucceeded += (notification) => {
                Console.WriteLine(@"Notification Sent!");
            };

            // Start the broker
            broker.Start();

            foreach (var deviceToken in deviceTokens)
            {
                // Queue a notification to send
                broker.QueueNotification(new ApnsNotification
                {
                    DeviceToken = deviceToken,
                    Payload = JObject.Parse("{\"aps\":{\"badge\":7}}")
                });
            }

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

