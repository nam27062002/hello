#import "iOSNativeShare.h"

@implementation iOSNativeShare{
}

#ifdef UNITY_4_0 || UNITY_5_0

#import "iPhone_View.h"

#else

extern UIViewController* UnityGetGLViewController();

#endif

+(id) withTitle:(char*)title withMessage:(char*)message{
    
    return [[iOSNativeShare alloc] initWithTitle:title withMessage:message];
}

-(id) initWithTitle:(char*)title withMessage:(char*)message{
    
    self = [super init];
    
    if( !self ) return self;
    
    ShowAlertMessage([[NSString alloc] initWithUTF8String:title], [[NSString alloc] initWithUTF8String:message]);
    
    return self;
    
}

void ShowAlertMessage (NSString *title, NSString *message){
    
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:title
                          
                                                    message:message
                          
                                                   delegate:nil
                          
                                          cancelButtonTitle:@"OK"
                          
                                          otherButtonTitles: nil];
    
    [alert show];
    
}

+(id) withText:(char*)text withURL:(char*)url withImage:(char*)image withSubject:(char*)subject{
    
    return [[iOSNativeShare alloc] initWithText:text withURL:url withImage:image withSubject:subject];
}

-(id) initWithText:(char*)text withURL:(char*)url withImage:(char*)image withSubject:(char*)subject{
    
    self = [super init];
    
    if( !self ) return self;
    
    
    
    NSString *mText = [[NSString alloc] initWithUTF8String:text];
    
    NSString *mUrl = [[NSString alloc] initWithUTF8String:url];
    
    NSString *mImage = [[NSString alloc] initWithUTF8String:image];
    
    NSString *mSubject = [[NSString alloc] initWithUTF8String:subject];
    
    
    NSMutableArray *items = [NSMutableArray new];
    
    if(mText != NULL && mText.length > 0){
        
        [items addObject:mText];
        
    }
    
    if(mUrl != NULL && mUrl.length > 0){
        
        NSURL *formattedURL = [NSURL URLWithString:mUrl];
        
        [items addObject:formattedURL];
        
    }
    if(mImage != NULL && mImage.length > 0){
        if([mImage hasPrefix:@"http"])
        {
            NSURL *urlImage = [NSURL URLWithString:mImage];
            
            NSData *dataImage = [NSData dataWithContentsOfURL:urlImage];
            
            UIImage *imageFromUrl = [UIImage imageWithData:dataImage];
            
            [items addObject:imageFromUrl];
        }else{
            NSFileManager *fileMgr = [NSFileManager defaultManager];
            if([fileMgr fileExistsAtPath:mImage]){
                
                NSData *dataImage = [NSData dataWithContentsOfFile:mImage];
                
                UIImage *imageFromUrl = [UIImage imageWithData:dataImage];
                
                [items addObject:imageFromUrl];
            }else{
                ShowAlertMessage(@"Error", @"Cannot find image");
            }
        }
    }
    
    UIActivityViewController *activity = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:Nil];
    UIViewController *rootViewController = UnityGetGLViewController();
    
    if ( [activity respondsToSelector:@selector(popoverPresentationController)] ) {
        // iOS8
        activity.popoverPresentationController.sourceView = rootViewController.view;
    }
    
    [activity setValue:mSubject forKey:@"subject"];
    [rootViewController presentViewController:activity animated:YES completion:Nil];
    
    return self;
}

# pragma mark - C API
iOSNativeShare* instance;

void showAlertMessage(struct ConfigStruct *confStruct) {
    instance = [iOSNativeShare withTitle:confStruct->title withMessage:confStruct->message];
}

void showSocialSharing(struct SocialSharingStruct *confStruct) {
    instance = [iOSNativeShare withText:confStruct->text withURL:confStruct->url withImage:confStruct->image withSubject:confStruct->subject];
}

@end