#import <Foundation/Foundation.h>
#import "FGOLWeibo.h"


#pragma mark -- Globals & Debug mode --

static NSString* kAppKey = @"";
static NSString* kAppSecret = @"";
static NSString* kUnityListenerName = @"";
static NSString* kRedirectUrl = @"";
static bool kInitialized = false;

FGOLWeibo* kWeibo = NULL;

static const BOOL kEnableDebugMode = NO;

#pragma mark -- AppDelegateListener --

@implementation FGOLWeiboAppDelegateListener

- (void)onOpenURL:(NSNotification*)notification
{
    NSDictionary* data = notification.userInfo;
    NSURL* urlString = [data objectForKey:@"url"];
    
    if (urlString != nil)
    {
        NSLog(@"FGOLWeiboAppDelegateListner recieved: %@", urlString);
        [WeiboSDK handleOpenURL:urlString delegate:kWeibo];
    }
    else
    {
        NSLog(@"FGOLWeiboAppDelegateListner something went wrong with callback URL");
    }
}

@end

static FGOLWeiboAppDelegateListener* kWeiboAppDelegate = nil;

#pragma mark -- Native bindings --

const char* NewWeiboString(NSString* str)
{
    char* cstring = (char*) malloc([str length] + 1);
    strcpy(cstring, [str UTF8String]);
    return cstring;
}

void _InitWeibo(char* appKey, char* appSecret,  char* redirectURL, char* unityListenerName)
{
    //  Setup keys
    NSLog(@"_InitWeibo");
    kAppKey = [NSString stringWithUTF8String:appKey];
    kAppSecret = [NSString stringWithUTF8String:appSecret];
    kUnityListenerName = [NSString stringWithUTF8String:unityListenerName];
	kRedirectUrl = [NSString stringWithUTF8String:redirectURL];
    
    //  Init delegate
    kWeibo = [[FGOLWeibo alloc] init];
    if (kWeibo == NULL)
    {
        NSLog(@"WeiboDelegate is null");
    }
    
    //  Init Unity Listener
    kWeiboAppDelegate = [[FGOLWeiboAppDelegateListener alloc] init];
    UnityRegisterAppDelegateListener(kWeiboAppDelegate);
    
    //  Init SDK
    [WeiboSDK enableDebugMode:kEnableDebugMode];
    [WeiboSDK registerApp: kAppKey];
    kInitialized = true;
}

void _WeiboLogin()
{
    NSLog(@"_WeiboLogin");
    WBAuthorizeRequest *request = [WBAuthorizeRequest request];
    request.redirectURI = kRedirectUrl;
    request.scope = @"all";
    request.userInfo = @{@"SSO_From": @"Hungry Shark"};
    [WeiboSDK sendRequest: request];
}

bool _IsWeiboInitialised()
{
    return kInitialized;
}

#pragma mark -- FGOLWeibo implementation --

@implementation FGOLWeibo
- (void)didReceiveWeiboResponse:(WBBaseResponse *)response
{
    NSLog(@"FGOLWeiboResponseReceived");
    if ([response isKindOfClass:WBAuthorizeResponse.class])
    {
        if ([(WBAuthorizeResponse *)response accessToken] == NULL)
        {
            NSString* error = [NSString stringWithFormat:@"Failed : %d", (int)response.statusCode];
            UnitySendMessage([kUnityListenerName UTF8String], "OnLoginCompleteFailed", [error UTF8String]);
        }
        else
        {
            NSString* kAccessToken = [[(WBAuthorizeResponse *)response accessToken] copy];
            NSString* kUserID = [[(WBAuthorizeResponse *)response userID] copy];
            NSDate* kExpiry = [[(WBAuthorizeResponse *)response expirationDate] copy];
            NSString* kRefreshToken = [[(WBAuthorizeResponse *)response refreshToken] copy];
            
            NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
            [formatter setDateFormat:@"yyyy-MM-dd HH:mm:ss zzz"];
            NSString* kExpiryString = [formatter stringFromDate:kExpiry];
            
            NSMutableDictionary *dict = [[NSMutableDictionary alloc]init];
            [dict setValue:kAccessToken forKey:@"token"];
            [dict setValue:kExpiryString forKey:@"expiry"];
            [dict setValue:kRefreshToken forKey:@"refreshToken"];
            [dict setValue:kUserID forKey:@"uid"];
            
            NSError *err;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:0 error:&err];
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            UnitySendMessage([kUnityListenerName UTF8String], "OnLoginCompleteSuccess", [jsonString UTF8String]);
        }
    }
}

- (void)didReceiveWeiboRequest:(WBBaseRequest *)request
{
    //not sure we need this
}
@end