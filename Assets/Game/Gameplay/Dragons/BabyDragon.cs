// BabyDragon.cs
// Hungry Dragon
//
// Created by Jordi Riambau on 27/05/2020.
// Copyright (c) 2015 Ubisoft. All rights reserved.

public class BabyDragon 
{
    public string sku;
    public int probability;
    public int extraGems;
    public int firstSucceed;

    public bool IsEquipped()
    {
        return !string.IsNullOrEmpty(sku);
    }
}
