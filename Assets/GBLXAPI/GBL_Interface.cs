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
        Application.isEditor ? new GBLConfig("https://lrsels.lip6.fr/data/xAPI", "e6efcf3eac5c03e121af621dae0df3a50c8733f0", "ffda037ebf1368a89e5b8b59d30a1b77beebc27e") : new GBLConfig("https://lrsels.lip6.fr/data/xAPI", "9fe9fa9a494f2b34b3cf355dcf20219d7be35b14", "b547a66817be9c2dbad2a5f583e704397c9db809")
    };
	public static string userUUID = ""; // Muratet : overrided in TitleScreenSystem system
    public static string playerName = ""; // Muratet : overrided in TitleScreenSystem system

    // ------------------------------------------------------------------------
    // Sample Gameplay GBLxAPI Triggers
    // ------------------------------------------------------------------------
    /*
	Here is where you will put functions to be called whenever you want to send a GBLxAPI statement.
	 */

    public static void SendStatement(string verb, string activityType, Dictionary<string, string> activityExtensions = null)
    {
        ActivityBuilder.IOptional activityBuilder = GBLXAPI.Activity
            .WithID(activityType)
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
            .WithID(activityType)
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