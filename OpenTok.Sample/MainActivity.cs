using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Opentok.Android;

namespace OpenTok.Sample
{
    [Activity(Label = "OpenTok.Sample", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : Activity, Session.ISessionListener, PublisherKit.IPublisherListener,
        SubscriberKit.IVideoListener
    {
        private const string ApiKey = "45568962";
        private const string SessionId = "2_MX40NTU2ODk2Mn5-MTQ2MTIzNTI4Mjk2Mn5uWGhBNWhLeTBEclEyZ0thQzJ1VGw4czd-UH4";

        private const string Token =
            "T1==cGFydG5lcl9pZD00NTU2ODk2MiZzaWc9YWZjN2IyMDRkMDlhODgxNjgwOGIxODJjOWViN2M4ZTM4ZTI1NGUxODpyb2xlPXB1Ymxpc2hlciZzZXNzaW9uX2lkPTJfTVg0ME5UVTJPRGsyTW41LU1UUTJNVEl6TlRJNE1qazJNbjV1V0doQk5XaExlVEJFY2xFeVowdGhRekoxVkd3NGN6ZC1VSDQmY3JlYXRlX3RpbWU9MTQ2MTIzNTMwMSZub25jZT0wLjcxODEyNzM3MjcwMzQyMjImZXhwaXJlX3RpbWU9MTQ2MTIzODg1OSZjb25uZWN0aW9uX2RhdGE9";

        private readonly List<Stream> _streams = new List<Stream>();

        // Spinning wheel for loading subscriber view
        private ProgressBar _loadingSub;
        private Publisher _publisher;

        private RelativeLayout _publisherViewContainer;

        private Session _session;
        private Subscriber _subscriber;
        private RelativeLayout _subscriberViewContainer;

        public void OnError(PublisherKit p0, OpentokError p1)
        {
        }

        public void OnStreamCreated(PublisherKit p0, Stream p1)
        {
            _streams.Add(p1);
            if (_subscriber == null)
            {
                SubscribeToStream(p1);
            }
        }

        public void OnStreamDestroyed(PublisherKit p0, Stream p1)
        {
            if (_subscriber != null)
            {
                UnsubscribeFromStream(p1);
            }
        }

        public void OnConnected(Session p0)
        {
            if (_publisher != null)
                return;

            _publisher = new Publisher(this, "publisher");
            _publisher.SetPublisherListener(this);
            AttachPublisherView(_publisher);
            p0.Publish(_publisher);
        }

        public void OnDisconnected(Session p0)
        {
            if (_publisher != null)
            {
                _publisherViewContainer.RemoveView(_publisher.View);
            }

            if (_subscriber != null)
            {
                _subscriberViewContainer.RemoveView(_subscriber.View);
            }

            _publisher = null;
            _subscriber = null;
            _streams.Clear();
            _session = null;
        }

        public void OnError(Session p0, OpentokError p1)
        {
        }

        public void OnStreamDropped(Session p0, Stream p1)
        {
            if (_subscriber != null)
            {
                UnsubscribeFromStream(p1);
            }
        }

        public void OnStreamReceived(Session p0, Stream p1)
        {
            _streams.Add(p1);
            if (_subscriber == null)
            {
                SubscribeToStream(p1);
            }
        }

        public void OnVideoDataReceived(SubscriberKit p0)
        {
            _loadingSub.Visibility = ViewStates.Gone;
            AttachSubscriberView(_subscriber);
        }

        public void OnVideoDisableWarning(SubscriberKit p0)
        {
            throw new NotImplementedException();
        }

        public void OnVideoDisableWarningLifted(SubscriberKit p0)
        {
            throw new NotImplementedException();
        }

        public void OnVideoDisabled(SubscriberKit p0, string p1)
        {
            throw new NotImplementedException();
        }

        public void OnVideoEnabled(SubscriberKit p0, string p1)
        {
            throw new NotImplementedException();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _publisherViewContainer = FindViewById<RelativeLayout>(Resource.Id.publisherview);
            _subscriberViewContainer = FindViewById<RelativeLayout>(Resource.Id.subscriberview);
            _loadingSub = FindViewById<ProgressBar>(Resource.Id.loadingSpinner);

            SessionConnect(SessionId, Token);
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (!IsFinishing)
                return;

            if (_session != null)
            {
                _session.Disconnect();
            }
        }

        public override void OnBackPressed()
        {
            if (_session != null)
            {
                _session.Disconnect();
            }
            base.OnBackPressed();
        }

        private void SessionConnect(string sessionId, string token)
        {
            if (_session != null)
                return;

            _session = new Session(this, ApiKey, sessionId);
            _session.SetSessionListener(this);
            _session.Connect(token);
        }

        private void SubscribeToStream(Stream stream)
        {
            _subscriber = new Subscriber(this, stream);
            _subscriber.SetVideoListener(this);
            _session.Subscribe(_subscriber);
            // start loading spinning
            _loadingSub.Visibility = ViewStates.Visible;
        }

        private void UnsubscribeFromStream(Stream stream)
        {
            _streams.Remove(stream);
            if (_subscriber.Stream.StreamId.Equals(stream.StreamId))
            {
                _subscriberViewContainer.RemoveView(_subscriber.View);
                _subscriber = null;
                if (_streams.Count != 0)
                {
                    SubscribeToStream(_streams[0]);
                }
            }
        }

        private void AttachSubscriberView(Subscriber subscriber)
        {
            var layoutParams = new RelativeLayout.LayoutParams(
                Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);


            _subscriberViewContainer.AddView(_subscriber.View, layoutParams);
            subscriber.SetStyle(BaseVideoRenderer.StyleVideoScale,
                BaseVideoRenderer.StyleVideoFill);
        }

        private void AttachPublisherView(Publisher publisher)
        {
            _publisher.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);
            var layoutParams = new RelativeLayout.LayoutParams(320, 240);
            layoutParams.AddRule(LayoutRules.AlignParentBottom, -1);
            layoutParams.AddRule(LayoutRules.AlignParentRight, -1);
            _publisherViewContainer.AddView(publisher.View, layoutParams);
        }

        public void OnVideoDisabled(SubscriberKit p0)
        {
        }
    }
}