﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class IronSourceImpressionData
{

    public readonly string auctionId;
    public readonly string adUnit;
    public readonly string country;
    public readonly string ab;
    public readonly string segmentName;
    public readonly string placement;
    public readonly string adNetwork;
    public readonly string instanceName;
    public readonly string instanceId;
    public readonly double? revenue;
    public readonly string precision;
    public readonly double? lifetimeRevenue;
    public readonly string encryptedCPM;
    public readonly string allData;


    public IronSourceImpressionData(string json)
    {
        if (json != null)
        {
            try
            {
                object obj;
                double parsedDouble;
                allData = json;
                Dictionary<string, object> jsonDic = IronSourceJSON.Json.Deserialize(json) as Dictionary<string, object>;
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_AUCTION_ID, out obj) && obj != null)
                {
                    auctionId = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_AD_UNIT, out obj) && obj != null)
                {
                    adUnit = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_COUNTRY, out obj) && obj != null)
                {
                    country = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_ABTEST, out obj) && obj != null)
                {
                    ab = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_SEGMENT_NAME, out obj) && obj != null)
                {
                    segmentName = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_PLACEMENT, out obj) && obj != null)
                {
                    placement = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_AD_NETWORK, out obj) && obj != null)
                {
                    adNetwork = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_INSTANCE_NAME, out obj) && obj != null)
                {
                    instanceName = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.INSTANCE_ID_KEY, out obj) && obj != null)
                {
                    instanceId = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_PRECISION, out obj) && obj != null)
                {
                    precision = obj.ToString();
                }
                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_ENCRYPTED_CPM, out obj) && obj != null)
                {
                    encryptedCPM = obj.ToString();
                }

                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_REVENUE, out obj) && obj != null && double.TryParse(obj.ToString(), out parsedDouble))
                {
                    revenue = parsedDouble;
                }

                if (jsonDic.TryGetValue(IronSourceConstants.IMPRESSION_DATA_KEY_LIFETIME_REVENUE, out obj) && obj != null && double.TryParse(obj.ToString(), out parsedDouble))
                {
                    lifetimeRevenue = parsedDouble;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("error parsing impression " + ex.ToString());
            }

        }
    }

    public override string ToString()
    {
        return "IronSourceImpressionData{" +
                "auctionId='" + auctionId + '\'' +
                ", adUnit='" + adUnit + '\'' +
                ", country='" + country + '\'' +
                ", ab='" + ab + '\'' +
                ", segmentName='" + segmentName + '\'' +
                ", placement='" + placement + '\'' +
                ", adNetwork='" + adNetwork + '\'' +
                ", instanceName='" + instanceName + '\'' +
                ", instanceId='" + instanceId + '\'' +
                ", revenue=" + revenue +
                ", precision='" + precision + '\'' +
                ", lifetimeRevenue=" + lifetimeRevenue +
                ", encryptedCPM='" + encryptedCPM + '\'' +
                '}';
    }
}