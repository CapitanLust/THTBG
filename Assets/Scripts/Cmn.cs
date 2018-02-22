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
            case EPlayerColor.Blue: return new Color(174, 221, 232);
            case EPlayerColor.Red: return new Color(224, 123, 123);
            case EPlayerColor.Gray: return new Color(191, 191, 191);
            case EPlayerColor.Yellow: return new Color(237, 237, 97);
            case EPlayerColor.Green: return new Color(97, 237, 116);
        }
        return Color.white;
    }
    
    public static BinaryFormatter binaryFormatter = new BinaryFormatter();

    public static byte[] SerializeBatch (Batch batch)
    {
        if (batch == null) return null;

        using (var ms = new MemoryStream())
        {
            binaryFormatter.Serialize(ms, batch);
            return ms.ToArray();
        }
    }
    public static Batch DeserializeBatch (byte[] barray) 
    {
        if (barray == null) return null;

        using(var ms = new MemoryStream(barray))
        {
            return binaryFormatter.Deserialize(ms) as Batch;
        }

        return null;
    }
    

}
