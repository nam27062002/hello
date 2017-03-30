

#import "PlatformUtils.h"
#import <GameKit/GameKit.h>
#import <AdSupport/ASIdentifierManager.h>


#include <string>
#include <inttypes.h>
#import <mach/mach.h>

using namespace std;

@implementation PlatformUtils

extern "C"
{
    char* stringCopy(const char* string)
    {
        if (string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        
        return res;
    }
    
    const char* IOsGetCountryCode()
    {
        NSLocale *currentLocale = [NSLocale currentLocale];  // get the current locale.
        return stringCopy( ((NSString*)[currentLocale objectForKey:NSLocaleCountryCode]).UTF8String );
    }
    
    // https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/Account+merging
    const void generateIdentityVerificationSignature()
    {
        GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
        [localPlayer generateIdentityVerificationSignatureWithCompletionHandler:^(NSURL *publicKeyUrl, NSData *signature, NSData *salt, uint64_t timestamp, NSError *error)
         {
             const char* objectName = "Singleton - InstanceMng";
             if(error != nil)
             {
                 UnitySendMessage( objectName, "GCTokensRecieved", "false");
                 // Callback succed false
                 return; //some sort of error, can't authenticate right now
             }
             
             // Add public Key Url
             const char* cPublicKeyUrl = [publicKeyUrl absoluteString].UTF8String;
             UnitySendMessage( objectName, "SetGCPublicKeyUrl", cPublicKeyUrl);
             
             // Add signature
             NSString* str_signature = [signature base64EncodedStringWithOptions:0];
             const char* cSignature = str_signature.UTF8String;
             UnitySendMessage( objectName, "SetGCSignature", cSignature);
             
             // Add salt
             NSString* str_salt = [salt base64EncodedStringWithOptions:0];
             const char* cSalt = str_salt.UTF8String;
             UnitySendMessage( objectName, "SetGCSalt", cSalt);
             
             // Add timestamp
             char buffer[256];
             sprintf(buffer, "%" PRIu64, timestamp);
             UnitySendMessage( objectName, "SetGCTimestamp", buffer);
             
             // Callback
             UnitySendMessage( objectName, "GCTokensRecieved", "true");
             
         }];
    }
    
    const char* IOsGetTrackingId()
    {
        // TODO (miguel)
        // NSUUID *IDFA = [[ASIdentifierManager sharedManager] advertisingIdentifier];
        // NSString *idfaString = [IDFA UUIDString];
        // return (idfaString == nil) ? stringCopy( "" ) : stringCopy( idfaString.UTF8String );
        return stringCopy( "" );
    }
    
    const char* IOsFormatPrice( float price, const char* currencyLocale )
    {
        NSNumberFormatter *_currencyFormatter = [[NSNumberFormatter alloc] init];
        [_currencyFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
        [_currencyFormatter setCurrencyCode: [NSString stringWithUTF8String: currencyLocale] ];
        NSString* priceStr = [_currencyFormatter stringFromNumber:@(price)];
        
        return (priceStr == nil) ? stringCopy( "" ) : stringCopy( priceStr.UTF8String );
    }
    
    const char* NetworkClient_GetDefaultProxyURL()
    {
        CFDictionaryRef dicRef = CFNetworkCopySystemProxySettings();
        
        const CFStringRef proxyCFstr = (const CFStringRef)CFDictionaryGetValue(dicRef, (const void*)kCFNetworkProxiesHTTPProxy);
        
        if (proxyCFstr)
        {
            char buffer[4096];
            memset(buffer, 0, 4096);
            
            if (CFStringGetCString(proxyCFstr, buffer, 4096, kCFStringEncodingUTF8))
            {
                return stringCopy(std::string(buffer).c_str());
            }
        }
        return stringCopy("");
    }
    
    int NetworkClient_GetDefaultProxyPort()
    {
        CFDictionaryRef dicRef = CFNetworkCopySystemProxySettings();
        
        const CFNumberRef portCFnum = (const CFNumberRef)CFDictionaryGetValue(dicRef, (const void*)kCFNetworkProxiesHTTPPort);
        
        if (portCFnum)
        {
            SInt32 port;
            if (CFNumberGetValue(portCFnum, kCFNumberSInt32Type, &port))
            {
                return port;
            }
        }
        return -1;
    }
    
    long IOsGetFreeSpaceAvailable( const char* path)
    {
        NSDictionary *atDict = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[NSString stringWithUTF8String: path] error:NULL];
        long freeSpace = [[atDict objectForKey:NSFileSystemFreeSize] longValue];
        NSLog(@"Free Diskspace: %ld bytes - %f MiB", freeSpace, (freeSpace/1024.0)/1024.0);
        
        return freeSpace;
    }
    
    void IOsReportAchievement( const char* achievementId, double progress)
    {
        GKAchievement *achievement = [[GKAchievement alloc] initWithIdentifier: [NSString stringWithUTF8String: achievementId]];
        if (achievement)
        {
            achievement.percentComplete = progress;
            achievement.showsCompletionBanner = YES;
            [GKAchievement reportAchievements:@[achievement] withCompletionHandler:^(NSError *error)
             {
                 if (error != nil)
                 {
                     NSLog(@"Error in reporting achievements: %@", error);
                 }
             }];
        }
    }
    
    
    
    unsigned long IOsFreeMemory()
    {
        mach_port_t host_port = mach_host_self();
        mach_msg_type_number_t host_size = sizeof(vm_statistics_data_t) / sizeof(integer_t);
        vm_size_t pagesize;
        vm_statistics_data_t vm_stat;
        
        host_page_size(host_port, &pagesize);
        (void) host_statistics(host_port, HOST_VM_INFO, (host_info_t)&vm_stat, &host_size);
        return vm_stat.free_count * pagesize;
    }
    
    char* IOsGetCommandLineArgs()
    {
        NSArray<NSString *>* arguments = [[NSProcessInfo processInfo] arguments];
        std::string ret = "";
        for (NSString* element in arguments)
        {
            ret += std::string(element.UTF8String) + "#";
        }
        return stringCopy( ret.c_str() );
    }
}


@end
