#import "UnityAppController.h"

// https://stackoverflow.com/questions/13551042/different-data-for-sharing-providers-in-uiactivityviewcontroller
// 
@interface MyActivityItemProvider : UIActivityItemProvider
@end

@implementation MyActivityItemProvider

- (id)item
{
    // Return nil, if you don't want this provider to apply
    // to a particular activity type (say, if you provide
    // print data as a separate item for UIActivityViewController).
    if ([self.activityType isEqualToString:UIActivityTypePostToFacebook])
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
