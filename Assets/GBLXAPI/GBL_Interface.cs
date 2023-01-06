// -------------------------------------------------------------------------------------------------
// GBL_Interface.cs
// Project: GBLXAPI-Unity
// Created: 2018/07/06
// Copyright 2018 Dig-It! Games, LLC. All rights reserved.
// This code is licensed under the MIT License (see LICENSE.txt for details)
// -------------------------------------------------------------------------------------------------

// required for GBLXAPI
using DIG.GBLXAPI;
using System.Collections.Generic;

using DIG.GBLXAPI.Builders;
using UnityEngine;

// --------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------
public static class GBL_Interface {

	public enum durationSlots
	{
		Application,
		Game,
		Tutorial,
		Level
	};

    // Fill in these fields for GBLxAPI setup.
    //Statements will be sent to all addresses in this list
    public static List<GBLConfig> lrsAddresses = new List<GBLConfig>() {
        // Root statement to Dev store in editor and Prod store else
        Application.isEditor ? new GBLConfig("https://lrsmocah.lip6.fr/data/xAPI", "a4b46f0307a8671674fd4f55139ae5bebb9a4a4d", "cb1992c04168586866dd55f51a0d1f6589e45335") : new GBLConfig("https://lrsmocah.lip6.fr/data/xAPI", "dc2cfee1883f369feb56c964c219f50555d00507", "213c1323b76f6d53fdcd979df168543a3c57d961")
    };
	public static string userUUID = ""; // Muratet : overrided in SendStatements system
    public static string playerName = ""; // Muratet : overrided in SendStatements system

    // ------------------------------------------------------------------------
	// Sample Gameplay GBLxAPI Triggers
	// ------------------------------------------------------------------------
	/*
	Here is where you will put functions to be called whenever you want to send a GBLxAPI statement.
	 */
	
	public static void SendStatement(string verb, string activityType, Dictionary<string, string> activityExtensions = null)
    {
        ActivityBuilder.IOptional activityBuilder = GBLXAPI.Activity
            .WithID("https://www.lip6.fr/mocah/ELS/" + activityType)
            .WithType(activityType);
        if (activityExtensions != null)
        {
            //set extensions
            ExtensionsBuilder extensions = GBLXAPI.Extensions;
            foreach (KeyValuePair<string, string> entry in activityExtensions)
                extensions = extensions.WithStandard(entry.Key, entry.Value);
            activityBuilder.WithExtensions(extensions.Build());
        }

        GBLXAPI.Statement
            .WithActor(GBLXAPI.Agent
                .WithAccount(userUUID, "https://www.lip6.fr/mocah/")
                .WithName(playerName)
                .Build())
            .WithVerb(verb)
            .WithTargetActivity(activityBuilder.Build())
            .Enqueue();
        ;
    }
	
	public static void SendStatementWithResult(string verb, string activityType, Dictionary<string, string> activityExtensions = null, Dictionary<string, string> resultExtensions = null, bool? completed = null, bool? success = null, string response = null, int? score = null,
        float duration = 0)
    {
        ActivityBuilder.IOptional activityBuilder = GBLXAPI.Activity
            .WithID("https://www.lip6.fr/mocah/ELS/" + activityType)
            .WithType(activityType);
        if (activityExtensions != null)
        {
            //set extensions
            ExtensionsBuilder extensions = GBLXAPI.Extensions;
            foreach (KeyValuePair<string, string> entry in activityExtensions)
                extensions = extensions.WithStandard(entry.Key, entry.Value);
            activityBuilder.WithExtensions(extensions.Build());
        }

        ResultBuilder resultBuilder = GBLXAPI.Result;
        if (completed != null)resultBuilder = resultBuilder.WithCompletion(completed == true);
        if (success != null) resultBuilder = resultBuilder.WithSuccess(success == true);
        if (score != null) resultBuilder = resultBuilder.WithScore(score);
        if (response != null) resultBuilder = resultBuilder.WithResponse(response);
        if (resultExtensions != null)
        {
            //set extensions
            ExtensionsBuilder extensions = GBLXAPI.Extensions;
            foreach (KeyValuePair<string, string> entry in resultExtensions)
                extensions = extensions.WithStandard(entry.Key, entry.Value);
            resultBuilder.WithExtensions(extensions.Build());
        }

        GBLXAPI.Statement
            .WithActor(GBLXAPI.Agent
                .WithAccount(userUUID, "https://www.lip6.fr/mocah/")
                .WithName(playerName)
                .Build())
            .WithVerb(verb)
            .WithTargetActivity(activityBuilder.Build())
            .WithResult(resultBuilder)
            .Enqueue();
	}
}