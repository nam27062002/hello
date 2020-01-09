//
//  ISTikTokAdapter.h
//  ISTikTokAdapter
//
//  Created by Guy Lis on 20/05/2019.
//  Copyright © 2019 IronSource. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "IronSource/ISBaseAdapter+Internal.h"

static NSString * const TikTokAdapterVersion = @"4.1.2";
static NSString * GitHash = @"66454182e";

//System Frameworks For TikTok Adapter

@import AdSupport;
@import AVFoundation;
@import CoreLocation;
@import CoreMedia;
@import CoreMotion;
@import CoreTelephony;
@import MediaPlayer;
@import MobileCoreServices;
@import StoreKit;
@import SystemConfiguration;
@import WebKit;

@interface ISTikTokAdapter : ISBaseAdapter

@end
