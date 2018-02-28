using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Cmn {

    public static string Nik;

    public enum EPlayerColor :int
        { Blue, Red, Gray, Yellow, Green }
    public static EPlayerColor PlayerColor;
    
    public static string ToHex (this EPlayerColor pc)
    {
        switch (pc)
        {
            case EPlayerColor.Blue: return "#AEDDE8";
            case EPlayerColor.Red: return "#E07B7B";
            case EPlayerColor.Gray: return "#BFBFBF";
            case EPlayerColor.Yellow: return "#EDED61";
            case EPlayerColor.Green: return "#61ED74";
        }
        return "";
    }
    public static Color ToColor (this EPlayerColor pc)
    {
        switch (pc)
        {
            case EPlayerColor.Blue: return new Color(0.68f, 0.87f, 0.9f);
            case EPlayerColor.Red: return new Color(0.93f, 0.345f, 0.46f);
            case EPlayerColor.Gray: return new Color(.7f, .7f, .7f);
            case EPlayerColor.Yellow: return new Color(.91f, .93f, .345f);
            case EPlayerColor.Green: return new Color(.345f, .93f, .63f);
        }
        return Color.white;
    }
    
    public static BinaryFormatter binaryFormatter = new BinaryFormatter();

    public static byte[] SerializeTurn (Turn turn)
    {
        if (turn == null) return null;

        using (var ms = new MemoryStream())
        {
            binaryFormatter.Serialize(ms, turn);
            return ms.ToArray();
        }
    }
    public static Turn DeserializeTurn (byte[] barray) 
    {
        if (barray == null) return null;

        using(var ms = new MemoryStream(barray))
        {
            return binaryFormatter.Deserialize(ms) as Turn;
        }

        return null;
    }

    /// <summary>
    /// Awaits for some seconds and execute(s?) next()
    /// </summary>
    /// <param name="next">Action that will been executed </param>
    /// <param name="awaitFor">Time to await</param>
    public static IEnumerator AwaitAnd(Action next, float awaitFor = .2f)
    {
        yield return new WaitForSeconds(awaitFor);
        next();
    }

}
