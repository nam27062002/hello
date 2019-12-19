//
//  Copyright (c) 2015 IronSource. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "IronSource/ISBaseAdapter+Internal.h"

static NSString * const AdColonyAdapterVersion = @"4.1.7";
static NSString * GitHash = @"66454182e";

//System Frameworks For AdColony Adapter

@import AdSupport;
@import AudioToolbox;
@import AVFoundation;
@import CoreMedia;
@import CoreTelephony;
@import EventKit;
@import JavaScriptCore;
@import MessageUI;
@import MobileCoreServices;
@import Social;
@import StoreKit;
@import SystemConfiguration;
@import WatchConnectivity;
@import WebKit;

@interface ISAdColonyAdapter : ISBaseAdapter

@end
