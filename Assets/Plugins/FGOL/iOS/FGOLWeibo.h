//
//  FGOLWeibo.h
//  Unity-iPhone
//
//  Created by Peter Pearson on 25/01/2016.
//
//

#ifndef FGOLWeibo_h
#define FGOLWeibo_h

#include "WeiboSDK.h"
#import "AppDelegateListener.h"

//  Interface to support resolving Weibo responses
@interface FGOLWeibo : NSObject<WeiboSDKDelegate>
-(void) didReceiveWeiboRequest:(WBBaseRequest *)request;
-(void) didReceiveWeiboResponse:(WBBaseResponse *)response;
@end

//  Interface to listen to OnOpenURL event from Weibo
@interface FGOLWeiboAppDelegateListener : NSObject<AppDelegateListener>
- (void) onOpenURL: (NSNotification*) notification;
@end

//  Native binding code (calls from Unity)
extern "C"
{
    void _InitWeibo(char* appKey, char* appSecret,  char* redirectURL, char* unityListenerName);
    void _WeiboLogin();
    bool _IsWeiboInitialised();
}

#endif /* FGOLWeibo_h */
