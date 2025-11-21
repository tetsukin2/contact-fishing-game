using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameplayConfigExtensions
{
    public static Dictionary<string, object> ToFirestoreData(this GameplayConfig config)
    {
        return new Dictionary<string, object>
        {
            { "RotateUpAngle", config.RotateUpAngle },
            { "RotateDownAngle", config.RotateDownAngle },
            { "RollRightAngle", config.RollRightAngle },
            { "RollLeftAngle", config.RollLeftAngle },
            { "SideRotateUpAngle", config.SideRotateUpAngle },
            { "SideRotateDownAngle", config.SideRotateDownAngle },
            { "BobberSensitivity", config.BobberSensitivity },
            { "BaitPreparationSteps", config.BaitPreparationSteps },
            { "ReelTotalProgress", config.ReelTotalProgress },
            { "ReelProgressAmount", config.ReelProgressAmount },
            { "ReelDecayRate", config.ReelDecayRate },
            { "BraillePatternInterval", config.BraillePatternInterval },
            { "FishTotalToCatch", config.FishTotalToCatch },
            { "DiscoveredFish", config.DiscoveredFish },
            // Optionally add ReelActionSequence here if needed
        };
    }
}

public static class GameplayConfigLoader
{
    public static void ApplyFromFirestore(GameplayConfig config, Dictionary<string, object> firestoreData)
    {
        if (firestoreData.TryGetValue("rotateUpAngle", out var up)) config.RotateUpAngle = float.Parse(up.ToString());
        if (firestoreData.TryGetValue("rotateDownAngle", out var down)) config.RotateDownAngle = float.Parse(down.ToString());
        if (firestoreData.TryGetValue("rollRightAngle", out var rr)) config.RollRightAngle = float.Parse(rr.ToString());
        if (firestoreData.TryGetValue("rollLeftAngle", out var rl)) config.RollLeftAngle = float.Parse(rl.ToString());
        if (firestoreData.TryGetValue("sideRotateUpAngle", out var sru)) config.SideRotateUpAngle = float.Parse(sru.ToString());
        if (firestoreData.TryGetValue("sideRotateDownAngle", out var srd)) config.SideRotateDownAngle = float.Parse(srd.ToString());
        if (firestoreData.TryGetValue("bobberSensitivity", out var bs)) config.BobberSensitivity = int.Parse(bs.ToString());
        if (firestoreData.TryGetValue("baitPreparationSteps", out var bps)) config.BaitPreparationSteps = int.Parse(bps.ToString());
        if (firestoreData.TryGetValue("reelTotalProgress", out var rtp)) config.ReelTotalProgress = int.Parse(rtp.ToString());
        if (firestoreData.TryGetValue("reelProgressAmount", out var rpa)) config.ReelProgressAmount = int.Parse(rpa.ToString());
        if (firestoreData.TryGetValue("reelDecayRate", out var rdd)) config.ReelDecayRate = float.Parse(rdd.ToString());
        if (firestoreData.TryGetValue("braillePatternInterval", out var bpi)) config.BraillePatternInterval = float.Parse(bpi.ToString());
         if (firestoreData.TryGetValue("fishTotalToCatch", out var ftc)) 
            config.FishTotalToCatch = int.Parse(ftc.ToString());
        if (firestoreData.TryGetValue("discoveredFish", out var dfList) && dfList is List<object> dfObjects)
            config.DiscoveredFish = dfObjects.ConvertAll(obj => obj.ToString());
        if (firestoreData.TryGetValue("reelActionSequence", out var rasObj) && rasObj is List<object> rasList)
        {
            config.ReelActionSequence = new List<ReelingState.ReelActionName>();
            foreach (var val in rasList)
            {
                var str = val.ToString();
                if (!string.IsNullOrEmpty(str) && Enum.TryParse(str, out ReelingState.ReelActionName action))
                {
                    config.ReelActionSequence.Add(action);
                }
            }
        }
    }
}
