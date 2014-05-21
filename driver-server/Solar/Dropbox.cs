using System;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using RestSharp;

namespace Solar
{
	public class Dropbox
	{
		const string ConsumerKey = "yd7c6qd3cl323p8";
		const string ConsumerSecret = "ptbp0vjz6721wto";
		string AccessToken = "_PZKmyWNtlEAAAAAAAADxK_Ew8VHZB56IULj-fxFbbJQpd__lnvhSMDyPsaPJi_c";
		RestClient api_client = new RestClient("https://api.dropbox.com");
		RestClient content_client = new RestClient("https://api-content.dropbox.com");

		public Dropbox()
		{
			if (string.IsNullOrEmpty(this.AccessToken))
			{
				string auth_code = this.OAuth2Authorize(); // "_PZKmyWNtlEAAAAAAAADw2AB7pDfAWXcFLEuAQflY3I";
				this.AccessToken = this.OAuth2Token(auth_code).Result;
			}
		}

		public string OAuth2Authorize()
		{
			// get authorization code
			string url = "https://www.dropbox.com/1/oauth2/authorize?response_type=code&client_id=" + ConsumerKey;
			System.Diagnostics.Process.Start(url);
			Console.WriteLine("DROPBOX:\tType authorization code: ");
			return Console.ReadLine();
		}

		public async Task<string> OAuth2Token(string auth_code)
		{
			// get access token
			RestRequest token_request = new RestRequest("1/oauth2/token", Method.POST);
			token_request.AddParameter("code", auth_code);
			token_request.AddParameter("grant_type", "authorization_code");
			token_request.AddParameter("client_id", ConsumerKey);
			token_request.AddParameter("client_secret", ConsumerSecret);
			// TODO remove these workarounds. RestSharp's ExecuteTaskAsync<> is currently bugged 
			IRestResponse<TokenResponse> token_response = await Task.Run(() => api_client.Execute<TokenResponse>(token_request));
			Debug.WriteLine("DROPBOX:\tOAuth2Token: " + token_response.Content);
			return token_response.Data.AccessToken;
		}

		public async Task<AccountInfoResponse> AccountInfo()
		{
			// check account
			RestRequest account_request = new RestRequest("1/account/info", Method.GET);
			account_request.AddHeader("Authorization", string.Format("Bearer {0}", this.AccessToken));
			// TODO remove these workarounds. RestSharp's ExecuteTaskAsync<> is currently bugged 
			IRestResponse<AccountInfoResponse> account_response = await Task.Run(() => api_client.Execute<AccountInfoResponse>(account_request));
			Debug.WriteLine("DROPBOX:\tAccountInfo: " + account_response.Content);
			return account_response.Data;
		}

		public async Task<MetadataResponse> Metadata(string path, bool list = true)
		{
			// check file
			RestRequest metadata_request = new RestRequest("1/metadata/{root}/{path}", Method.GET);
			metadata_request.AddHeader("Authorization", string.Format("Bearer {0}", this.AccessToken));
			metadata_request.AddUrlSegment("root", "sandbox");
			metadata_request.AddUrlSegment("path", path);
			metadata_request.AddParameter("list", list ? "true" : "false");
			// TODO remove these workarounds. RestSharp's ExecuteTaskAsync<> is currently bugged 
			IRestResponse<MetadataResponse> metadata_response = await Task.Run(() => api_client.Execute<MetadataResponse>(metadata_request));
			Debug.WriteLine("DROPBOX:\tMetadata: " + metadata_response.Content);
			return metadata_response.Data;
		}

		public async Task<MetadataResponse> FilesPut(string source_path, string dest_path, string parent_rev = null)
		{
			byte[] fileData = null;
			using (System.IO.FileStream source_stream = System.IO.File.OpenRead(source_path))
			{
				fileData = new byte[source_stream.Length];
				await source_stream.ReadAsync(fileData, 0, (int)source_stream.Length);
				source_stream.Close();
			}
			return await this.FilesPut(fileData, dest_path, parent_rev);
		}

		public async Task<MetadataResponse> FilesPut(byte[] fileData, string dest_path, string parent_rev = null)
		{
			// upload file
			RestRequest file_put_request = new RestRequest("1/files_put/{root}/{path}", Method.PUT);
			file_put_request.AddHeader("Authorization", string.Format("Bearer {0}", this.AccessToken));
			file_put_request.AddUrlSegment("root", "sandbox");
			file_put_request.AddUrlSegment("path", dest_path);
			if (!string.IsNullOrEmpty(parent_rev))
				file_put_request.AddParameter("parent_rev", parent_rev, ParameterType.GetOrPost);
			//file_put_request.AddFile("FileData", source_path);
			file_put_request.AddParameter("file", fileData, ParameterType.RequestBody);
			// TODO remove these workarounds. RestSharp's ExecuteTaskAsync<> is currently bugged 
			IRestResponse<MetadataResponse> file_put_response = await Task.Run(() => content_client.Execute<MetadataResponse>(file_put_request));
			Debug.WriteLine("DROPBOX:\tFilesPut: " + file_put_response.Content);
			return file_put_response.Data;
		}

		public class TokenResponse
		{
			public string AccessToken { get; set; }

			public string TokenType { get; set; }

			public string Uid { get; set; }
		}

		public class AccountInfoResponse
		{
			public string ReferralLink { get; set; }

			public string DisplayName { get; set; }

			public string Uid { get; set; }

			public string Country { get; set; }
			// team: {name: ""}
			// quota_info: {normal: 0, shared: 1, quota: 2}
		}

		public class MetadataResponse
		{
			public string Size { get; set; }

			public int Bytes { get; set; }

			public string Path { get; set; }

			public bool IsDir { get; set; }

			public string Rev { get; set; }

			public string Modified { get; set; }

			public string Root { get; set; }

			public string Icon { get; set; }

			public bool ThumbExists { get; set; }

			/// File only
			public string ClientMtime { get; set; }

			/// Folder only
			public string Hash { get; set; }
			// is_deleted
		}
	}
}

