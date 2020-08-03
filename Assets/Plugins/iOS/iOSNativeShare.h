#import "UnityAppController.h"

// https://stackoverflow.com/questions/13551042/different-data-for-sharing-providers-in-uiactivityviewcontroller
// 
@interface MyActivityItemProvider : UIActivityItemProvider
@property BOOL hasImage;
@end

@implementation MyActivityItemProvider

- (id)item
{
    // Return nil, if you don't want this provider to apply
    // to a particular activity type (say, if you provide
    // print data as a separate item for UIActivityViewController).
        // Remove Facebook Text because it goes against the guidelines
    if ([self.activityType isEqualToString:UIActivityTypePostToFacebook])
        return nil;
    
    // Do not sava an image with text only when saving to files
    if ([self.activityType isEqualToString:@"com.apple.CloudDocsUI.AddToiCloudDrive"] && self.hasImage)
           return nil;
    
        // In Whatsapp if we have an image we remove the text or it will not show
    if ([self.activityType isEqualToString:@"net.whatsapp.WhatsApp.ShareExtension"] && self.hasImage)
        return nil;
    
    return self.placeholderItem;
}


@end


@interface iOSNativeShare : UIViewController
{
    UINavigationController *navController;
}


struct ConfigStruct {
    char* title;
    char* message;
};

struct SocialSharingStruct {
    char* text;
    char* subject;
	char* filePaths;
};


#ifdef __cplusplus
extern "C" {
#endif
    
    void showAlertMessage(struct ConfigStruct *confStruct);
    void showSocialSharing(struct SocialSharingStruct *confStruct);
    
#ifdef __cplusplus
}
#endif


@end
