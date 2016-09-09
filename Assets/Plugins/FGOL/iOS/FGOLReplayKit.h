#import <Foundation/Foundation.h>
#import <ReplayKit/ReplayKit.h>

@interface FGOLReplayKit : NSObject <RPScreenRecorderDelegate, RPPreviewViewControllerDelegate>
{
    bool m_previewViewOpen;
    NSString* m_callbackGameObjName;
    RPPreviewViewController * m_previewViewController;
}

- (void)InitReplayKit;
- (bool)IsSupported;
- (bool)IsRecordingAvailable;
- (void)StartRecording:(NSString*)gameObjectName useMicrophone:(bool)useMicrophone;
- (void)StopRecording:(NSString*)gameObjectName;
- (void)ShowRecording:(NSString*)gameObjectName;
- (void)DiscardRecording:(NSString*)gameObjectName;

@end