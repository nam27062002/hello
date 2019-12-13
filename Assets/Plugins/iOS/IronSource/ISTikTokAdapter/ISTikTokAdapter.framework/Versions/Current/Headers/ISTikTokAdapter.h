//
//  ISTikTokAdapter.h
//  ISTikTokAdapter
//
//  Created by Guy Lis on 20/05/2019.
//

#import <Foundation/Foundation.h>
#import "IronSource/ISBaseAdapter+Internal.h"

@import StoreKit;
@import MobileCoreServices;
@import WebKit;
@import MediaPlayer;
@import CoreMedia;
@import AVFoundation;
@import CoreLocation;
@import CoreTelephony;
@import SystemConfiguration;
@import Photos;
@import AdSupport;
@import CoreMotion;

static NSString * const TikTokAdapterVersion             = @"4.1.0";
static NSString *  GitHash = @"54c9a700";

@interface ISTikTokAdapter : ISBaseAdapter

@end
