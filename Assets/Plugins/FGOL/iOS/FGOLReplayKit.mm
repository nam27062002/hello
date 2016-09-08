//
//  ReplayKit.m
//  Unity-iPhone
//
//  Created by Tristan Cartledge on 05/11/2015.
//
//

#import "FGOLReplayKit.h"

@implementation FGOLReplayKit

- (id)init
{
    self = [super init];
    
    m_previewViewOpen = false;
    m_callbackGameObjName = nil;
    m_previewViewController = nil;
    
    return self;
}

- (void)InitReplayKit
{
    RPScreenRecorder* recorder = RPScreenRecorder.sharedRecorder;
    recorder.delegate = self;
}

- (bool)IsSupported
{
    bool supported =  ([[NSProcessInfo processInfo] respondsToSelector:@selector(operatingSystemVersion)]
                       && [NSProcessInfo processInfo].operatingSystemVersion.majorVersion >= 9
                       && RPScreenRecorder.sharedRecorder.available);
    
    NSLog(@"FGOLReplayKit (IsSupported) :: supported - %d", supported);
    
    return supported;
}

- (bool)IsRecordingAvailable
{
    return m_previewViewController != nil;
}

- (void)StartRecording:(NSString*)gameObjectName useMicrophone:(bool)useMicrophone
{
    NSLog(@"FGOLReplayKit (StartRecording) :: gameObjectName - %@", gameObjectName);
    
    RPScreenRecorder* recorder = RPScreenRecorder.sharedRecorder;
    
    [recorder startRecordingWithMicrophoneEnabled:useMicrophone handler:^(NSError * _Nullable error) {
        const char * success = "false";
        
        if (error)
        {
            NSLog(@"FGOLReplayKit (StartRecording) :: Error - %@", error);
        }
        else
        {
            success = "true";
        }
        
        UnitySendMessage([gameObjectName UTF8String], "OnStartRecording", success);
    }];
}

- (void)StopRecording:(NSString *)gameObjectName
{
    m_callbackGameObjName = gameObjectName;
    
    RPScreenRecorder * recorder = RPScreenRecorder.sharedRecorder;
    
    [recorder stopRecordingWithHandler:^(RPPreviewViewController * _Nullable previewViewController, NSError * _Nullable error)
    {
        bool success = false;
        
        if (error)
        {
            NSLog(@"FGOLReplayKit (StopRecording) :: Error - %@", error);
        }
        else if (previewViewController)
        {
            m_previewViewController = previewViewController;
            success = true;
        }
        
        UnitySendMessage([gameObjectName UTF8String], "OnStopRecording", success ? "true" : "false");

    }];
}

- (void)ShowRecording:(NSString*)gameObjectName
{
    if (m_previewViewController != nil)
    {
        m_previewViewController.previewControllerDelegate = self;
        [m_previewViewController setModalPresentationStyle:UIModalPresentationFullScreen];
    
        [[[UnityGetGLView() window] rootViewController] presentViewController:m_previewViewController animated:true completion:^{
            NSLog(@"FGOLReplayKit (ShowRecording) :: Preview controller shown!");
            m_previewViewOpen = true;
        }];
    }
    else
    {
        NSLog(@"FGOLReplayKit (ShowRecording) :: Error - No preview controller available! Assuming no recording!");
        UnitySendMessage([gameObjectName UTF8String], "OnShowRecording", "false");
    }
}

-(void)DiscardRecording:(NSString *)gameObjectName
{
    NSLog(@"FGOLReplayKit (DiscardRecording) :: Discard Recording!");
    
    RPScreenRecorder * recorder = RPScreenRecorder.sharedRecorder;
    
    //There is a bug where the handler is never called
    //TODO use the handler to reset the preview controller when bug is fixed
    [recorder discardRecordingWithHandler:^{
        NSLog(@"FGOLReplayKit (DiscardRecording) :: Recording discarded!");
    }];
    
    m_previewViewController = nil;
    UnitySendMessage([gameObjectName UTF8String], "OnDiscardRecording", "true");
}

- (void)screenRecorder:(RPScreenRecorder *)screenRecorder didStopRecordingWithError:(NSError *)error previewViewController:(RPPreviewViewController *)previewViewController
{
    if (error)
    {
        NSLog(@"FGOLReplayKit (screenRecorderdidStopRecordingWithError) :: Error - %@", error);
    }
}

- (void)screenRecorderDidChangeAvailability:(RPScreenRecorder *)screenRecorder
{
    NSLog(@"FGOLReplayKit (screenRecorderDidChangeAvailability) :: availability - %d", screenRecorder.available);
}

- (void)previewControllerDidFinish:(RPPreviewViewController *)previewController
{
    if (previewController != nil)
    {
        dispatch_async(dispatch_get_main_queue(), ^{
           [previewController dismissViewControllerAnimated:true completion:^{
               NSLog(@"FGOLReplayKit (previewControllerDidFinish) :: preview controller dismissed");
               
               if (m_previewViewOpen && m_callbackGameObjName != nil)
               {
                   m_previewViewOpen = false;
                   UnitySendMessage([m_callbackGameObjName UTF8String], "OnShowRecording", "true");
                   m_callbackGameObjName = nil;
               }
           }];
        });
    }
}

@end

static FGOLReplayKit* s_replayKitInstance = nil;

extern "C"
{
    void _InitReplayKit()
    {
        if (s_replayKitInstance == nil)
        {
            s_replayKitInstance = [[FGOLReplayKit alloc] init];
        }
        
        [s_replayKitInstance InitReplayKit];
    }
    
    bool _IsSupported()
    {
        bool supported = false;
        
        if (s_replayKitInstance != nil)
        {
            supported = [s_replayKitInstance IsSupported];
        }
        else
        {
            NSLog(@"FGOLReplayKit (_IsSupported) :: Replay Kit Not Initialized");
        }
        
        return supported;
    }
    
    bool _IsRecordingAvailable()
    {
        bool available = false;
        
        if (s_replayKitInstance != nil)
        {
            available = [s_replayKitInstance IsRecordingAvailable];
        }
        else
        {
            NSLog(@"FGOLReplayKit (_IsRecordingAvailable) :: Replay Kit Not Initialized");
        }
        
        return available;
    }
    
    void _StartRecording(const char* gameObjectName, bool useMicrophone)
    {
        NSString* gameObjName = [[NSString alloc] initWithUTF8String:gameObjectName];
        
        if (s_replayKitInstance != nil)
        {
            [s_replayKitInstance StartRecording:gameObjName useMicrophone:useMicrophone];
        }
        else
        {
            NSLog(@"FGOLReplayKit (_StartRecording) :: Replay Kit Not Initialized");
            UnitySendMessage([gameObjName UTF8String], "OnStartRecording", "false");
        }
    }
    
    void _StopRecording(const char* gameObjectName)
    {
        NSString* gameObjName = [[NSString alloc] initWithUTF8String:gameObjectName];
        
        if (s_replayKitInstance != nil)
        {
            [s_replayKitInstance StopRecording:gameObjName];
        }
        else
        {
            NSLog(@"FGOLReplayKit (_StopRecording) :: Replay Kit Not Initialized");
            UnitySendMessage([gameObjName UTF8String], "OnStopRecording", "false");
        }
    }
    
    void _ShowRecording(const char* gameObjectName)
    {
        NSString* gameObjName = [[NSString alloc] initWithUTF8String:gameObjectName];
        
        if (s_replayKitInstance != nil)
        {
            [s_replayKitInstance ShowRecording:gameObjName];
        }
        else
        {
            NSLog(@"FGOLReplayKit (_ShowRecording) :: Replay Kit Not Initialized");
            UnitySendMessage([gameObjName UTF8String], "OnStopRecording", "false");
        }
    }

    void _DiscardRecording(const char* gameObjectName)
    {
        NSString* gameObjName = [[NSString alloc] initWithUTF8String:gameObjectName];
        
        if (s_replayKitInstance != nil)
        {
            [s_replayKitInstance DiscardRecording:gameObjName];
        }
        else
        {
            NSLog(@"FGOLReplayKit (_DiscardRecording) :: Replay Kit Not Initialized");
            UnitySendMessage([gameObjName UTF8String], "OnDiscardRecording", "false");
        }
    }
}