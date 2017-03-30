#import <MediaPlayer/MPMusicPlayerController.h>
#import <CoreTelephony/CTTelephonyNetworkInfo.h>
#import <CoreTelephony/CTCarrier.h>
#import <AdSupport/ASIdentifierManager.h>
#import "Reachability.h"
#import <CommonCrypto/CommonCrypto.h>

//#import "Appirater.h"

#if FALSE
#define FGOL_LOG(...) printf(__VA_ARGS__)
#else
#define FGOL_LOG(...)
#endif


// ------------------------------------------------------------------------------
// This function is required to return strings as Unity attempts to free() our const char* pointers returned from native methods(!)

const char* NewCString(NSString* str)
{
    char* cstring = malloc([str length] + 1);
    strcpy(cstring, [str UTF8String]);
    return cstring;
}

// ------------------------------------------------------------------------------
// Version info

const char* _GetBundleVersion()
{
    return NewCString([[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleShortVersionString"]);
}

const char* _GetUserLocation()
{
    CTTelephonyNetworkInfo *netInfo = [ [ [ CTTelephonyNetworkInfo alloc ] init ] autorelease ];
    CTCarrier *carrier = [ netInfo subscriberCellularProvider ];
    NSString *isoCode = [carrier isoCountryCode];
    
    if( !isoCode )
        isoCode = [ [ NSLocale preferredLanguages ] objectAtIndex:0 ]; // Fallback to device setting
    
    if( !isoCode )
        isoCode = @"XX";
    
    FGOL_LOG("Location is %s\n", [isoCode UTF8String]);
    
    return NewCString(isoCode);
}

const char* _GetUserCountryISO()
{
    CTTelephonyNetworkInfo *netInfo = [ [ [ CTTelephonyNetworkInfo alloc ] init ] autorelease ];
    CTCarrier *carrier = [ netInfo subscriberCellularProvider ];
    NSString *isoCode = [carrier isoCountryCode];
	
	// If we failed getting ISO code, try to get it from country locale
	if( !isoCode )
	{
		NSLocale *countryLocale = [NSLocale currentLocale];
		if(countryLocale)
		{
			isoCode = [countryLocale objectForKey:NSLocaleCountryCode];
		}
	}

    if( !isoCode )
        isoCode = @"XX";
    
    FGOL_LOG("Country ISO %s\n", [isoCode UTF8String]);
    
    return NewCString(isoCode);
}

const char* _GetLanguage()
{
    NSString * language = [[NSLocale preferredLanguages] objectAtIndex:0];
    if (language != nil)
    {
        return NewCString(language);
    }
    return NewCString(@"");
}

const Reachability* g_Reachability = nil;

const char* _GetConnectionType()
{
    NSString* output = @"";
    
    if (g_Reachability == nil)
    {
        g_Reachability = [Reachability reachabilityForInternetConnection];
        [g_Reachability startNotifier];
    }
    
    NetworkStatus status = [g_Reachability currentReachabilityStatus];
    switch (status) {
        case NotReachable:
            output = @"None";
            break;
        case ReachableViaWiFi:
            output = @"Wifi";
            break;
        case ReachableViaWWAN:
            output = @"Data";
            break;
        default:
            break;
    }
    
    return NewCString(output);
}

const char* _GetIDFA()
{
    //ASIdentifierManager is there?
    if (NSClassFromString(@"ASIdentifierManager"))
    {
        NSUUID *IDFA = [[ASIdentifierManager sharedManager] advertisingIdentifier];
        NSString *IDFAString = [IDFA UUIDString];
        return NewCString(IDFAString);
    }
    return NewCString(@"Unknown");
}


const bool _IsiOSTrackingEnabled()
{
    //ASIdentifierManager is there?
    if (NSClassFromString(@"ASIdentifierManager"))
    {
        return [[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled];
    }
    return false;
}

void _DontBackupDirectory(const char* str)
{
    NSURL* url = [NSURL fileURLWithPath:[NSString stringWithUTF8String:str]];
    [url setResourceValue: [NSNumber numberWithBool: YES] forKey: NSURLIsExcludedFromBackupKey error: Nil];
}


// ------------------------------------------------------------------------------
// iPod music

bool _IsIPodMusicPlaying()
{
    MPMusicPlayerController* lpMusicPlayer = [MPMusicPlayerController systemMusicPlayer];
    if (lpMusicPlayer != nil) {
        if (lpMusicPlayer.nowPlayingItem != nil) {
            return (lpMusicPlayer.playbackState == MPMusicPlaybackStatePlaying);
        }
    }
    return false;
}

// ------------------------------------------------------------------------------

static UIAlertView* g_alertView = nil;

@interface FGOLAlertViewDelegate : NSObject<UIAlertViewDelegate>
{
    int msg_id;
}
@end

@implementation FGOLAlertViewDelegate
-(FGOLAlertViewDelegate*) initialize:( int ) msgid {
    [super init];
    msg_id = msgid;
    
    return self;
}
-(void) alertView: ( UIAlertView *) alertView
clickedButtonAtIndex: ( NSInteger ) buttonIndex {
    FGOL_LOG("clickedButtonAtIndex: %d\n", buttonIndex);
    if(msg_id != -1)
    {
        if(buttonIndex == 0)
        {
            NSString *params = [NSString stringWithFormat:@"%d:%@",msg_id,@"CANCEL"];
            UnitySendMessage("FGOLNativeReceiver", "MessageBoxClick", [params UTF8String]);
        }
        else
        {
            NSString *params = [NSString stringWithFormat:@"%d:%@",msg_id,@"OK"];
            UnitySendMessage("FGOLNativeReceiver", "MessageBoxClick", [params UTF8String]);
        }
    }
    [g_alertView release], g_alertView=nil;
}
@end

void _ShowMessageBox(const char* title, const char* message, int msg_id)
{
    NSString* stitle = [[NSString alloc] initWithUTF8String:title];
    NSString* smessage = [[NSString alloc] initWithUTF8String:message];
    
    NSLog(@"_ShowMessageBox: %@ %@", stitle, smessage);
    
    g_alertView = [[UIAlertView alloc] initWithTitle:stitle message:smessage delegate:nil
                                   cancelButtonTitle:@"Ok" otherButtonTitles:nil];
    
    g_alertView.delegate = [[FGOLAlertViewDelegate alloc] initialize:msg_id];
    
    [g_alertView show];
    
    [stitle release];
    [smessage release];
}

void _ShowMessageBoxWithButtons(const char* title, const char* message, const char* ok_button, const char* cancel_button, int msg_id)
{
    NSString* stitle = [[NSString alloc] initWithUTF8String:title];
    NSString* smessage = [[NSString alloc] initWithUTF8String:message];
    NSString* sok = [[NSString alloc] initWithUTF8String:ok_button];
    NSString* scancel = [[NSString alloc] initWithUTF8String:cancel_button];
    
    NSLog(@"_ShowMessageBoxWithButtons: %@ %@", stitle, smessage);
    
    g_alertView = [[UIAlertView alloc] initWithTitle:stitle message:smessage delegate:nil
                                   cancelButtonTitle:scancel otherButtonTitles:sok, nil];
    
    g_alertView.delegate = [[FGOLAlertViewDelegate alloc] initialize:msg_id];
    
    [g_alertView show];
    
    [stitle release];
    [smessage release];
    [sok release];
    [scancel release];
}

void _ShowMessageBoxWithButtonsAndTextfield(const char* title, const char* message, const char* ok_button, const char* cancel_button, int msg_id)
{
    NSString* stitle = [[NSString alloc] initWithUTF8String:title];
    NSString* smessage = [[NSString alloc] initWithUTF8String:message];
    NSString* sok = [[NSString alloc] initWithUTF8String:ok_button];
    NSString* scancel = [[NSString alloc] initWithUTF8String:cancel_button];
    
    NSLog(@"_showMessageBoxWithButtonsAndTextfield: %@ %@", stitle, smessage);
    
    g_alertView = [[UIAlertView alloc] initWithTitle:stitle message:smessage delegate:nil
                                   cancelButtonTitle:scancel otherButtonTitles:sok, nil];
    [g_alertView setAlertViewStyle:UIAlertViewStylePlainTextInput];
    g_alertView.delegate = [[FGOLAlertViewDelegate alloc] initialize:msg_id];
    
    [g_alertView show];
    
    [stitle release];
    [smessage release];
    [sok release];
    [scancel release];
    
    
}

// ------------------------------------------------------------------------------

bool _CanOpenURL(const char* url)
{
    NSString* urlstring = [[NSString alloc] initWithUTF8String:url];
    NSURL* nsurl = [[NSURL alloc] initWithString:urlstring];
    
    NSLog(@"_CanOpenURL: %@", urlstring);
    bool canOpen = [[UIApplication sharedApplication] canOpenURL:nsurl];
    
    [urlstring release];
    [nsurl release];
    
    return canOpen;
}
// ------------------------------------------------------------------------------

// ------------------------------------------------------------------------------
// Unique device identifier

#include <sys/socket.h>
#include <sys/sysctl.h>
#include <net/if.h>
#include <net/if_dl.h>

NSString* getMacAddress()
{
    int                 mgmtInfoBase[6];
    char                *msgBuffer = NULL;
    size_t              length;
    unsigned char       macAddress[6];
    struct if_msghdr    *interfaceMsgStruct;
    struct sockaddr_dl  *socketStruct;
    NSString            *errorFlag = NULL;
    
    // Setup the management Information Base (mib)
    mgmtInfoBase[0] = CTL_NET;        // Request network subsystem
    mgmtInfoBase[1] = AF_ROUTE;       // Routing table info
    mgmtInfoBase[2] = 0;
    mgmtInfoBase[3] = AF_LINK;        // Request link layer information
    mgmtInfoBase[4] = NET_RT_IFLIST;  // Request all configured interfaces
    
    // With all configured interfaces requested, get handle index
    if ((mgmtInfoBase[5] = if_nametoindex("en0")) == 0)
        errorFlag = @"if_nametoindex failure";
    else
    {
        // Get the size of the data available (store in len)
        if (sysctl(mgmtInfoBase, 6, NULL, &length, NULL, 0) < 0)
            errorFlag = @"sysctl mgmtInfoBase failure";
        else
        {
            // Alloc memory based on above call
            if ((msgBuffer = malloc(length)) == NULL)
                errorFlag = @"buffer allocation failure";
            else
            {
                // Get system information, store in buffer
                if (sysctl(mgmtInfoBase, 6, msgBuffer, &length, NULL, 0) < 0)
                    errorFlag = @"sysctl msgBuffer failure";
            }
        }
    }
    
    // Befor going any further...
    if (errorFlag != NULL)
    {
        NSLog(@"Error: %@", errorFlag);
        return errorFlag;
    }
    
    // Map msgbuffer to interface message structure
    interfaceMsgStruct = (struct if_msghdr *) msgBuffer;
    
    // Map to link-level socket structure
    socketStruct = (struct sockaddr_dl *) (interfaceMsgStruct + 1);
    
    // Copy link layer address data in socket structure to an array
    memcpy(&macAddress, socketStruct->sdl_data + socketStruct->sdl_nlen, 6);
    
    // Read from char array into a string object, into traditional Mac address format
    NSString *macAddressString = [NSString stringWithFormat:@"%02X:%02X:%02X:%02X:%02X:%02X",
                                  macAddress[0], macAddress[1], macAddress[2],
                                  macAddress[3], macAddress[4], macAddress[5]];
    
    //NSLog(@"Mac Address: %@", macAddressString);
    
    // Release the buffer memory
    free(msgBuffer);
    
    return macAddressString;
}

const char* _GetUniqueDeviceIdentifier()
{
    return NewCString(getMacAddress());
}

const char* _GetDeviceName()
{
    size_t size;
    sysctlbyname("hw.machine", NULL, &size, NULL, 0);
    
    char *machine = malloc(size);
    sysctlbyname("hw.machine", machine, &size, NULL, 0);
    
    FGOL_LOG("Device machine: %s\n", machine);
    
    return machine;
}

/*
 bool _IsUniqueDeviceIdentifierOnKeyChain()
 {
 NSString* retrieveUUID =
 [SSKeychain passwordForService:@"com.fgol.HungrySharkEvolution" account:@"user"];
 
 return ( retrieveUUID != nil );
 }
 
 const char* _GetUniqueDeviceIdentifierFromKeyChain()
 {
 // Try get
 NSString* retrieveUUID =
 [SSKeychain passwordForService:@"com.fgol.HungrySharkEvolution" account:@"user"];
 
 if ( retrieveUUID != nil )
 {
 NSLog(@"Returning From Key Chain");
 return NewCString(retrieveUUID);
 }
 
 // Generate
 CFUUIDRef theUUID = CFUUIDCreate(NULL);
 //    CFStringRef string = CFUUIDCreateString(NULL, theUUID);
 CFStringRef string = CFStringCreateWithCString(NULL, _GetUniqueDeviceIdentifier(), kCFStringEncodingUTF8);
 CFRelease(theUUID);
 
 NSString* UUID = [(NSString *)string  autorelease];
 
 // Set
 [SSKeychain setPassword:UUID forService:@"com.fgol.HungrySharkEvolution" account:@"user"];
 
 NSLog(@"Returning Generated");
 return NewCString(UUID);
 }
 
 void _WriteUUIDToKeyChain( const char* _UUID )
 {
 NSString* UUID = [[NSString alloc] initWithUTF8String:_UUID];
 
 // Set
 [SSKeychain setPassword:UUID forService:@"com.fgol.HungrySharkEvolution" account:@"user"];
 }
 */

// ------------------------------------------------------------------------------
// Anti-Piracy measures

#import <dlfcn.h>
#import <mach-o/dyld.h>
#import <TargetConditionals.h>

/* The encryption info struct and constants are missing from the iPhoneSimulator SDK, but not from the iPhoneOS or
 * Mac OS X SDKs. Since one doesn't ever ship a Simulator binary, we'll just provide the definitions here. */
#if TARGET_IPHONE_SIMULATOR && !defined(LC_ENCRYPTION_INFO)
#define LC_ENCRYPTION_INFO 0x21
struct encryption_info_command {
    uint32_t cmd;
    uint32_t cmdsize;
    uint32_t cryptoff;
    uint32_t cryptsize;
    uint32_t cryptid;
};
#endif

int main(int argc, char *argv[]);

// should be equal to running this one the app exe:
// otool -l *exectuable file*
unsigned int _GetBuildEncryptionChecksum()
{
    const struct mach_header *header;
    Dl_info dlinfo;
    
    /* Fetch the dlinfo for main() */
    if (dladdr((const void*)main, &dlinfo) == 0 || dlinfo.dli_fbase == NULL) {
        // Can't find 'main' function, very odd!
        return 0;
    }
    header = (const struct mach_header*)dlinfo.dli_fbase;
    
    /* Compute the image size and search for a UUID */
    struct load_command *cmd = (struct load_command *) (header+1);
    
    for (uint32_t i = 0; cmd != NULL && i < header->ncmds; i++) {
        /* Encryption info segment */
        if (cmd->cmd == LC_ENCRYPTION_INFO) {
            struct encryption_info_command *crypt_cmd = (struct encryption_info_command *) cmd;
            /* Check if binary encryption is enabled */
            return crypt_cmd->cryptid; // probably pirated if crypt_cmd->cryptid < 10
        }
        
        cmd = (struct load_command *) ((uint8_t *) cmd + cmd->cmdsize);
    }
    
    /* Encryption info not found */
    return 0; // probably pirated
}

// ------------------------------------------------------------------------------
// General methods

#include "mach/mach.h"

// Get user memory
int _GetMemoryUsage()
{
    struct mach_task_basic_info info;
    mach_msg_type_number_t size = sizeof(info);
    kern_return_t kerr = task_info(mach_task_self(), TASK_BASIC_INFO, (task_info_t) &info, &size);
    
    if(kerr == KERN_SUCCESS)
        return (int) info.resident_size;
    else
        return -1;
}

// Get user memory
int _GetMaxMemoryUsage()
{
    struct mach_task_basic_info info;
    mach_msg_type_number_t size = sizeof(info);
    kern_return_t kerr = task_info(mach_task_self(), TASK_BASIC_INFO, (task_info_t) &info, &size);
    
    if(kerr == KERN_SUCCESS)
        return (int) info.resident_size_max;
    else
        return -1;
}


long _GetMaxDeviceMemory()
{
    long long total = [NSProcessInfo processInfo].physicalMemory;
    return (long) total;
}

// Custom method to calculate the SHA-256 hash using Common Crypto
NSString* hashedValueForAccountName(NSString* userAccountName)
{
    const int HASH_SIZE = 32;
    unsigned char hashedChars[HASH_SIZE];
    const char *accountName = [userAccountName UTF8String];
    size_t accountNameLen = strlen(accountName);
    
    // Confirm that the length of the user name is small enough
    // to be recast when calling the hash function.
    if (accountNameLen > UINT32_MAX) {
        NSLog(@"Account name too long to hash: %@", userAccountName);
        return nil;
    }
    CC_SHA256(accountName, (CC_LONG)accountNameLen, hashedChars);
    
    // Convert the array of bytes into a string showing its hex representation.
    NSMutableString *userAccountHash = [[NSMutableString alloc] init];
    for (int i = 0; i < HASH_SIZE; i++) {
        // Add a dash every four bytes, for readability.
        if (i != 0 && i%4 == 0) {
            [userAccountHash appendString:@"-"];
        }
        [userAccountHash appendFormat:@"%02x", hashedChars[i]];
    }
    
    return userAccountHash;
}


const char* _HashedValueForAccountName(const char* userID)
{
    if (userID != nil)
    {
        NSString* nsUserID = [NSString stringWithUTF8String:userID];
        NSString* output = hashedValueForAccountName(nsUserID);
        if (output != nil)
        {
            return NewCString(output);
        }
    }
    return 0;
}


