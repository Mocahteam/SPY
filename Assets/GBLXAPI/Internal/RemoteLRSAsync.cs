// -------------------------------------------------------------------------------------------------
// RemoteLRSAsync.cs
// Project: GBLXAPI
// Created: 2017/05/30
// Copyright 2017 Dig-It! Games, LLC. All rights reserved.
// This code is licensed under the MIT License (see LICENSE.txt for details)
//
// NOTE:
// This is a slim async version of RemoteLRS.cs for WebGL that only saves statements
// There is a desktop/mobile version that uses threading and the full RemoteLRS to make async.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using TinCan;
using TinCan.LRSResponses;
using UnityEngine;
using UnityEngine.Networking;

namespace DIG.GBLXAPI.Internal
{
    public class RemoteLRSAsync
	{
		// config
		public TCAPIVersion version { get; set; }
		private readonly LrsConfig m_Config;

		public string xApiEndpoint { get { return m_Config.GetEndpoint(); } }


		public RemoteLRSAsync(LrsConfig config)
        {
			m_Config = config;
			this.version = TCAPIVersion.latest();
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public async Awaitable<StatementLRSResponse> PostStatements(List<Statement> statements)
		{
			// https://learninglocker.dig-itgames.com/data/xAPI/statements?statementId=58098b7c-3353-4f9c-b812-1bddb08876fd
			string queryURL = this.xApiEndpoint + "/statements";
			AuthenticationHeaderValue auth = await m_Config.GetAuthHeader();

            string jsonData = "";
            if (statements.Count > 1)
                jsonData += "[";
            for (int i = 0; i < statements.Count; i++) {
                jsonData += statements[i].ToJSON(version);
                if (i < statements.Count - 1)
                    jsonData += ", ";
            }
            if (statements.Count > 1)
                jsonData += "]";
			if (jsonData == "")
			{
                return new StatementLRSResponse()
                {
                    success = true
                };
			}
			
			byte[] formBytes = Encoding.UTF8.GetBytes(jsonData);

			using (UnityWebRequest request = new UnityWebRequest(queryURL, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(formBytes)))
			{
				request.SetRequestHeader("Content-Type", "application/json");
				request.SetRequestHeader("X-Experience-API-Version", version.ToString());
				request.SetRequestHeader("Authorization", auth.ToString());

				await request.SendWebRequest();
				if (request.result != UnityWebRequest.Result.Success)
				{
					return new StatementLRSResponse()
					{
						success = false,
						errMsg = request.error
					};
				}
				JArray ids = JArray.Parse(request.downloadHandler.text);
				return new StatementLRSResponse() {success = true };
			}

        }
	}
}
