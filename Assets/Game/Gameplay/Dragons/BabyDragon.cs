// BabyDragon.cs
// Hungry Dragon
//
// Created by Jordi Riambau on 27/05/2020.
// Copyright (c) 2015 Ubisoft. All rights reserved.

using System.Collections.Generic;

public class BabyDragon 
{
    public List<string> sku = new List<string>();
    public int probability;
    public int extraGems;
    public int firstSucceed;

    public bool IsEquipped()
    {
        return sku.Count > 0;
    }

    public void Reset()
    {
        sku.Clear();
        probability = 0;
        extraGems = 0;
        firstSucceed = 0;
    }
}
