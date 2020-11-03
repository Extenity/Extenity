using System;
using System.Collections.Specialized;
using System.Net;

namespace Extenity.IntegrationToolbox
{

	public class DiscordMessageSender : IDisposable
	{
		private WebClient WebClient;
		private NameValueCollection Values;
		public string WebhookURL;

		public DiscordMessageSender(string webhookURL, string overrideUsername = null, string overrideAvatarURL = null)
		{
			Values = new NameValueCollection();

			WebhookURL = webhookURL;

			if (!string.IsNullOrWhiteSpace(overrideUsername))
			{
				Values.Add("username", overrideUsername);
			}
			if (!string.IsNullOrWhiteSpace(overrideAvatarURL))
			{
				Values.Add("avatar_url", overrideAvatarURL);
			}

			WebClient = new WebClient();
		}

		public void Dispose()
		{
			WebClient.Dispose();
		}

		public void SendMessage(string msgSend)
		{
			if (string.IsNullOrWhiteSpace(WebhookURL))
			{
				throw new InvalidOperationException("A valid webhook address is required. See Integrations in Discord Server Settings.");
			}

			Values.Remove("content");
			Values.Add("content", msgSend);

			WebClient.UploadValues(WebhookURL, Values);
		}
	}

}
